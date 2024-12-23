// Models/ConnectionSettings.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using try_to_build_client.Helpers;

namespace try_to_build_client.Models
{
    public class ConnectionSettings : INotifyPropertyChanged
    {
        private string _username;
        private string _ipAddress;
        private int _port;
        private int _timeLimit;
        
        public ConnectionSettings()
        {
            Username = AppConfigManager.GetSetting("Username");
            IpAddress = AppConfigManager.GetSetting("IpAddress");
            if (int.TryParse(AppConfigManager.GetSetting("Port"), out int port))
                Port = port;
            if (int.TryParse(AppConfigManager.GetSetting("TimeLimit"), out int timeLimit))
                TimeLimit = timeLimit;
        }
        public string Username
        {
            get { return _username; }
            set { _username = value; OnPropertyChanged(); }
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

        public int TimeLimit
        {
            get { return _timeLimit; }
            set { _timeLimit = value; OnPropertyChanged(); }
        }
  

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                ValidationMessage = "Username cannot be empty.";
                return false;
            }
            if (!IsValidIpAddress(IpAddress))
            {
                ValidationMessage = "IP Address is not valid.";
                return false;
            }
            if (!IsValidPortNumber(Port))
            {
                ValidationMessage = "Port number is not valid. range 1024-65535";
                return false;
            }
            if (!IsValidTimeLimit(TimeLimit))
            {
                ValidationMessage = "Time limit must be greater than 0 and less than 600";
                return false;
            }
            ValidationMessage = "";
            return true;
        }
        private bool IsValidIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return false;
            }
            string[] parts = ipAddress.Split('.');
            if (parts.Length != 4)
            {
                return false;
            }
            foreach (string part in parts)
            {
                if (!byte.TryParse(part, out _))
                {
                    return false;
                }
            }
            return true;
        }
        private bool IsValidPortNumber(int port)
        {
            return port >= 1024 && port <= 65535;
        }
        private bool IsValidTimeLimit(int timeLimit)
        {
            return timeLimit > 0 && timeLimit < 600;
        }

        private string _validationMessage;

        public string ValidationMessage
        {
            get { return _validationMessage; }
            set
            {
                _validationMessage = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SaveSettings()
        {
            AppConfigManager.SetSetting("Username", Username);
            AppConfigManager.SetSetting("IpAddress", IpAddress);
            AppConfigManager.SetSetting("Port", Port.ToString());
            AppConfigManager.SetSetting("TimeLimit", TimeLimit.ToString());
        }
    }
}