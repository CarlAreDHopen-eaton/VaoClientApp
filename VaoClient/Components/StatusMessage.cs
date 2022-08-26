using System;
using Vao.Client.Components.Interfaces;
using Vao.Client.Contracts;
using Vao.Client.Enum;

namespace Vao.Client.Components
{
   public class StatusMessage : BaseComponent
   {
      #region Private Members
      
      private JsonStatusMessage mStatusMessage;

      #endregion

      #region Constructors

      /// <summary>
      /// Constructor for the message.
      /// </summary>
      /// <param name="statusMessage">The messages desrialzed to JSON</param>
      /// <param name="vaoClient">The client that received the message</param>
      internal StatusMessage(JsonStatusMessage statusMessage, VaoClient vaoClient) 
         : base(vaoClient) 
      {
         ReceivedTime = DateTime.Now;
         mStatusMessage = statusMessage;
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// The level of the message.
      /// </summary>
      public MessageLevel Level
      { 
         get
         {
            if (mStatusMessage.type.Equals("debug", StringComparison.InvariantCultureIgnoreCase))
               return MessageLevel.Debug;
            if (mStatusMessage.type.Equals("error", StringComparison.InvariantCultureIgnoreCase))
               return MessageLevel.Error;
            if (mStatusMessage.type.Equals("info", StringComparison.InvariantCultureIgnoreCase))
               return MessageLevel.Info;

            return MessageLevel.Info;
         }
      }
         
      /// <summary>
      /// The type of message.      
      /// </summary>
      public MessageType Type
      {
         get
         {
            // Session
            if (Message.Contains("REST Session started"))
               return MessageType.SessionStart;
            if (Message.Contains("Session logon success"))
               return MessageType.SessionLogonSuccess;
            if (Message.Contains("Session logon failed"))
               return MessageType.SessionLogonFail;

            // Camera main stream (Stream 1)
            if (Message.Contains("Camera video connection lost"))
               return MessageType.CameraVideoStream1Lost;
            if (Message.Contains("Camera video connection restored"))
               return MessageType.CameraVideoStream1Restored;

            // Camera sub stream (Stream 2)
            if (Message.Contains("Camera video sub stream connection lost"))
               return MessageType.CameraVideoStream2Lost;
            if (Message.Contains("Camera video sub stream connection restored"))
               return MessageType.CameraVideoStream2Restored;

            // Camera data
            if (Message.Contains("Camera data connection lost"))
               return MessageType.CameraDataLost;

            // Unknown messages.
            return MessageType.Unknown;
         }
      }

      /// <summary>
      /// The message text received from the VMS system.
      /// </summary>
      public string Message 
      { 
         get 
         { 
            return mStatusMessage.description; 
         } 
      }

      /// <summary>
      /// The message timestamp when the VMS system created this message.      
      /// Note: The time the message was received by the clinet is in <see cref="ReceivedTime"/>
      /// </summary>
      public string Timestamp
      { 
         get 
         { 
            return mStatusMessage.timestamp; 
         }
      }

      /// <summary>
      /// The time the message was received.
      /// </summary>
      public DateTime ReceivedTime { get; }

      #endregion
   }
}
