using System;

namespace ChatApp.Model.Utils
{
    public class Message
    {
        public enum ConnectionDirection
        {
            Incoming,
            Outgoing
        }

        public string Content { get; set; }
        public ConnectionDirection Direction { get; set; }

        public DateTime TimeStamp { get; set; }

        public Message(string content, ConnectionDirection direction)
        {
            Content = content;
            Direction = direction;
            TimeStamp = DateTime.Now;
        }
    }
}
