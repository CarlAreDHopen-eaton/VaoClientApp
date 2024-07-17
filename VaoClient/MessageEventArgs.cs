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

      /// <summary>
      /// The message text of this message.
      /// </summary>
      public string Message { get; private set; }

      /// <summary>
      /// The level of the message.
      /// </summary>
      public MessageLevel Level { get; private set; }

      /// <summary>
      /// The status message received from the system.
      /// </summary>
      public StatusMessage StatusMessage { get; private set; }

      /// <summary>
      /// Checks if the message was received from the system.
      /// </summary>
      public bool IsFromSystem { get { return StatusMessage != null; } }
   }
}