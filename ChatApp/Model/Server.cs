using ChatApp.Model;
using ChatApp.Model.Utils;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static ChatApp.Model.Utils.ChatProtocol;
using static ChatApp.Model.Utils.Message;

public class Server
{
    private readonly TcpListener listener;
    private readonly ConnectionManager cm;

    public Server(ConnectionManager cm)
    {
        this.cm = cm;
        listener = new TcpListener(cm.IP, cm.Port);

        _ = StartAsync(); // Start the listener asynchronously
    }

    public async Task StartAsync()
    {
        listener.Start();
        Trace.WriteLine("Listening for incoming connections...");

        try
        {
            while (true)
            {
                // Wait for a client to connect asynchronously
                TcpClient client = await listener.AcceptTcpClientAsync();
                Trace.WriteLine($"Connected to {client.Client.RemoteEndPoint}");

                // Handle the new client connection
                _ = HandleClientAsync(client);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error in server: {ex.Message}");
        }
        finally
        {
            // Ensure the listener is stopped when server shuts down
            listener.Stop();
            Trace.WriteLine("Server stopped.");
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (client.Connected)
            {
                int size = await stream.ReadAsync(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, size);
                Trace.WriteLine("Message: " + message);
                ChatProtocol protocol = Deserialize(message);

                if (size == 0)
                {

                    break; // Client disconnected
                }

                // Process the received message
                await ProcessMessageAsync(protocol, client);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error while handling client: {ex.Message}");
        }
        finally
        {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
            }
            if (client != null)
            {
                foreach (var connection in cm.Connections)
                {
                    if (connection.Value.Equals(client))
                    {
                        cm.RemoveConnection(connection.Key);
                        break;
                    }
                }
                client.Close();
                client.Dispose();
            }
            Trace.WriteLine("Disconnected from the client");
        }
    }

    private async Task ProcessMessageAsync(ChatProtocol protocol, TcpClient client)
    {
        switch (protocol.Request)
        {
            case RequestType.Connect:
                await HandleConnectAsync(protocol, client);
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

    private async Task HandleConnectAsync(ChatProtocol protocol, TcpClient tcpClient)
    {
        if (cm.Connections.ContainsKey(protocol.Guid))
        {
            string declineMessage = new ChatProtocol(cm.Guid, cm.Username, RequestType.Refuesed).Serialize();
            byte[] messageBytes = Encoding.UTF8.GetBytes(declineMessage);

            await tcpClient.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
        }
        else
        {
            cm.ShowMessageBox("Incoming request", $"Do you want to accept incoming chat request from {protocol.Username}?", MessageBoxButton.YesNo,
                MessageBoxImage.Question, async result =>
                {
                    if (result == MessageBoxResult.Yes)
                    {
                        // Handle the new connection
                        User user = new User(protocol.Guid, protocol.Username, DateTime.Now);
                        cm.AddConnection(user, tcpClient);

                        Trace.WriteLine($"Received connection from {protocol.Guid}");

                        // Send ack message back to the client
                        string acceptMessage = new ChatProtocol(cm.Guid, cm.Username, RequestType.Accept).Serialize();
                        byte[] messageBytes = Encoding.UTF8.GetBytes(acceptMessage);

                        await tcpClient.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                    }
                    else
                    {
                        string declineMessage = new ChatProtocol(cm.Guid, cm.Username, RequestType.Decline).Serialize();
                        byte[] messageBytes = Encoding.UTF8.GetBytes(declineMessage);

                        await tcpClient.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                    }
                });
        }
    }

}
