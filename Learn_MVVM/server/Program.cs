// server/Program.cs
using System;

namespace server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Listener listener = new Listener();
            listener.StartListening();

            // stop server manually
            Console.WriteLine("Input 'stop' to stop server:");
            Console.ReadLine();

        }
    }
}