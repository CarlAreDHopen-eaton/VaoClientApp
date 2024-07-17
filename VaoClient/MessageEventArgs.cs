using System;
using Vao.Client.Components;
using Vao.Client.Enum;

namespace Vao.Client
{
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(MessageLevel level, string message, StatusMessage statusMessage)
        {
            Level = level;
            Message = message;
            StatusMessage = statusMessage;
        }

        public string Message { get; private set; }

        /// <summary>
        /// The level of the message.
        /// </summary>
        public MessageLevel Level { get; private set; }

        public StatusMessage StatusMessage { get; private set; }

        public bool IsStatusMessage { get { return StatusMessage != null; } }
    }
}