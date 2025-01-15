using ChatApp.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatApp.View
{
    /// <summary>
    /// Interaction logic for MessageView.xaml
    /// </summary>
    /// 

    public partial class MessageView : UserControl
    {
        public MessageView()
        {
            InitializeComponent();
        }

        private void MessageInput_GotFocus(object sender, RoutedEventArgs e)
        {
            MessagePlaceholder.Visibility = Visibility.Hidden;
        }

        private void MessageInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageInput.Text))
            {
                MessagePlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                e.Handled = true;

                var viewModel = this.DataContext as ChatViewModel;
                if (viewModel != null && !string.IsNullOrWhiteSpace(viewModel.CurrentMessage))
                {
                    viewModel.SendMessageCommand.Execute(viewModel.CurrentMessage);
                    viewModel.CurrentMessage = string.Empty;
                }
            }
        }
    }
}