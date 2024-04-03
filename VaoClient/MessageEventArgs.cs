using System;
using Vao.Client.Components;
using Vao.Client.Enum;

namespace Vao.Client
{
   public class MessageEventArgs : EventArgs
   {
      public MessageEventArgs(MessageLevel type, string message, StatusMessage statusMessage)
      {
         Type = type;
         Message = message;
         StatusMessage = statusMessage;
      }

      public string Message { get; private set; }

      public MessageLevel Type { get; private set; }

      public StatusMessage StatusMessage { get; private set; }

      public bool IsStatusMessage { get { return StatusMessage != null; } }
   }
}