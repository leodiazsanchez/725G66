namespace ChatApp.Model
{
    using ChatApp.Model.Utils;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using static ChatApp.Model.Utils.Message;

    public class Client
    {
        private readonly ConnectionManager cm;

        public Client(ConnectionManager cm)
        {
            this.cm = cm;
        }

        public async Task ConnectAsync(IPAddress ip, int port)
        {
            TcpClient tcpClient = new TcpClient();

            try
            {
                // Set a connection timeout
                using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                {
                    await tcpClient.ConnectAsync(ip, port).WaitAsync(cancellationTokenSource.Token);
                }

                Trace.WriteLine("Connected to the server.");

                string connectRequest = new ChatProtocol(cm.Guid, cm.Username, RequestType.Connect).Serialize();

                await SendMessageAsync(connectRequest, tcpClient);

                _ = ListenForMessagesAsync(tcpClient);
            }
            catch (OperationCanceledException)
            {
                throw new Exception("Connection timed out. No listener on the other side.");
            }
            catch (SocketException ex)
            {
                throw new Exception($"Socket error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Connection error: {ex.Message}");
                throw;
            }
        }


        private async Task SendMessageAsync(string message, TcpClient tcpClient)
        {
            if (tcpClient.Connected)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await tcpClient.GetStream().WriteAsync(data, 0, data.Length);
                Trace.WriteLine("Message sent to the server.");
            }
        }

        private async Task ListenForMessagesAsync(TcpClient tcpClient)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (tcpClient.Connected)
                {
                    int bytesRead = await tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        Trace.WriteLine("Connection closed by the server.");
                        break;
                    }

                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Trace.WriteLine("Received from server: " + response);

                    ChatProtocol protocol = ChatProtocol.Deserialize(response);

                    // Handle the server response
                    switch (protocol.Request)
                    {
                        case RequestType.Accept:
                            cm.ShowMessageBox("Chat request accepted", $"Your chat request to {protocol.Username} got accepted", MessageBoxButton.OK, MessageBoxImage.Information);
                            cm.AddConnection(new Utils.User(protocol.Guid, protocol.Username, DateTime.Now), tcpClient);

                            Trace.WriteLine("Acknowledgment received from server.");
                            break;

                        case RequestType.Decline:
                            cm.ShowMessageBox("Chat request declined", $"Your chat request to {protocol.Username} got declined", MessageBoxButton.OK, MessageBoxImage.Stop);
                            break;

                        case RequestType.Refuesed:
                            cm.ShowMessageBox("Chat request refuesed", $"You already have an ongoing chat with {protocol.Username}", MessageBoxButton.OK, MessageBoxImage.Stop);
                            break;

                        case RequestType.Message:
                            if (protocol.Message != null)
                            {
                                Message message = new Message(protocol.Message, ConnectionDirection.Incoming);
                                cm.AddMessage(protocol.Guid, message);
                                await cm.SaveMessagesToJsonAsync(protocol.Guid, message);
                            }
                            break;

                        case RequestType.Buzz:
                            cm.Buzz();
                            break;

                        case RequestType.Disconnect:
                            cm.ShowMessageBox("User disconnected", $"{protocol.Username} disconnected", MessageBoxButton.OK, MessageBoxImage.Information);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                cm.RemoveConnection(protocol.Guid);
                            });
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in message listening: {ex.Message}");
            }
            finally
            {

                if (tcpClient != null)
                {
                    foreach (var connection in cm.Connections)
                    {
                        if (connection.Value.Equals(tcpClient))
                        {
                            cm.RemoveConnection(connection.Key);
                            break;
                        }
                    }
                    tcpClient.Close();
                    tcpClient.Dispose();
                }
                Trace.WriteLine("Disconnected from the server.");
            }

        }

    }
}
