using System;

namespace Vao.Client
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