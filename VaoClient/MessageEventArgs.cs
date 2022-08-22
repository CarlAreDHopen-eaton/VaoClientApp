using System;
using Vao.Client.Enum;

namespace Vao.Client
{
   public class MessageEventArgs : EventArgs
   {
      public MessageEventArgs(MessageLevel type, string message)
      {
         Type = type;
         Message = message;
      }

      public string Message { get; set; }

      public MessageLevel Type { get; set; }
   }
}