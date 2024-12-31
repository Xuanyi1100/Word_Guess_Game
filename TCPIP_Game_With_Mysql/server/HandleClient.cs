// server/HandleClient.cs
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;
using server.Helper;
using System.IO;
using System.Net;
using System.Data.Entity;
using server.Models;

namespace server
{
    internal class HandleClient
    {
        private NetworkStream stream;
        new ReceiveResult receiveResult;
        Game game = null;

        // Create a dictionary with string keys and Game values, use to store different games
        // Made this dictionary static, so all client can access it, otherwise the lifecirle of gameDict is gone after one connection
        internal static Dictionary<string, Game> gameDict = new Dictionary<string, Game>();

        // Database context for persistent storage
        private GameDataContext dbContext;
        internal HandleClient(TcpClient client)
        {
            // Initialize database context
            using (dbContext = new GameDataContext())
            {
                stream = client.GetStream();
                receiveResult = StartReceivingAsync(client, stream);

                ClientMessage clientMessage;
                clientMessage = receiveResult.ClientMessage;

                Console.WriteLine("Received Code: {0}", receiveResult.HeaderCode);
                Console.WriteLine("Received UserGuess: {0}\n", clientMessage.UserGuess);

                // handle SessionId
                string sessionId = null;
                if (clientMessage.SessionId == null)
                {
                    sessionId = Guid.NewGuid().ToString();
                    game = new Game();
                    gameDict.Add(sessionId, game);
                }
                else
                {
                    sessionId = clientMessage.SessionId;
                    game = gameDict[sessionId];
                }

                // handle ClientPort, need delete after database ???
                // pass ip and port data to Game, using for send shut down message to client
                game.ClientAddress = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
                game.ClientPort = clientMessage.ClientPort;

                // handle User in database:
                // Find or create user
                var user = dbContext.Users.FirstOrDefault(u => u.UserName == clientMessage.Username)
                    ?? new User
                    {
                        UserName = clientMessage.Username,
                        UserIP = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString(),
                        UserPort = clientMessage.ClientPort
                    };

                // Save new user if not exists
                if (user.UserID == 0)
                {
                    dbContext.Users.Add(user);
                    dbContext.SaveChanges();
                }

                // handle HeaderCode and pass UserGuess to Game
                int responseHeaderCode = -1; // Fallback value for unexpected cases;          
                switch (receiveResult.HeaderCode)
                {
                    case 0:
                        game.Start();
                        responseHeaderCode = 0;
                        break;
                    case 1:
                        game.Guess(clientMessage.UserGuess);
                        responseHeaderCode = game.Found ? 1 : 2;
                        if (game.RemainNumberToGuess == 0)
                        {
                            responseHeaderCode = 4;
                        }
                        break;
                    case 2:
                        // user wants to quit, ask if they really want to quit
                        responseHeaderCode = 3;
                        break;
                    case 3:
                        // User confirm quit, game ends, remove Game object
                        gameDict.Remove(sessionId);
                        break;
                    case 4:
                        // User choose to go on, server do nothing
                        break;
                    case 5:
                        // User wants to play again
                        gameDict.Remove(sessionId);
                        // no need to start here, client will connect again, will go to case 0
                        //game.Start();
                        responseHeaderCode = 0;
                        break;
                    default:
                        break;
                }


                // instantiate the object before attempting to access its properties
                ServerMessage message = new ServerMessage();
                // send data back
                message.SessionId = sessionId;
                message.CharacterString = game.GuessString;
                message.TotalWords = game.TotalNumberToGuess;
                message.WordsFound = game.RemainNumberToGuess;
                SendDataAsync(message, responseHeaderCode, client, stream);

                Console.WriteLine("Sent Code: {0}", responseHeaderCode);
                Console.WriteLine($"Sent Message:\n " +
                    $"SessionId: {message.SessionId}\n " +
                    $"CharacterString: {message.CharacterString}\n " +
                    $"TotalWords: {message.TotalWords}\n " +
                    $"WordsFound: {message.WordsFound}\n\n");


                // close connection after each response.
                stream.Close();
                client.Close();
            }
            
        }

        // tcp receive method, same as the method in the client TcpClientService.cs 
        public ReceiveResult StartReceivingAsync(TcpClient client, NetworkStream stream)
        {           
            if (client?.Connected != true || stream == null)
            {
                Console.Error.WriteLine("Not connected to server!");
                return null;
            }

            try
            {
                Header header = new Header();
                byte[] headerData = new byte[header.GetLength()];
                if (stream.Read(headerData, 0, headerData.Length) != headerData.Length)
                {
                    Console.Error.WriteLine("Header invalid");
                    return new ReceiveResult { HeaderCode = -1, ClientMessage = null };
                }

                header.code = headerData[0];
                int headerCode = header.code;
                header.length = BitConverter.ToInt32(headerData, 1);

                // if no message sent, just return the header code
                if (header.length == 0)
                    return new ReceiveResult { HeaderCode = headerCode, ClientMessage = null };
                // if there's message, pare message
                byte[] messageData = new byte[header.length];

                int count;
                count =  stream.Read(messageData, 0, messageData.Length);
                // The Encoding.ASCII.GetBytes method expects a non-null string as input. If message is null ,
                // it throws an ArgumentNullException, that's why we check (header.length == 0) first
                string json = Encoding.ASCII.GetString(messageData, 0, count);
                ClientMessage clientMessage = JsonConvert.DeserializeObject<ClientMessage>(json);

                return new ReceiveResult { HeaderCode = headerCode, ClientMessage = clientMessage };
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"IO Error while receiving data: {ex.Message}");
                return null;
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"Receiving data canceled: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while receiving data: {ex.Message}");
                return null;
            }
        }

        // tcp send method, same as the method in the client TcpClientService.cs 
        public bool SendDataAsync(ServerMessage message, int headerCode, TcpClient client, NetworkStream stream)
        {
            if (client?.Connected != true || stream == null)
            {
                Console.Error.WriteLine("Not connected to server!");
                return false;
            }
            try
            {
                string json = JsonConvert.SerializeObject(message);
                byte[] data = string.IsNullOrEmpty(json) ? Array.Empty<byte>() : Encoding.ASCII.GetBytes(json);
                Header header = new Header
                {
                    code = (byte)headerCode,
                    length = data.Length
                };

                stream.Write(header.GetBytes(), 0, header.GetLength());
                stream.Write(data, 0, data.Length);
                return true;
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"IO error while sending data: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while sending data: {ex.Message}");
                return false;
            }
        }

    }
}

