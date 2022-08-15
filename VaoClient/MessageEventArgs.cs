using System;
using Vao.Client.Enum;

namespace Vao.Client
{
   public class MessageEventArgs : EventArgs
   {
      public MessageEventArgs(MessageType type, string message)
      {
         Type = type;
         Message = message;
      }

      public string Message { get; set; }

      public MessageType Type { get; set; }
   }
}