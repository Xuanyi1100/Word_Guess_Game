using System;

namespace server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpServer server = new TcpServer(1); // use header code = 1
            server.Start();
            Console.WriteLine("Press Enter to stop server");
            Console.ReadLine();
            server.Stop();
        }
    }
}