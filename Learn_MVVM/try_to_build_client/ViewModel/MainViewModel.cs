// ViewModels/MainViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using try_to_build_client.Views;
using System;
using try_to_build_client.Models;

namespace try_to_build_client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private UserControl _currentPage;
        private TcpClientService _tcpClientService;
        public UserControl CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel()
        {
            // Start with the connect page
            NavigateToConnectPage();
        }
        private void NavigateToConnectPage()
        {
            var connectViewModel = new ConnectViewModel(Navigate);
            CurrentPage = new connectPage() { DataContext = connectViewModel };
        }
        private void NavigateToGamePage()
        {
            var gameViewModel = new GameViewModel(Navigate, _tcpClientService);
            CurrentPage = new gamePage() { DataContext = gameViewModel };
        }


        private void Navigate(UserControl view)
        {
            CurrentPage = view;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}