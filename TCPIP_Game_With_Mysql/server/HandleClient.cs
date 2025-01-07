// server/HandleClient.cs
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;
using server.Helper;
using System.Net;
using server.Models;

namespace server
{
    internal class HandleClient
    {
        private NetworkStream stream;
        new ReceiveResult receiveResult;

        // Database context for persistent storage
        private GameDataContext dbContext;
        internal HandleClient(TcpClient client)
        {
            // instantiate the object before attempting to access its properties
            ServerMessage message = new ServerMessage();          

            // Initialize database context
            using (dbContext = new GameDataContext())
            {
                stream = client.GetStream();
                receiveResult = StartReceivingAsync(client, stream);

                ClientMessage clientMessage;
                clientMessage = receiveResult.ClientMessage;

                Console.WriteLine("Received Code: {0}", receiveResult.HeaderCode);
                Console.WriteLine("Received UserGuess: {0}\n", clientMessage.UserGuess);
             
                User user = dbContext.Users.FirstOrDefault(u => u.UserName == clientMessage.Username);
                GameSession session = dbContext.Sessions.FirstOrDefault(u => u.SessionID == clientMessage.SessionId);
                
                if (session == null)    // if null, it's a new game, find a new gamestring
                {
                    // create user
                    if(user == null)
                    {
                        user = new User
                        {
                            UserName = clientMessage.Username,
                            UserIP = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString(),
                            UserPort = clientMessage.ClientPort
                        };
                        dbContext.Users.Add(user);
                        dbContext.SaveChanges();
                    }


                    // get GuessString from database, randomly
                    var randomGameString = dbContext.Database
                        .SqlQuery<GameString>("SELECT GameStringID, GameStringText FROM GameString ORDER BY RAND() LIMIT 1;")
                        .FirstOrDefault();

                    // calculate TotalWords from database
                    var gameWordCount = dbContext.GameStringGameWords
                        .Join(dbContext.GameStrings,
                            gsgw => gsgw.GameStringID,
                            gs => gs.GameStringID,
                            (gsgw, gs) => new { gsgw, gs })
                        .Where(x => x.gs.GameStringText == randomGameString.GameStringText)
                        .Count();

                    message.CharacterString = randomGameString.GameStringText;
                    message.TotalWords = gameWordCount;
                    message.WordsToFound = gameWordCount;

                    session = new GameSession
                    {
                        SessionID = Guid.NewGuid().ToString(),
                        UserID = user.UserID, 
                        Status = "active",   
                        GameStringID = randomGameString.GameStringID,
                        WordsToFound = gameWordCount,
                        TotalWords = gameWordCount,
                        StartTime = DateTime.Now
                    };
                    // Save new session
                    dbContext.Sessions.Add(session);
                    dbContext.SaveChanges();
                }
                else
                {
                    var gameString = dbContext.GameStrings
                        .FirstOrDefault(gs => gs.GameStringID == session.GameStringID);
                    message.CharacterString = gameString.GameStringText;
                    message.TotalWords = session.TotalWords;
                    message.WordsToFound = session.WordsToFound;
                }

                message.SessionId = session.SessionID;

                // handle HeaderCode 
                int responseHeaderCode = -1; // Fallback value for unexpected cases;          
                switch (receiveResult.HeaderCode)
                {
                    case 0:                       
                        responseHeaderCode = 0;
                        break;
                    case 1:
                        // calculate WordsToFound from database
                        int guessWordCount = dbContext.SessionGuessWords
                            .Where(sgw => sgw.GameSessionID == session.GameSessionID).Count();

                        // check if guess is in GameStringGameWord, and not in SessionGuessWord
                        // if so, it's a right guess, then put it in SessionGuessWord
                        // if not, it's a wrong guess
                        // Check if the guess exists in GameStringGameWord

                        // Step 1: Find the GameWordID for the guessed word
                        var gameWordId = dbContext.GameWords
                            .Where(gw => gw.GameWordText == clientMessage.UserGuess)
                            .Select(gw => gw.GameWordID)
                            .FirstOrDefault();

                        if (gameWordId != 0)
                        {
                            // Step 2: Check if the word is in GameStringGameWord for the current GameString
                            var isWordInGameString = dbContext.GameStringGameWords
                                .Any(gsgw => gsgw.GameStringID == session.GameStringID && gsgw.GameWordID == gameWordId);

                            if (isWordInGameString)
                            {
                                // Step 3: Check if the word has already been guessed in this session
                                var alreadyGuessed = dbContext.SessionGuessWords
                                    .Any(sgw => sgw.GameSessionID == session.GameSessionID && sgw.GuessWord == clientMessage.UserGuess);

                                if (!alreadyGuessed)
                                {
                                    // Step 4: Add the correct guess to SessionGuessWord
                                    dbContext.SessionGuessWords.Add(new SessionGuessWord
                                    {
                                        GameSessionID = session.GameSessionID,
                                        GuessWord = clientMessage.UserGuess
                                    });
                                    session.WordsToFound--;
                                    dbContext.SaveChanges(); // update the database

                                    message.WordsToFound = session.WordsToFound;
                                    // check if win:
                                    if (session.WordsToFound == 0)
                                    {
                                        session.Status = "win";
                                        session.EndTime = DateTime.Now;
                                        dbContext.SaveChanges();
                                        responseHeaderCode = 4; // User win
                                        break;
                                    }
                                    responseHeaderCode = 1; // Correct guess
                                }
                                else
                                {
                                    responseHeaderCode = 2; // Wrong guess
                                }
                            }
                            else
                            {
                                responseHeaderCode = 2; // Wrong guess
                            }
                        }
                        else
                        {
                            responseHeaderCode = 2; // Wrong guess
                        }

                        break;
                    case 2:
                        // user wants to quit, ask if they really want to quit
                        responseHeaderCode = 3;
                        break;
                    case 3:
                        // User confirm quit, game ends
                        session.Status = "Quit";
                        session.EndTime = DateTime.Now;
                        dbContext.SaveChanges();
                        break;
                    case 4:
                        // User choose to go on, server do nothing
                        break;
                    case 5:
                        // User wants to play again
                        responseHeaderCode = 0;
                        break;
                    case 6:
                        // time out user lose
                        responseHeaderCode = 0;
                        session.Status = "Lose";
                        session.EndTime = DateTime.Now;
                        dbContext.SaveChanges();
                        break;
                    default:
                        break;
                }
     
                SendDataAsync(message, responseHeaderCode, client, stream);

                Console.WriteLine("Sent Code: {0}", responseHeaderCode);
                Console.WriteLine($"Sent Message:\n " +
                    $"SessionId: {message.SessionId}\n " +
                    $"CharacterString: {message.CharacterString}\n " +
                    $"TotalWords: {message.TotalWords}\n " +
                    $"WordsFound: {message.WordsToFound}\n\n");


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

