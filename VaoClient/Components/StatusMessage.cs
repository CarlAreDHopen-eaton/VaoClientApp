using System;
using Vao.Client.Contracts;
using Vao.Client.Enum;

namespace Vao.Client.Components
{
   internal class StatusMessage
   {
      private JsonStatusMessage mStatusMessage { get; }

      public StatusMessage(JsonStatusMessage statusMessage, VaoClient vaoClient)
      {
         mStatusMessage = statusMessage;
         VaoClient = vaoClient;
      }

      public VaoClient VaoClient { get; }

      public MessageType StatusType
      { 
         get
         {
            if (mStatusMessage.type.Equals("debug", StringComparison.InvariantCultureIgnoreCase))
               return MessageType.Debug;
            if (mStatusMessage.type.Equals("error", StringComparison.InvariantCultureIgnoreCase))
               return MessageType.Error;
            if (mStatusMessage.type.Equals("info", StringComparison.InvariantCultureIgnoreCase))
               return MessageType.Info;

            return MessageType.Info;
         }
      }
         
      public MessageId MessageId
      {
         get
         {
            if (Message.Contains("REST Session started"))
               return MessageId.SesionStart;
            if (Message.Contains("Session logon success"))
               return MessageId.SesionLogonSuccess;
            //TODO Add further classification
            return MessageId.Unknown;
         }
      }


      public string Message 
      { 
         get 
         { 
            return mStatusMessage.description; 
         } 
      }

      public string Timestamp
      { 
         get 
         { 
            return mStatusMessage.timestamp; 
         }
      }
   }
}
