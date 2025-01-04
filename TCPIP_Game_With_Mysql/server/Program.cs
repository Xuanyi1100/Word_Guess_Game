// server/Program.cs
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using server.Helper;

namespace server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Listener listener = new Listener();
            // The listening operation would block the console input, so it has to run on a separate task 
            Task listeningTask = Task.Run(() => listener.StartListening(cts.Token));

            Console.WriteLine("Type 'stop' to shut down the server.");


            while (true)
            {
                string command = Console.ReadLine();
                if (command.ToLower() == "stop")
                {
                    // Access the user whose session Status is active and notify shut down
                    using (var dbContext = new GameDataContext())
                    {
                        // Find all active sessions
                        var activeSessions = dbContext.Sessions
                            .Where(session => session.Status == "active")
                            .ToList();

                        // Notify each user with an active session
                        foreach (var session in activeSessions)
                        {
                            // Get the associated user
                            var user = dbContext.Users
                                .FirstOrDefault(u => u.UserID == session.UserID);

                            if (user != null)
                            {
                                NotifyShutdown(user.UserIP, user.UserPort);
                            }
                        }
                    }

                    // shutdown server gracefully after notifying clients
                    cts.Cancel(); // Signal cancellation

                    // Wait for the listening task to finish
                    await listeningTask;

                    break;
                }
                else
                {                   
                    continue;
                }
            }


        }
        static void NotifyShutdown(string clientAddress, int clientPort)
        {
            try
            {
                TcpClient client = new TcpClient(clientAddress, clientPort);
                NetworkStream netStream = client.GetStream();
                Header header = new Header();
                header.code = 0x05;
                header.length = 0;

                netStream.Write(header.GetBytes(), 0, header.GetLength());

                netStream.Close();
                client.Close();
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }


    }
}