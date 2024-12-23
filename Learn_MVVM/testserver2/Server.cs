/*
 * FILE          : Server.cs
 * PROJECT       : PROG2121 - Assignment #4
 * PROGRAMMER    : Tian Yang
 * FIRST VERSION : 2024-11-06
 * DESCRIPTION   : 
 *   This file defines the `Server` class responsible for managing TCP connections, listening
 *   for incoming client connections, and spawning `Session` instances for each connection.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Resources;
using System.Configuration;
using static System.Collections.Specialized.BitVector32;

namespace testserver2
{
    /*
     * CLASS        : Server
     * DESCRIPTION  : 
     *   The `Server` class listens for client connections on a specified port. When a client 
     *   connects, it spawns a new `Session` to handle the interaction.
     */
    internal class Server
    {
        private TcpListener listener = null;
        private int port = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);

        /*
         * METHOD       : StartListener
         * DESCRIPTION  : 
         *   Initializes the TCP listener on the configured port and handles incoming client connections.
         */
        internal void StartListener()
        {
            try
            {
                IPAddress localAddress = IPAddress.Parse("0.0.0.0");
                listener = new TcpListener(localAddress, port);
                listener.Start();

                while (true)
                {
                    Console.WriteLine("Waiting for a connection... ");
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("There is a client connected!");

                    Task task = Task.Factory.StartNew(() =>
                    {
                        new Session(client);
                    });
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
