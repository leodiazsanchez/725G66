using ChatApp.Model;
using ChatApp.Model.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using static ChatApp.Model.Utils.ValidateIPV4;

namespace ChatApp.ViewModel
{
    public class ChatViewModel : BaseViewModel
    {
        public enum ViewState
        {
            EmptyState,
            Chat,
            History,
            EmptyHistory,
        }

        private string ip = "127.0.0.1";
        private string port;
        private string currentMessage;
        private User targetUser;
        private ViewState currentView;

        private readonly ConnectionManager connectionManager;
        public event EventHandler<ViewState> ViewChange;
        public event EventHandler<string> SearchChange;
        private Dictionary<(Guid, DateTime), ObservableCollection<Message>> chatHistory;
        private ObservableCollection<User> chatHistoryList;
        private string searchTerm;

        public ChatViewModel(MainViewModel mainViewModel)
        {
            connectionManager = mainViewModel.ConnectionManager;
            currentView = ViewState.EmptyState;
            chatHistory = new();
            chatHistoryList = new ObservableCollection<User>();

            connectionManager.Users.CollectionChanged += (sender, args) => UsersCollectionChanged();
            connectionManager.BuzzReceived += (sender, args) => mainViewModel.ShakeWindow();
            connectionManager.ShowMessageBoxReceived += (sender, args) => ShowMessageBox(args);

            ConnectCommand = new RelayCommand(ExecuteConnect);
            DisconnectCommand = new RelayCommand(ExecuteDisconnect);
            BuzzCommand = new RelayCommand(ExecuteBuzz);
            SendMessageCommand = new RelayCommand(ExecuteSendMessage);
            HistoryCommand = new RelayCommand(ExecuteHistory);
        }

        private void ShowMessageBox((string title, string message, MessageBoxButton button, MessageBoxImage image, Action<MessageBoxResult>? callback) args)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBoxResult result = MessageBox.Show(args.message, args.title, args.button, args.image);

                if (args.callback != null)
                {
                    args.callback(result);
                }
            });
        }

        public ICommand ConnectCommand { get; }
        public ICommand BuzzCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand HistoryCommand { get; }

        public void ChangeView(ViewState view)
        {
            ViewChange?.Invoke(this, view);
        }


        public string SearchTerm
        {
            get => searchTerm;
            set
            {
                if (searchTerm != value)
                {
                    searchTerm = value;
                    OnPropertyChanged(nameof(SearchTerm));
                    SearchChange?.Invoke(this, searchTerm);
                }
            }
        }

        public string Username
        {
            get => connectionManager.Username;

        }

        public ViewState CurrentView
        {
            get => currentView;

            set
            {
                currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }

        }

        public User TargetUser
        {
            get => targetUser;
            set
            {
                targetUser = value;
                Trace.WriteLine(targetUser.Name);
                OnPropertyChanged(nameof(TargetUser));
                OnPropertyChanged(nameof(TargetUserName));
                OnPropertyChanged(nameof(Messages));
                OnPropertyChanged(nameof(ChatHistoryMessages));
            }
        }

        public string TargetUserName
        {
            get
            {
                if (targetUser == null)
                {
                    return "No user selected";
                }
                else
                {
                    return targetUser.Name;
                }
            }
            set
            {
                targetUser.Name = value;
                OnPropertyChanged(nameof(TargetUserName));
            }
        }

        public void UsersCollectionChanged()
        {
            int count = connectionManager.Users.Count;
            switch (count)
            {
                case 0:
                    ChangeView(ViewState.EmptyState);
                    break;
                case 1:
                    ChangeView(ViewState.Chat);
                    break;
                default:
                    break;
            }
        }

        public ObservableCollection<Message> Messages
        {
            get
            {
                if (targetUser != null && connectionManager.Messages.ContainsKey(targetUser.Guid))
                {
                    return connectionManager.Messages[targetUser.Guid];
                }

                return new ObservableCollection<Message>();
            }
        }

        public ObservableCollection<User> Connections
        {
            get => connectionManager.Users;
            set
            {
                if (connectionManager.Users != value)
                {
                    connectionManager.Users = value;
                    OnPropertyChanged(nameof(Connections));
                }
            }
        }

        public ObservableCollection<User> ChatHistory
        {
            get => chatHistoryList;

        }

        public ObservableCollection<Message> ChatHistoryMessages
        {
            get
            {
                if (targetUser != null && chatHistory.ContainsKey((targetUser.Guid, targetUser.Timestamp)))
                {
                    return chatHistory[(targetUser.Guid, targetUser.Timestamp)];
                }

                return new ObservableCollection<Message>();
            }

        }


        public string DisplayIPandPort
        {
            get => connectionManager.IP.ToString() + ":" + connectionManager.Port;
        }

        public string CurrentMessage
        {
            get => currentMessage;
            set
            {
                currentMessage = value;
                OnPropertyChanged(nameof(CurrentMessage));
            }
        }

        public string Port
        {
            get => port;
            set
            {
                port = value;
                OnPropertyChanged(nameof(Port));
            }
        }

        public string IP
        {
            get => ip;
            set
            {
                ip = value;
                OnPropertyChanged(nameof(IP));
            }
        }

        private async void ExecuteConnect(object parameter)
        {
            try
            {
                int _port = int.Parse(port);

                if (!ValidateIPv4(ip))
                {
                    throw new InvalidIPException();
                }

                IPAddress _ip = IPAddress.Parse(ip);

                if (_port <= 1024 && _port >= 65535)
                {
                    throw new InvalidDataException("Invalid port!");
                }

                if (_port == connectionManager.Port && _ip.Equals(connectionManager.IP))
                {
                    throw new InvalidDataException("Cannot connect to self");
                }

                await connectionManager.ConnectAsync(_ip, _port);
                Port = string.Empty;
            }
            catch (Exception ex)
            {
                Port = string.Empty;
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        public void ExecuteHistory(object parameter)
        {
            try
            {

                string folderPath = Path.Combine(Environment.CurrentDirectory, "ChatHistoryFiles", connectionManager.Username);

                if (!Directory.Exists(folderPath))
                {
                    // Create the directory
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine($"Directory created at: {folderPath}");
                }

                chatHistory.Clear();
                chatHistoryList.Clear();


                string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

                foreach (var filePath in jsonFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileName(filePath);
                        string content = File.ReadAllText(filePath);
                        var jsonObject = JsonDocument.Parse(content);

                        var guid = jsonObject.RootElement.GetProperty("Guid").GetString();
                        var username = jsonObject.RootElement.GetProperty("Username").GetString();
                        DateTime timestamp = DateTime.Parse(jsonObject.RootElement.GetProperty("Timestamp").GetString()).Date;
                        var messagesJson = jsonObject.RootElement.GetProperty("Messages");

                        var messages = JsonSerializer.Deserialize<List<Message>>(messagesJson.ToString());
                        ObservableCollection<Message> messagesObservableCollection = new ObservableCollection<Message>(messages);

                        chatHistory.Add((Guid.Parse(guid), timestamp), messagesObservableCollection);
                        chatHistoryList.Add(new User(Guid.Parse(guid), username + " " + timestamp.ToString("yyyy-MM-dd"), timestamp));

                        Trace.WriteLine($"File Name: {fileName}");
                        Trace.WriteLine($"Content: {content}");
                        Trace.WriteLine(new string('-', 20));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Error processing file {filePath}: {ex.Message}");
                    }
                }

                chatHistoryList = new ObservableCollection<User>(chatHistoryList.OrderByDescending((user => user.Timestamp)));

                if (chatHistory.Count == 0)
                {
                    ChangeView(ViewState.EmptyHistory);
                }
                else
                {
                    ChangeView(ViewState.History);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDisconnect(object parameter)
        {
            try
            {
                connectionManager.Disconnect(targetUser);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private async void ExecuteSendMessage(object parameter)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(currentMessage))
                {
                    await connectionManager.SendRequestAsync(RequestType.Message, targetUser.Guid, currentMessage);
                    CurrentMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExecuteBuzz(object parameter)
        {
            try
            {
                await connectionManager.SendRequestAsync(RequestType.Buzz, targetUser.Guid);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

    }
}
