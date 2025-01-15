using ChatApp.Model.Utils;
using ChatApp.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ChatApp.ViewModel.ChatViewModel;
namespace ChatApp.View
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl
    {
        private ChatViewModel viewModel;

        private ViewState previousViewState;
        private bool historyClicked = false;
        private ObservableCollection<User> originalUsers = new ObservableCollection<User>();

        public ChatView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            viewModel = new ChatViewModel(mainViewModel);
            DataContext = viewModel;
            viewModel.ViewChange += (s, e) => OnViewChange(s, e);
            viewModel.SearchChange += (s, e) => OnSearchChange(s, e);
        }

        private void OnSearchChange(object sender, string searchTerm)
        {
            var users = ListBoxList.ItemsSource as ObservableCollection<User>;
            if (users == null)
                return;

            // Filter users by their Name property
            var filtered = string.IsNullOrWhiteSpace(searchTerm)
                ? originalUsers
                : new ObservableCollection<User>(users.Where(user =>
                    user.Name.IndexOf(searchTerm, System.StringComparison.OrdinalIgnoreCase) >= 0));

            // Update the ItemsSource with the filtered collection
            ListBoxList.ItemsSource = filtered;
        }

        private void OnViewChange(object sender, ViewState viewState)
        {
            if ((viewState == ViewState.History || viewState == ViewState.EmptyHistory) && historyClicked)
            {
                viewState = previousViewState;
                var drawingImageSource = (DrawingImage)SecondaryButtonImage.Source;
                var geometryDrawing = (GeometryDrawing)drawingImageSource.Drawing;

                geometryDrawing.Geometry = Geometry.Parse("M8.515,1.019 A7,7 0 0 0 8,1 V0 A8,8 0 0 1 8.589,0.022 M10.519,1.469 A7,7 0 0 0 9.534,1.17 L9.753,0.194 Q10.329,0.323 10.879,0.536 M11.889,2.179 A7,7 0 0 0 11.45,1.909 L11.943,1.039 A8,8 0 0 1 12.922,1.693 L12.307,2.482 A7,7 0 0 0 11.889,2.179 M13.723,3.969 A7,7 0 0 0 13.07,3.173 L13.794,2.483 Q14.2,2.912 14.541,3.393 M15.287,5.321 A7,7 0 0 0 15.073,4.853 L15.966,4.403 A8,8 0 0 1 16.416,5.491 L15.466,5.804 A7,7 0 0 0 15.287,5.321 M15.817,7.828 A7,7 0 0 0 15.717,6.803 L16.702,6.633 Q16.802,7.213 16.818,7.803 M15.686,9.366 Q15.736,9.112 15.767,8.856 L16.76,8.979 A8,8 0 0 1 16.53,10.134 L15.566,9.867 Q15.635,9.62 15.686,9.366 M14.734,11.745 Q15.01,11.309 15.22,10.837 L16.134,11.242 Q15.894,11.782 15.579,12.28 M13.77,13.03 Q13.953,12.847 14.12,12.652 L14.878,13.305 A8,8 0 0 1 14.477,13.737 Z M8,1 A7,7 0 1 0 12.95,12.95 L13.657,13.657 A8.001,8.001 0 1 1 8,0 Z M7.5,3 A0.5,0.5 0 0 1 8,3.5 V8.71 L11.248,10.566 A0.5,0.5 0 0 1 10.752,11.434 L7.252,9.434 A0.5,0.5 0 0 1 7,9 V3.5 A0.5,0.5 0 0 1 7.5,3");
                if (viewModel.Connections.Count > 0)
                {
                    ListBoxList.SelectedItem = viewModel.Connections[0];
                }
                historyClicked = false;
            }
            else
            {
                previousViewState = viewModel.CurrentView;
                historyClicked = (viewState == ViewState.History || viewState == ViewState.EmptyHistory);
            }

            // Apply view state changes
            switch (viewState)
            {
                case ViewState.EmptyState:
                    ListBoxTitle.Text = "Direct Messages";
                    SecondaryButtonText.Text = "Show History";
                    ListBoxList.ItemsSource = viewModel.Connections;
                    break;

                case ViewState.Chat:
                    ListBoxTitle.Text = "Direct Messages";
                    SecondaryButtonText.Text = "Show History";
                    if (viewModel.Connections.Count == 1)
                    {
                        ListBoxList.SelectedItem = viewModel.Connections[0];
                    }
                    originalUsers = viewModel.Connections;
                    ListBoxList.ItemsSource = viewModel.Connections;

                    OnSearchChange(this, SearchInput.Text);
                    break;

                case ViewState.History:
                    ChangesForHistoryState();
                    OnSearchChange(this, SearchInput.Text);
                    break;

                case ViewState.EmptyHistory:
                    ChangesForHistoryState();
                    break;
            }

            viewModel.CurrentView = viewState;
        }

        private void ChangesForHistoryState()
        {
            ListBoxTitle.Text = "Chat History";
            SecondaryButtonText.Text = "Back";

            var drawingImageSource = (DrawingImage)SecondaryButtonImage.Source;
            var geometryDrawing = (GeometryDrawing)drawingImageSource.Drawing;

            geometryDrawing.Geometry = Geometry.Parse("M1 8a7 7 0 1 0 14 0A7 7 0 0 0 1 8m15 0A8 8 0 1 1 0 8a8 8 0 0 1 16 0m-4.5-.5a.5.5 0 0 1 0 1H5.707l2.147 2.146a.5.5 0 0 1-.708.708l-3-3a.5.5 0 0 1 0-.708l3-3a.5.5 0 1 1 .708.708L5.707 7.5z");

            if (viewModel.ChatHistory.Count > 0)
            {
                ListBoxList.SelectedItem = viewModel.ChatHistory[0];
            }
            originalUsers = viewModel.ChatHistory;
            ListBoxList.ItemsSource = viewModel.ChatHistory;

        }

        private void NewChatButton_Click(object sender, RoutedEventArgs e)
        {
            PortInput portInput = new(viewModel);

            var mainWindow = Window.GetWindow(this);

            if (mainWindow != null)
            {
                portInput.Owner = mainWindow;

                portInput.WindowStartupLocation = WindowStartupLocation.Manual;
                portInput.Left = mainWindow.Left + (mainWindow.Width - portInput.Width) / 2;
                portInput.Top = mainWindow.Top + (mainWindow.Height - portInput.Height) / 2;
            }

            portInput.ShowDialog();
        }

        private void RecentChatsList_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (ListBoxList.SelectedItem is User user)
            {
                viewModel.TargetUser = user;
            }
        }

    }
}
