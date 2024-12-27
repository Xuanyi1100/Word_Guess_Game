// server/Listener.cs
using System.Net.Sockets;
using System.Net;

namespace server
{
    public class Listener
    {
        private TcpListener _listener = null;
        public int Port = 51717;
        public void StartListening()
        {
            try
            {

                _listener = new TcpListener(IPAddress.Any, Port);
                _listener.Start();
                Console.WriteLine($"Server started listening on port {Port}");

                while (true)
                {
                    // need task

                    TcpClient client = _listener.AcceptTcpClient();
                    Console.WriteLine($"Client connected from {client.Client.RemoteEndPoint}");

                    Task.Run(() =>
                    {
                        new HandleClient(client);
                    });
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
