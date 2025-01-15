using ChatApp.Model.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using static ChatApp.Model.Utils.Message;

namespace ChatApp.Model
{
    public class ConnectionManager
    {

        private Client client;
        public event EventHandler BuzzReceived;
        public event EventHandler<(string title, string message, MessageBoxButton button, MessageBoxImage image, Action<MessageBoxResult>? callback)> ShowMessageBoxReceived;

        public Guid Guid { get; private set; }
        public string Username { get; private set; }
        public IPAddress IP { get; private set; }
        public int Port { get; private set; }
        public ConcurrentDictionary<Guid, TcpClient> Connections { get; private set; }
        public ObservableCollection<User> Users { get; set; }
        public ConcurrentDictionary<Guid, ObservableCollection<Message>> Messages { get; private set; }


        public ConnectionManager(IPAddress ip, int port, string username)
        {
            Guid = Guid.NewGuid();
            Username = username;
            IP = ip;
            Port = port;
            Connections = new ConcurrentDictionary<Guid, TcpClient>();
            Users = new ObservableCollection<User>();
            Messages = new ConcurrentDictionary<Guid, ObservableCollection<Message>>();

            _ = new Server(this);
            client = new Client(this);
        }

        public void AddMessage(Guid guid, Message message)
        {
            if (!Messages.ContainsKey(guid))
            {
                Messages[guid] = new ObservableCollection<Message>();
            }

            Messages[guid].Add(message);
        }

        public void ShowMessageBox(string title, string message, MessageBoxButton button, MessageBoxImage image, Action<MessageBoxResult>? callback = null)
        {
            ShowMessageBoxReceived?.Invoke(this, (title, message, button, image, callback ?? (_ => { })));
        }


        public void AddConnection(User user, TcpClient tcpClient)
        {
            Connections.TryAdd(user.Guid, tcpClient);
            Users.Add(user);
            Messages[user.Guid] = new ObservableCollection<Message>();
        }

        public void RemoveConnection(Guid guid)
        {
            Trace.WriteLine($"Attempting to remove connection for Guid: {guid}");

            if (Connections.ContainsKey(guid))
            {
                var client = Connections[guid];
                client.Close();
                Connections.Remove(guid, out _);
                Trace.WriteLine($"Removed connection for user (Guid: {guid}");
            }
            else
            {
                Trace.WriteLine($"Connection for Guid: {guid}) not found.");
            }

            // Remove the user from the Users collection
            foreach (var user in Users)
            {
                if (user.Guid == guid)
                {
                    Users.Remove(user);
                    Trace.WriteLine($"Removed user {user.Name} (Guid: {user.Guid}) from Users.");
                    break;
                }
            }


            // Clean up messages related to this user
            Messages.Remove(guid, out _);
            Trace.WriteLine($"Removed messages for (Guid: {guid}).");
        }


        public void Buzz()
        {
            BuzzReceived?.Invoke(this, EventArgs.Empty);
        }

        public async Task ConnectAsync(IPAddress ip, int port)
        {
            await client.ConnectAsync(ip, port);
        }

        public async void Disconnect(User user)
        {

            if (Connections.TryGetValue(user.Guid, out var client))
            {
                Trace.WriteLine("dc...");
                await SendRequestAsync(RequestType.Disconnect, user.Guid);
                RemoveConnection(user.Guid);
            }
            else
            {
                Trace.WriteLine("couldnt dc...");
            }

        }

        public async Task SendRequestAsync(RequestType request, Guid guid, string? message = null)
        {

            if (Connections.TryGetValue(guid, out var targetClient))
            {
                ChatProtocol protocol = new(Guid, Username, request, message);

                byte[] messageBytes = Encoding.UTF8.GetBytes(protocol.Serialize());

                NetworkStream stream = targetClient.GetStream();
                await stream.WriteAsync(messageBytes.AsMemory(0, messageBytes.Length));

                Trace.WriteLine($"Sent to {guid}: {protocol.Serialize()}");

                if (protocol.Message != null)
                {
                    Message messageObj = new Message(protocol.Message, ConnectionDirection.Outgoing);
                    Application.Current.Dispatcher.Invoke(() => Messages[guid].Add(messageObj));
                    await SaveMessagesToJsonAsync(guid, messageObj);
                }

            }
            else
            {
                Trace.WriteLine($"Client {guid} not found.");
            }
        }

        public async Task SaveMessagesToJsonAsync(Guid guid, Message message)
        {
            if (Messages.TryGetValue(guid, out var messages))
            {
                string username = GetUserByGuid(Users, guid).Name;
                string folderPath = Path.Combine(Environment.CurrentDirectory, $"ChatHistoryFiles/{Username}");
                string filePath = Path.Combine(folderPath, $"{username}_{DateTime.Now:yyyyMMdd}.json");

                // Ensure the directory exists
                Directory.CreateDirectory(folderPath);

                if (File.Exists(filePath))
                {
                    string fileName = Path.GetFileName(filePath);
                    string content = File.ReadAllText(filePath);
                    var jsonObject = JsonDocument.Parse(content);

                    var targetGuid = jsonObject.RootElement.GetProperty("Guid").GetString();
                    var targetUsername = jsonObject.RootElement.GetProperty("Username").GetString();
                    DateTime timestamp = DateTime.Parse(jsonObject.RootElement.GetProperty("Timestamp").GetString());
                    var messagesJson = jsonObject.RootElement.GetProperty("Messages");

                    var messagesTemp = JsonSerializer.Deserialize<List<Message>>(messagesJson.ToString());
                    ObservableCollection<Message> messagesObservableCollection = new ObservableCollection<Message>(messagesTemp);
                    messagesObservableCollection.Add(message);

                    var data = new
                    {
                        Guid = guid,
                        Username = username,
                        Messages = messagesObservableCollection,
                        Timestamp = DateTime.Now,
                    };

                    JsonSerializerOptions jsonOptions = new() { WriteIndented = true };


                    string json = JsonSerializer.Serialize(data, jsonOptions);


                    await File.WriteAllTextAsync(filePath, json);

                    Trace.WriteLine($"Saving JSON to {filePath}");
                }
                else
                {
                    var data = new
                    {
                        Guid = guid,
                        Username = username,
                        Messages = messages,
                        Timestamp = DateTime.Today.Date,
                    };

                    JsonSerializerOptions jsonOptions = new() { WriteIndented = true };


                    string json = JsonSerializer.Serialize(data, jsonOptions);


                    await File.WriteAllTextAsync(filePath, json);

                    Trace.WriteLine($"Saving JSON to {filePath}");

                }
            }
        }


        static User GetUserByGuid(ObservableCollection<User> users, Guid targetGuid)
        {
            // Using LINQ to find the user by Guid
            return users.FirstOrDefault(user => user.Guid == targetGuid);
        }
    }
}
