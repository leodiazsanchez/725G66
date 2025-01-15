using ChatApp.ViewModel;
using System.Windows;

namespace ChatApp.View
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class PortInput : Window
    {
        public PortInput(ChatViewModel chatViewModel)
        {
            InitializeComponent();
            DataContext = chatViewModel;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            PlaceholderText.Visibility = Visibility.Visible;
            DialogResult = true;
            Close();
        }

        private void PortTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderText.Visibility = Visibility.Collapsed;
        }

        private void PortTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PlaceholderText.Text))
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }
        }
    }

}
