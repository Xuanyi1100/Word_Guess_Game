// ViewModels/GameViewModel.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Controls;
using try_to_build_client.Views;

namespace try_to_build_client.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private string _timerDisplay = "00:00";
        private int _totalWords;
        private int _wordsFound;
        private string _characterString = "Your 80-character string will appear here.";
        private string _guessInput;
        private string _guessFeedback;
        private Action<UserControl> _navigationAction;
        public GameViewModel(Action<UserControl> navigationAction)
        {
            _navigationAction = navigationAction;
            SubmitGuessCommand = new RelayCommand(SubmitGuess);
            EndGameCommand = new RelayCommand(EndGame);
            // Initialize your game state, start timer, fetch string etc.
        }


        public string TimerDisplay
        {
            get { return _timerDisplay; }
            set { _timerDisplay = value; OnPropertyChanged(); }
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
        public string CharacterString
        {
            get { return _characterString; }
            set { _characterString = value; OnPropertyChanged(); }
        }
        public string GuessInput
        {
            get { return _guessInput; }
            set { _guessInput = value; OnPropertyChanged(); }
        }
        public string GuessFeedback
        {
            get { return _guessFeedback; }
            set { _guessFeedback = value; OnPropertyChanged(); }
        }

        public ICommand SubmitGuessCommand { get; private set; }
        public ICommand EndGameCommand { get; private set; }
        private void SubmitGuess()
        {
            if (string.IsNullOrEmpty(GuessInput))
            {
                GuessFeedback = "Please input your guess";
                return;
            }
            GuessFeedback = "You guessed: " + GuessInput;
            // Logic to check the guess
        }
        private void EndGame()
        {
            _navigationAction.Invoke(new connectPage());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}