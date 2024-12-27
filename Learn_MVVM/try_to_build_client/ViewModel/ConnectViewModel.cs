// ViewModels/ConnectViewModel.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Controls;
using try_to_build_client.Views;
using try_to_build_client.Models;
using System.Threading.Tasks;
using try_to_build_client.Helpers;

namespace try_to_build_client.ViewModels
{
    public class ConnectViewModel : INotifyPropertyChanged
    {
        private string _connectionStatus;
        private Action<UserControl> _navigationAction;
        private ConnectModel _connectModel;
        private TcpClientService _tcpClientService;

        public ConnectViewModel(Action<UserControl> navigationAction)
        {
            _navigationAction = navigationAction;

            ConnectCommand = new RelayCommand(async () => await Connect());
            _connectModel = new ConnectModel();
            _tcpClientService = new TcpClientService();
            _tcpClientService.OnConnectionError += OnConnectionError;
        }
        private void OnConnectionError(string message)
        {
            ConnectionStatus = message;
        }

        public string Username
        {
            get { return _connectModel.Username; }
            set { _connectModel.Username = value; OnPropertyChanged(); }
        }

        public string IpAddress
        {
            get { return _connectModel.IpAddress; }
            set { _connectModel.IpAddress = value; OnPropertyChanged(); }
        }
        public string Port
        {
            get { return _connectModel.Port.ToString(); }
            set
            {
                if (int.TryParse(value, out var parsedPort))
                {
                    _connectModel.Port = parsedPort;
                    OnPropertyChanged();
                }
            }
        }
        // client port
        public string ClientPort
        {
            get { return _connectModel.ClientPort.ToString(); }
            set
            {
                if (int.TryParse(value, out var parsedClientPort))
                {
                    _connectModel.ClientPort = parsedClientPort;
                    OnPropertyChanged();
                }
            }
        }
        public string TimeLimit
        {
            get { return _connectModel.TimeLimit.ToString(); }
            set
            {
                if (int.TryParse(value, out var parsedTimeLimit))
                {
                    _connectModel.TimeLimit = parsedTimeLimit;
                    OnPropertyChanged();
                }
            }
        }
        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            set { _connectionStatus = value; OnPropertyChanged(); }
        }

        public ICommand ConnectCommand { get; private set; }


        private async Task Connect()
        {
            if (!_connectModel.IsValid())
            {
                ConnectionStatus = _connectModel.ValidationMessage;
                return;
            }

            try
            {
                _connectModel.SaveSettings();
                ConnectionStatus = "Connecting...";
                await _tcpClientService.ConnectAsync(_connectModel.IpAddress, _connectModel.Port);
                ConnectionStatus = "Connected Successfully!";

                // filling the ClientMessage object for connect
                ClientMessage clientMessage = new ClientMessage
                {
                    Username = _connectModel.Username,
                    ClientPort = _connectModel.ClientPort
                };
                // send code 0 to start 
                await _tcpClientService.SendDataAsync(clientMessage, 0);

                ReceiveResult result = await _tcpClientService.StartReceivingAsync();
                if (result != null)
                {
                    if (result.HeaderCode == 0)
                    {
                        // instaniate GameData here to hold IpAddress and port then pass to gameViewModel
                        var gameData = new GameModel()
                        {
                            IpAddress = _connectModel.IpAddress,
                            Port = _connectModel.Port
                        };


                        // After successful connection, navigate to the game page
                        /* When navigate from connectPage to gamePage,
                         * the ContentControl in the MainWindow is updated with the new gamePage, 
                         * but without a correct DataContext, the gamePage won't know which ViewModel to connect to. 
                         * This results in the view model constructor not being called.*/
                        // So, make sure creates a new instance of gamePage and properly set's it's DataContext.
                        var gameViewModel = new GameViewModel(_navigationAction, result.ServerMessage, gameData);
                        _navigationAction.Invoke(new gamePage() { DataContext = gameViewModel });
                    }
                    else
                    {
                        ConnectionStatus = "Header Code not valid";
                    }
                }
                else
                {
                    ConnectionStatus = "No response from server.";
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = "Connection failed: " + ex.Message;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}