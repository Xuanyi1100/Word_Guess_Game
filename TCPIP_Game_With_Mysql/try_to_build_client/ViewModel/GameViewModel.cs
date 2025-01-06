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
using System.Net;

namespace try_to_build_client.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged, IDisposable
    {
        private string _timerDisplay = "00:00";
        private string _guessInput;
        private string _guessFeedback;
        private int _timeLimit;
        /*  
         *  Why timer is here rather than in model:
            The DispatcherTimer is specifically designed to raise its Tick event on the UI thread.
            This makes it the perfect tool for updating UI-related data at regular intervals.
            Models are typically meant to be UI-agnostic, meaning they should not be tightly coupled with 
            any particular UI framework.
            So, a DispatcherTimer which is tightly coupled with the UI, is not suitable in the model. 
        *
        */
        private DispatcherTimer _timer;
        private int _remainingTime;
        private readonly Action<UserControl> _navigationAction;
        private GameModel _gameModel;
        private TcpClientService _tcpClientService;
        private bool _disposed = false;
        private TcpServerListener _tcpServerListener;

        public GameViewModel(Action<UserControl> navigationAction,  ServerMessage serverMessage, GameModel gamedata)
        {
            _navigationAction = navigationAction;
            SubmitGuessCommand = new RelayCommand(async () => await SubmitGuess());
            EndGameCommand = new RelayCommand(async () => await EndGame());
            _gameModel = gamedata;
            _tcpClientService = new TcpClientService();

            if (serverMessage != null)
            {
                _gameModel.CharacterString = serverMessage.CharacterString;
                _gameModel.TotalWords = serverMessage.TotalWords;
                _gameModel.WordsFound = serverMessage.WordsToFound;
                _gameModel.SessionId = serverMessage.SessionId;
            }
            StartTimer();
            //  subscribe to the GameModel's PropertyChanged event
            /* if your class subscribes to a event, you should also release the subscription when the class is not used anymore */
            _gameModel.PropertyChanged += GameModel_PropertyChanged;

            // start listener to receive server shut down message
            _tcpServerListener = new TcpServerListener();
            IPAddress localAddr = IPAddress.Parse("0.0.0.0");
            _tcpServerListener.SetUpListener(localAddr.ToString(), _gameModel.ClientPort); 
            _tcpServerListener.OnShutdownMessageReceived += OnShutdownMessageReceived;
        }

        private void OnShutdownMessageReceived()
        {
            _timer.Stop();
            MessageBoxResult result = MessageBox.Show(Application.Current.MainWindow, "Server shut down!", "End Game", MessageBoxButton.OK);
            if (result == MessageBoxResult.OK)
            {
                // Dispose the current ViewModel
                this.Dispose();

                // Exit the application
                Application.Current.Shutdown();
            }
        }

        // When the model's WordsFound property changes, notify that the ViewModel's WordsFound property has changed 
        private void GameModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {            
            if (e.PropertyName == nameof(GameModel.WordsFound))
            {
                OnPropertyChanged(nameof(WordsFound));
            }
        }
        public string TimerDisplay
        {
            get { return _timerDisplay; }
            set { _timerDisplay = value; OnPropertyChanged(); }
        }
        public string TotalWords
        {
            get { return _gameModel.TotalWords.ToString(); }
        }
        public string WordsFound
        {
            get { return _gameModel.WordsFound.ToString(); }

            // since it already in the model, you don't have to use setter to change it here,
            // just change it in model and subscrib to the change event then change it here in viewmodel
            //set { _wordsFound = value; OnPropertyChanged(); }
        }
        public string CharacterString
        {
            get { return _gameModel.CharacterString; }
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
            try
            {
                // Input validation is now in model
                if (!_gameModel.IsValidGuess(GuessInput))
                {
                    GuessFeedback = _gameModel.GuessValidationMessage;
                    return;
                }

                GuessFeedback = "You guessed: " + GuessInput;
                ClientMessage clientMessage = new ClientMessage
                {
                    SessionId = _gameModel.SessionId,
                    Username = AppConfigManager.GetSetting("Username"),
                    UserGuess = GuessInput
                };

                // connect again because it's a stateless connect model.
                /* stateless means :
                 * Client creates a new connection for each request
                 * Client sends data
                 * Client receives response
                 * Server closes the connection
                 * The cycle repeats for the next request
                 */
                await _tcpClientService.ConnectAsync(_gameModel.IpAddress, _gameModel.Port);

                // code 1, represent "Submit a Guess"
                await _tcpClientService.SendDataAsync(clientMessage, 1);

                ReceiveResult result = await _tcpClientService.StartReceivingAsync();
                if (result?.ServerMessage != null)
                {
                    // it's the "just change it in model" :
                    _gameModel.WordsFound = result.ServerMessage.WordsToFound;

                    switch (result.HeaderCode)
                    {
                        case 1:
                            GuessFeedback = "Correct Guess, go ahead!";
                            break;

                        case 2:
                            GuessFeedback = "Wrong Guess, try again.";
                            break;

                        case 4:
                            _timer.Stop();
                            MessageBoxResult messageResult1 = MessageBox.Show("You win! Do you want to play again?", "Congratulations", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (messageResult1 == MessageBoxResult.Yes)
                            {
                                // code 5, represent "Play again", then server can delete relate game object
                                await _tcpClientService.ConnectAsync(_gameModel.IpAddress, _gameModel.Port);
                                await _tcpClientService.SendDataAsync(clientMessage, 5);

                                // Dispose the current ViewModel
                                this.Dispose();

                                // Go back to connect page
                                // Don't forget connect the view to the view's viewmodel,
                                // just "_navigationAction.Invoke(new connectPage());" won't work, no data will fill in
                                var connectViewModel = new ConnectViewModel(_navigationAction);
                                _navigationAction.Invoke(new connectPage() { DataContext = connectViewModel });

                            }
                            else
                            {
                                // Dispose the current ViewModel
                                this.Dispose();

                                // Exit the application
                                Application.Current.Shutdown();
                            }
                            break;
                        default:
                            GuessFeedback = "Unknown Response";
                            break;
                    }
                }
                else
                {
                    GuessFeedback = "Server does not send data";
                }
            }
            catch (Exception ex)
            {
                GuessFeedback = "Error connecting to server: " + ex.Message;
            }
                       
        }
        private async Task EndGame()
        {
            try
            {
                _timer.Stop();

                // connect again and sent header code 2, expect code 3 ( ask user if want to quit)          
                ClientMessage clientMessage = new ClientMessage
                {
                    SessionId = _gameModel.SessionId,
                };

                await _tcpClientService.ConnectAsync(_gameModel.IpAddress, _gameModel.Port);

                await _tcpClientService.SendDataAsync(clientMessage, 2);

                ReceiveResult result = await _tcpClientService.StartReceivingAsync();
                if (result?.ServerMessage != null)
                {
                    if (result.HeaderCode == 3)
                    {
                        MessageBoxResult messageResult = MessageBox.Show("Do you want to quit?", "Exit Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (messageResult == MessageBoxResult.Yes)
                        {

                            try
                            {
                                // Inform the server user quit
                                await _tcpClientService.ConnectAsync(_gameModel.IpAddress, _gameModel.Port);
                                await _tcpClientService.SendDataAsync(clientMessage, 3);
                            }
                            catch (Exception ex)
                            {
                                // Handle exceptions if needed
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                            finally
                            {
                                // Dispose the current ViewModel
                                this.Dispose();

                                // Shutdown the application
                                Application.Current.Shutdown();
                            }

                        }
                        else
                        {
                            // inform the server user go on game
                            await _tcpClientService.ConnectAsync(_gameModel.IpAddress, _gameModel.Port);

                            await _tcpClientService.SendDataAsync(clientMessage, 4);

                            _timer.Start();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Unknown Response");
                    }
                }
                else
                {
                    GuessFeedback = "Server does not send data";
                }
            }
            catch (Exception ex)
            {
                GuessFeedback = "Error connecting to server: " + ex.Message;
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

        private async void Timer_Tick(object sender, EventArgs e)
        {
            if (_remainingTime > 0)
            {
                _remainingTime--;
                UpdateTimerDisplay();
            }
            else
            {
                _timer.Stop();
                // tell server time out user lose, no need to hear response
                ClientMessage clientMessage = new ClientMessage
                {
                    SessionId = _gameModel.SessionId,
                };

                await _tcpClientService.ConnectAsync(_gameModel.IpAddress, _gameModel.Port);

                await _tcpClientService.SendDataAsync(clientMessage, 6);

                MessageBoxResult result = MessageBox.Show("Time's Up! Game Over.", "Game Over", MessageBoxButton.OK);
                if (result == MessageBoxResult.OK)
                {
                    // Dispose the current ViewModel
                    this.Dispose();

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

        // Implement IDisposable pattern
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _timer?.Stop();
                    _timer = null;

                    // Unsubscribe from events
                    if (_gameModel != null)
                    {
                        _gameModel.PropertyChanged -= GameModel_PropertyChanged;
                    }

                    if (_tcpServerListener != null)
                    {
                        _tcpServerListener.OnShutdownMessageReceived -= OnShutdownMessageReceived;
                        _tcpServerListener.Disconnect();
                        _tcpServerListener = null;  // Clear the reference after disconnecting
                    }

                }

                // Clean up unmanaged resources (if any)

                _disposed = true;
            }
        }

        // Add finalizer only if there are unmanaged resources
        ~GameViewModel()
        {
            Dispose(false);
        }
    }
}