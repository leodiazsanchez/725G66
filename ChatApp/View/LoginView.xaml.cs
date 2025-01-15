using ChatApp.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace ChatApp.View
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            DataContext = new LoginViewModel(mainViewModel);
        }

        private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UsernameText.Visibility = Visibility.Collapsed; // Hide placeholder
        }

        private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                UsernameText.Visibility = Visibility.Visible; // Show placeholder if empty
            }
        }

        private void PortTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderText.Visibility = Visibility.Collapsed; // Hide placeholder
        }

        private void PortTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PortTextBox.Text))
            {
                PlaceholderText.Visibility = Visibility.Visible; // Show placeholder if empty
            }
        }
    }
}
