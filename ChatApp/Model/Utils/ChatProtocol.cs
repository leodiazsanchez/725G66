using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatApp.Model.Utils
{
    public enum RequestType
    {
        Accept,
        Decline,
        Message,
        Buzz,
        Connect,
        Disconnect,
        Refuesed,
    }

    public class ChatProtocol
    {
        private Guid guid;
        private string? message;
        private readonly DateTime date;
        private readonly RequestType request;
        private string username;

        [JsonConstructor]
        public ChatProtocol(Guid guid, string username, RequestType request, string? message = null)
        {
            this.guid = guid;
            this.username = username;
            this.request = request;
            this.message = message;
            date = DateTime.Now;
        }

        public Guid Guid => guid;
        public string? Message => message;
        public DateTime Date => date;
        public RequestType Request => request;
        public string Username => username;


        public string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }

        public static ChatProtocol Deserialize(string json)
        {
            return JsonSerializer.Deserialize<ChatProtocol>(json);
        }
    }

}
