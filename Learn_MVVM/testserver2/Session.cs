/*
 * FILE          : Session.cs
 * PROJECT       : PROG2121 - Assignment #4
 * PROGRAMMER    : Tian Yang
 * FIRST VERSION : 2024-11-06
 * DESCRIPTION   : 
 *   This file defines the `Session` class, which handles communication with a single client.
 *   It manages the session's lifecycle, processes requests, and sends responses.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace testserver2
{
    /*
     * STRUCT       : Header
     * DESCRIPTION  : 
     *   Represents the protocol header for communication, including the request code and 
     *   payload length.
     */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public byte code;
        public int length;
    }

    public static class HeaderExtension
    {
        public static byte[] GetBytes(this Header header)
        {
            int size = Marshal.SizeOf(header);
            byte[] data = new byte[size];
            IntPtr pointer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(header, pointer, true);
            Marshal.Copy(pointer, data, 0, size);
            Marshal.FreeHGlobal(pointer);
            return data;
        }

        public static int GetLength(this Header header)
        {
            return Marshal.SizeOf(header);
        }
    }
    internal class SentBody
    {
        public string SessionID { get; set; }
        public string LongString { get; set; }
        public int TotalNumber { get; set; }
        public int NumberOfWordsToFind { get; set; }
    }

    internal class ReceivedBody
    {
        public string username { get; set; }
        public string sessionid { get; set; }
        public int requestcode { get; set; }
        public string userguess { get; set; }
        public int clientport { get; set; }
    }

    /*
     * CLASS        : Session
     * DESCRIPTION  : 
     *   Handles the client-server communication for a single session. It manages requests, 
     *   game states, and sends responses.
     */
    internal class Session
    {
        public static Game[] AllGames => gameDict.Values.ToArray();

        private static Dictionary<string, Game> gameDict = new Dictionary<string, Game>();

        private NetworkStream netStream;

        internal Session(TcpClient client)
        {
            netStream = client.GetStream();
            Game game = null;
            string sessionId = null;
            if (Receive(out Header header, out string message))
            {
                Console.WriteLine("Code: {0}", header.code);
                Console.WriteLine("Legth: {0}", header.length);
                Console.WriteLine("Received: {0}", message);

                ReceivedBody receivedJSON = JsonSerializer.Deserialize<ReceivedBody>(message);
                sessionId = receivedJSON.sessionid;
                if (receivedJSON.sessionid == null)
                {
                    sessionId = Guid.Empty.ToString();
                }
                try
                {
                    game = gameDict[sessionId];
                }
                catch (KeyNotFoundException)
                {
                    sessionId = Guid.NewGuid().ToString();
                    game = new Game();
                    gameDict.Add(sessionId, game);
                }
                game.ClientAddress = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
                game.ClientPort = receivedJSON.clientport;

                SentBody sent = new SentBody();
                Header responseHeader = new Header();
                string responseJSON = null;
                switch (header.code)
                {
                    case 0x00:
                        game.Start();
                        responseHeader.code = 0x00;
                        break;
                    case 0x01:
                        game.Guess(receivedJSON.userguess);
                        responseHeader.code = (byte)(game.Found ? 0x01 : 0x02);
                        if (game.RemainNumberToGuess == 0)
                        {
                            responseHeader.code = 0x04;
                        }
                        break;
                    case 0x02:
                        responseHeader.code = 0x03;
                        break;
                    case 0x03:
                        responseHeader.code = 0x07;
                        gameDict.Remove(sessionId);
                        break;
                    case 0x04:
                        break;
                    case 0x05:
                        game.Start();
                        responseHeader.code = 0x00;
                        break;
                    default:
                        break;
                }
                sent.SessionID = sessionId;
                sent.LongString = game.GuessString;
                sent.TotalNumber = game.RemainNumberToGuess;
                sent.NumberOfWordsToFind = game.RemainNumberToGuess;
                responseJSON = JsonSerializer.Serialize<SentBody>(sent);
                Send(responseHeader, responseJSON);

                Console.WriteLine("Sent Code: {0}", responseHeader.code);
                Console.WriteLine("Sent Legth: {0}", responseHeader.length);
                Console.WriteLine("Sent: {0}", responseJSON);
            }
            netStream.Close();
            client.Close();
        }

        internal bool Send(Header header, string message)
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(message);
                header.length = data.Length;
                // Send header
                netStream.Write(header.GetBytes(), 0, header.GetLength());
                // Send body
                netStream.Write(data, 0, data.Length);
                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine("Message send failed: {0}", message);
                Console.Error.WriteLine(e.Message);
                return false;
            }
        }

        internal bool Receive(out Header header, out string message)
        {
            header = new Header();
            try
            {
                byte[] data = new byte[header.GetLength()];
                // Return zero if there is no data in NetworkStream
                int readLength = netStream.Read(data, 0, data.Length);
                if (readLength != data.Length)
                {
                    if (readLength != 0)
                    {
                        Console.Error.WriteLine("Header invalid");
                    }
                    message = null;
                    return false;
                }

                header.code = data[0];

                header.length = BitConverter.ToInt32(data, 1);
                data = new byte[header.length];
                int count = netStream.Read(data, 0, data.Length);
                message = Encoding.ASCII.GetString(data, 0, count);
                return true;
            }
            catch (IOException)
            {
                Console.WriteLine("Client disconnected unexpectedly");
                message = null;
                return false;
            }
        }
    }
}
