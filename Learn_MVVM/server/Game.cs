// server/Game.cs

using System.Net.Sockets;
using System.Text;
using server.Helper;


namespace server
{
    internal class Game
    {
        public Game() { }

        internal string ClientAddress { get; set; }
        internal int ClientPort { get; set; }
        
        internal bool Found => _found;
        private bool _found = false;

        private bool _repeated = false;

        internal string GuessString => _guessString;
        private string _guessString = null;

        internal int TotalNumberToGuess => _totalNumberToGuess;
        private int _totalNumberToGuess = 0;

        internal int RemainNumberToGuess => _remainNumberToGuess;
        private int _remainNumberToGuess = 0;

        // hashset is efficient for checking the existence of an item because it uses a hash-based lookup.
        private HashSet<string> answerSet = null;
        private HashSet<string> wordSet = null;

        // find a random string, do it in folder first, later in database
        internal void Start () 
        {
            string[] files = Directory.GetFiles("gameStrings");
            Random rand = new Random(DateTime.Now.Second);
            string filename = files[rand.Next(files.Length)];
            string[] lines = File.ReadAllLines(filename);

            _guessString = lines[0];
            _totalNumberToGuess = Convert.ToInt32(lines[1]);
            _remainNumberToGuess = _totalNumberToGuess;
            answerSet = new HashSet<string>(lines.Skip(2));

            wordSet = new HashSet<string>(answerSet.Count);

        }

        // decide if user guess right, update number of words left
        internal void Guess(string userGuess)
        {
            _found = answerSet.Contains(userGuess);
            _repeated = wordSet.Contains(userGuess);
            if (_found && !_repeated)
            {
                wordSet.Add(userGuess);
                _remainNumberToGuess--;
            }

        }

        // using user ip and port, send tcp message to notify shut down
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
