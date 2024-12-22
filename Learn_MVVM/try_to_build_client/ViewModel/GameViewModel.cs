// ViewModels/GameViewModel.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Controls;
using try_to_build_client.Views;
using System.Windows.Threading;
using System.Windows;
using try_to_build_client.Helpers;
using try_to_build_client.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace try_to_build_client.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private string _timerDisplay = "00:00";
        private string _guessInput;
        private string _guessFeedback;
        private int _timeLimit;
        // Why timer is here rather than in model:
        // The DispatcherTimer is specifically designed to raise its Tick event on the UI thread.
        // This makes it the perfect tool for updating UI-related data at regular intervals.
        // Models are typically meant to be UI-agnostic, meaning they should not be tightly coupled with any particular UI framework.
        // So, a DispatcherTimer which is tightly coupled with the UI, is not suitable in the model.
        private DispatcherTimer _timer;
        private int _remainingTime;
        private readonly Action<UserControl> _navigationAction;
        private GameData _gameData;
        private TcpClientService _tcpClientService;

        public GameViewModel(Action<UserControl> navigationAction, TcpClientService tcpClientService)
        {
            _navigationAction = navigationAction;
            SubmitGuessCommand = new RelayCommand(async () => await SubmitGuess());
            EndGameCommand = new RelayCommand(EndGame);
            _gameData = new GameData();
            _tcpClientService = tcpClientService;
            _tcpClientService.OnDataReceived += OnDataReceived;
            StartTimer();
        }

        private void OnDataReceived(string message)
        {
            _gameData.CharacterString = message; // ??? need to split the message
            Debug.WriteLine($"Message from server: {message}");
        }

        public string TimerDisplay
        {
            get { return _timerDisplay; }
            set { _timerDisplay = value; OnPropertyChanged(); }
        }
        public string TotalWords
        {
            get { return _gameData.TotalWords.ToString(); }
        }
        public string WordsFound
        {
            get { return _gameData.WordsFound.ToString(); }
        }
        public string CharacterString
        {
            get { return _gameData.CharacterString; }
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

        private async Task SubmitGuess()
        {
            if (string.IsNullOrEmpty(GuessInput))
            {
                GuessFeedback = "Please input your guess";
                return;
            }
            GuessFeedback = "You guessed: " + GuessInput;
            await _tcpClientService.SendDataAsync(GuessInput, 1);
            await _tcpClientService.StartReceivingAsync();
            // Logic to check the guess
        }
        private void EndGame()
        {
            _timer.Stop();
            MessageBoxResult result = MessageBox.Show("Are you sure you want to quit?", "End Game", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var connectViewModel = new ConnectViewModel(_navigationAction);
                _navigationAction.Invoke(new connectPage() { DataContext = connectViewModel });
            }
            else
            {
                _timer.Start();
            }
        }
        private void StartTimer()
        {
            if (int.TryParse(AppConfigManager.GetSetting("TimeLimit"), out _timeLimit))
            {
                _remainingTime = _timeLimit;
                UpdateTimerDisplay();
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("Timer Tick Called");
            if (_remainingTime > 0)
            {
                _remainingTime--;
                UpdateTimerDisplay();
            }
            else
            {
                _timer.Stop();
                MessageBoxResult result = MessageBox.Show("Time's Up! Game Over.", "Game Over", MessageBoxButton.OK);
                if (result == MessageBoxResult.OK)
                {
                    var connectViewModel = new ConnectViewModel(_navigationAction);
                    _navigationAction.Invoke(new connectPage() { DataContext = connectViewModel });
                }

            }
        }
        private void UpdateTimerDisplay()
        {
            TimeSpan time = TimeSpan.FromSeconds(_remainingTime);
            TimerDisplay = time.ToString(@"mm\:ss");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}