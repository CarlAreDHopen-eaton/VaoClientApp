﻿using System;
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
         DateTime lastCheck = DateTime.Now;
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
                     string strMessage = $"{message.Timestamp} : [{message.StatusType}] {message.Message}";
                     mClient.RaiseOnMessage(message.StatusType, strMessage);
                  }
               }
            }

            lastCheck = DateTime.Now;
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