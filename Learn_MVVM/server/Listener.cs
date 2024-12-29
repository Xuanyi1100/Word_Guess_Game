// server/Listener.cs
using System.Net.Sockets;
using System.Net;
using System.Configuration;

namespace server
{
    public class Listener
    {
        private TcpListener _listener = null;       
        public int Port;
     
    public void StartListening(CancellationToken token)
        {
            try
            {
                // Read the port value from app.config
                string portValue = ConfigurationManager.AppSettings["Port"];
                if (!int.TryParse(portValue, out Port))
                {
                    Console.WriteLine("Invalid port value in app.config. Using default port 51717.");
                    Port = 51717; // Default port
                }

                _listener = new TcpListener(IPAddress.Any, Port);
                _listener.Start();
                Console.WriteLine($"Server started listening on port {Port}");

                while (!token.IsCancellationRequested)
                {

                    // _listener.AcceptTcpClient() will block the loop, wait until next client comes,
                    // that makes the token check unuse, so we need .Pending(),
                    // that means if no client come for now, go other branch, sleep this thread then check token again.

                    if (_listener.Pending())
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        Console.WriteLine($"Client connected from {client.Client.RemoteEndPoint}");
                        Task.Run(() => new HandleClient(client));
                    }
                    else
                    {
                        Thread.Sleep(100); // Avoid busy-waiting
                    }

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"Error {e.Message}");
            }
            finally
            {
                _listener?.Stop();
                Console.WriteLine("Server Stopped");
            }
        }

    }
}
