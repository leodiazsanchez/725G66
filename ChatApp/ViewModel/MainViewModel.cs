using ChatApp.Model;
using ChatApp.View;
using System;
using System.Windows.Controls;

namespace ChatApp.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private UserControl currentView;
        private ConnectionManager connectionManager;

        public event EventHandler ShakeRequested;

        public MainViewModel()
        {
            CurrentView = new LoginView(this);
        }

        public UserControl CurrentView
        {
            get => currentView;
            set
            {
                currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public void ShakeWindow()
        {
            ShakeRequested?.Invoke(this, EventArgs.Empty);
        }

        public ConnectionManager ConnectionManager { get => connectionManager; set => connectionManager = value; }
    }
}
