// Models/GameData.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Net;

namespace try_to_build_client.Models
{
    // How to test the logic without triggering UI-related side effects??
    public class GameModel : INotifyPropertyChanged
    {
        private string _characterString = "";
        private int _totalWords;
        private int _wordsFound;
        private string _sessionId;
        private List<string> _foundWords = new List<string>();
        private string _ipAddress;
        private int _port;
        private string _guessValidationMessage;

        public string CharacterString
        {
            get { return _characterString; }
            set { _characterString = value; OnPropertyChanged(); }
        }
        public int TotalWords
        {
            get { return _totalWords; }
            set { _totalWords = value; OnPropertyChanged(); }
        }
        public int WordsFound
        {
            get { return _wordsFound; }
            set { _wordsFound = value; OnPropertyChanged(); }
        }
        
        public string SessionId
        {
            get { return _sessionId; }
            set { _sessionId = value; OnPropertyChanged(); }
        }

        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; OnPropertyChanged(); }
        }
        public int Port
        {
            get { return _port; }
            set { _port = value; OnPropertyChanged(); }
        }

        public string GuessValidationMessage
        {
            get { return _guessValidationMessage; }
            set { _guessValidationMessage = value; OnPropertyChanged(); }
        }
        public bool IsValidGuess(string guess)
        {
            if (string.IsNullOrEmpty(guess))
            {
                GuessValidationMessage = "Please input your guess";
                return false;
            }
            GuessValidationMessage = "";
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}