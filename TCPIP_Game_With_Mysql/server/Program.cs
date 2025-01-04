// server/Program.cs
using System;
using System.Threading;
using System.Threading.Tasks;

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


    }
}