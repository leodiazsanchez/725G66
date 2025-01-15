using ChatApp.Model;
using ChatApp.View;
using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using static ChatApp.Model.Utils.ValidateIPV4;

namespace ChatApp.ViewModel
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;

        public LoginViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        public ICommand LoginCommand { get; }

        private string username = "";
        private string _port = "";
        private string _ip = "127.0.0.1";

        public string Username
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Port
        {
            get => _port;
            set
            {
                _port = value;
                OnPropertyChanged(nameof(Port));
            }
        }

        public string IP
        {
            get => _ip;
            set
            {
                _ip = value;
                OnPropertyChanged(nameof(Port));
            }
        }

        private void ExecuteLogin(object parameter)
        {

            try
            {
                if (!string.IsNullOrWhiteSpace(username))
                {
                    int port = Int32.Parse(_port);

                    if (port >= 1024 && port <= 65535)
                    {
                        if (ValidateIPv4(_ip))
                        {
                            IPAddress ip = IPAddress.Parse(_ip);
                            ConnectionManager connectionManager = new(ip, port, username);
                            _mainViewModel.ConnectionManager = connectionManager;
                            _mainViewModel.CurrentView = new ChatView(_mainViewModel);
                        }
                        else
                        {
                            throw new InvalidIPException();
                        }

                    }
                    else
                    {
                        MessageBox.Show("Port must be between 1024 and 65535.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Username cannot be null or empty.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid port format. Please enter a numeric value.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
