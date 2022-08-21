using System;
using System.Threading;

namespace Vao.Client
{
   internal class FeedbackHandler
   {
      private Thread mStatusCheckThread;
      private VaoClient mClient;
      private readonly ManualResetEvent mStopStatusCheckThread = new ManualResetEvent(false);

      public FeedbackHandler(VaoClient vaoClient)
      {         
         mClient = vaoClient;
      }

      public bool Start()
      { 
         if (mStatusCheckThread == null)
         {
            mStatusCheckThread = new Thread(StatusCheckThread);
            mStatusCheckThread.IsBackground = true;
            mStatusCheckThread.Start();
            return true;
         }
         return false; 
      }

      private void StatusCheckThread(object obj)
      {
         DateTime lastCheck = DateTime.Now.AddDays(-1);
         while (!mStopStatusCheckThread.WaitOne(1000))
         {
            string rawMessages = mClient.GetVaoStatusMessages(lastCheck);
            if (!string.IsNullOrEmpty(rawMessages))
            {
               var statusMessages = Utility.JsonParser.ParseStatusMessages(rawMessages, mClient);
               if (statusMessages != null && statusMessages.Count > 0)
               {
                  foreach (var message in statusMessages)
                  {
                     HandleMessage(message);
                     RaiseOnMessageEvents(message);
                  }
               }
            }

            lastCheck = DateTime.Now;
         }
      }

      private void HandleMessage(Components.StatusMessage message)
      {
         // TODO Parse message to see if is a status message.         
      }

      private void RaiseOnMessageEvents(Components.StatusMessage message)
      {
         try
         {
            string strMessage = $"{message.Timestamp} : [{message.StatusType}][{message.MessageId}] {message.Message}";
            mClient.RaiseOnMessage(message.StatusType, strMessage);
         }
         catch
         {
            // Catch errors in case the event handler causes an exception.
         }
      }

      public bool Stop()
      {
         if (mStatusCheckThread != null)
         {
            mStopStatusCheckThread.Set();
            mStatusCheckThread.Join();
            mStatusCheckThread = null;
         }
         return false;
      }
   }
}
