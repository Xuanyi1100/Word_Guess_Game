/*
 * FILE          : Program.cs
 * PROJECT       : PROG2121 - Assignment #4
 * PROGRAMMER    : Tian Yang
 * FIRST VERSION : 2024-11-06
 * DESCRIPTION   : 
 *   This file contains the main entry point of the server application. It initializes the 
 *   server, starts the listener in a separate task, and handles user commands to control 
 *   the server's lifecycle, including shutting down active games.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace testserver2
{
    /*
     * CLASS        : Program
     * DESCRIPTION  : 
     *   The `Program` class initializes the server and manages the lifecycle of the application.
     *   It handles user commands, such as stopping the server and notifying active games of shutdown.
     */
    internal class Program
    {
        static bool quit = false;
        static void Main(string[] args)
        {
            Server server = new Server();

            Task serverTask = Task.Run(server.StartListener);
            while (!quit)
            {
                string command = Console.ReadLine();
                if (command == "STOP")
                {
                    quit = true;

                    foreach (Game game in Session.AllGames)
                    {
                        game.NotifyShutdown();
                    }
                }
            }
        }
    }
}
