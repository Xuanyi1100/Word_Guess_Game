// ViewModels/ConnectViewModel.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Controls;
using try_to_build_client.Views;
using try_to_build_client.Models;

namespace try_to_build_client.ViewModels
{
    public class ConnectViewModel : INotifyPropertyChanged
    {
        private string _connectionStatus;
        private Action<UserControl> _navigationAction;
        private ConnectionSettings _connectionSettings;

        public ConnectViewModel(Action<UserControl> navigationAction)
        {
            _navigationAction = navigationAction;
            ConnectCommand = new RelayCommand(Connect);
            _connectionSettings = new ConnectionSettings();
        }

        public string Username
        {
            get { return _connectionSettings.Username; }
            set { _connectionSettings.Username = value; OnPropertyChanged(); }
        }

        public string IpAddress
        {
            get { return _connectionSettings.IpAddress; }
            set { _connectionSettings.IpAddress = value; OnPropertyChanged(); }
        }
        public string Port
        {
            get { return _connectionSettings.Port.ToString(); }
            set
            {
                if (int.TryParse(value, out var parsedPort))
                {
                    _connectionSettings.Port = parsedPort;
                    OnPropertyChanged();
                }
            }
        }
        public string TimeLimit
        {
            get { return _connectionSettings.TimeLimit.ToString(); }
            set
            {
                if (int.TryParse(value, out var parsedTimeLimit))
                {
                    _connectionSettings.TimeLimit = parsedTimeLimit;
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


        private void Connect()
        {
            if (!_connectionSettings.IsValid())
            {
                ConnectionStatus = _connectionSettings.ValidationMessage;
                return;
            }

            try
            {
                _connectionSettings.SaveSettings();
                // After successful connection, navigate to the game page
                /* When navigate from connectPage to gamePage,
                 * the ContentControl in the MainWindow is updated with the new gamePage, 
                 * but without a correct DataContext, the gamePage won't know which ViewModel to connect to. 
                 * This results in the view model constructor not being called.*/
                // So, make sure creates a new instance of gamePage and properly set's it's DataContext.
                var gameViewModel = new GameViewModel(_navigationAction);
                _navigationAction.Invoke(new gamePage() { DataContext = gameViewModel });          
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