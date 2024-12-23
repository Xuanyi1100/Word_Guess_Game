/*
 * FILE          : Game.cs
 * PROJECT       : PROG2121 - Assignment #4
 * PROGRAMMER    : Tian Yang
 * FIRST VERSION : 2024-11-06
 * DESCRIPTION   : 
 *   This file defines the `Game` class, which represents an individual game instance.
 *   It manages the game state, validates guesses, and tracks progress.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Resources;
using System.ComponentModel;
using System.Net;

namespace testserver2
{
    /*
     * CLASS        : Game
     * DESCRIPTION  : 
     *   Represents the state and logic for a single game instance. Handles client interactions,
     *   such as starting a game, validating guesses, and tracking progress.
     */
    internal class Game
    {
        internal string GuessString => guessString;
        internal int TotalNumberToGuess => totalNumberToGuess;
        internal int RemainNumberToGuess => remainNumberToGuess;
        internal bool Found => found;
        internal bool Repeated => repeated;

        internal string ClientAddress { get; set; }
        internal int ClientPort { get; set; }

        internal Game()
        {
        }

        private string guessString = null;
        private int totalNumberToGuess = 0;
        private int remainNumberToGuess = 0;
        private HashSet<string> answerSet = null;
        private HashSet<string> wordSet = null;

        private bool found = false;
        private bool repeated = false;

        /*
         * METHOD       : Start
         * DESCRIPTION  : 
         *   Initializes the game by reading configuration files and setting up the initial state.
         */
        internal void Start()
        {
            //string[] lines = Properties.Resources.test.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] files = Directory.GetFiles("gameConfig");
            Random rand = new Random(DateTime.Now.Second);
            string filename = files[rand.Next(files.Length)];
            string[] lines = File.ReadAllLines(filename);

            guessString = lines[0];
            totalNumberToGuess = Convert.ToInt32(lines[1]);
            remainNumberToGuess = totalNumberToGuess;
            answerSet = new HashSet<string>(lines.Skip(2));
            wordSet = new HashSet<string>(answerSet.Count);
        }

        /*
         * METHOD       : Guess
         * DESCRIPTION  : 
         *   Validates a user's guess and updates the game state accordingly.
         */
        internal void Guess(string userguess)
        {
            found = answerSet.Contains(userguess);
            repeated = wordSet.Contains(userguess);
            if (found && !repeated)
            {
                wordSet.Add(userguess);
                remainNumberToGuess--;
            }
        }

        internal void NotifyShutdown()
        {
            try
            {
                TcpClient client = new TcpClient(ClientAddress, ClientPort);
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
