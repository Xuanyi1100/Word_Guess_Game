// Models/GameData.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace try_to_build_client.Models
{
    // How to test the logic without triggering UI-related side effects??
    public class GameData : INotifyPropertyChanged
    {
        private string _characterString = "Your 80-character string will appear here.";
        private int _totalWords;
        private int _wordsFound;
        private List<string> _foundWords = new List<string>();

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}