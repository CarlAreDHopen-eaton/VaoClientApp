using System;

namespace VaoClient
{
   public class ConnectEventArgs : EventArgs
   {
      public ConnectEventArgs(string message)
      {
         Message = message;
      }

      public string Message { get; set; }
   }
}