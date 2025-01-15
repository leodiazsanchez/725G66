using System;

namespace ChatApp.Model.Utils
{
    public class User
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }

        public User(Guid guid, string name, DateTime timestamp)
        {
            Guid = guid;
            Name = name;
            Timestamp = timestamp;
        }
    }


}
