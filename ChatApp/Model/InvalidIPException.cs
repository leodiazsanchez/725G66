using System;

namespace ChatApp.Model
{
    class InvalidIPException : Exception
    {
        public override string Message { get; } = "Invalid IP address!";
    }
}
