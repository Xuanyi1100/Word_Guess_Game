using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
// The following code is extracted from the MSDN site:
//https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener?view=net-5.0
//
namespace TCPIPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Listener listener = new Listener();
            listener.StartListener();

            Console.WriteLine("Press Enter to End");
            Console.ReadLine();
        }

    }
}
