﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Vao.Client.Components;
using Vao.Client.Enum;

namespace Vao.Client
{
   internal class FeedbackHandler
   {
      private Thread mStatusCheckThread;
      private readonly VaoClient mClient;
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
         DateTime lastCheck = new DateTime(1980, 1, 1);
         while (!mStopStatusCheckThread.WaitOne(1000))
         {
            string rawMessages = mClient.GetStatusMessages(lastCheck);
            if (!string.IsNullOrEmpty(rawMessages))
            {
               var statusMessages = Utility.JsonParser.ParseStatusMessages(rawMessages, mClient);
               if (statusMessages != null && statusMessages.Count > 0)
               {
                  foreach (var message in statusMessages)
                  {
                     HandleMessage(message);
                     RaiseOnMessageEvents(message);
                     
                     DateTime.TryParse(message.Timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime checkDate);
                     if (checkDate > lastCheck)
                        lastCheck = checkDate;
                  }
               }
            }
         }
      }

      private void HandleMessage(StatusMessage message)
      {
         switch (message.Type)
         {
            case MessageType.CameraDataLost:
            case MessageType.CameraDataRestored:
            case MessageType.CameraVideoStream1Lost:
            case MessageType.CameraVideoStream1Restored:
            case MessageType.CameraVideoStream2Lost:
            case MessageType.CameraVideoStream2Restored:
               List<Camera> cameras = message.VaoClient.GetCameraList();
               Camera foundCamera = null;
               foreach (Camera camera in cameras)
               {
                  if (message.Message.Contains(camera.Name))
                  {
                     foundCamera = camera;
                     break;
                  }
               }

               if (foundCamera != null)
               {
                  foundCamera.UpdateCameraStatus(message);
               }
               break;
         }
      }

      private void RaiseOnMessageEvents(StatusMessage message)
      {
         try
         {
            string strMessage = $"{message.Timestamp} : [{message.Level}][{message.Type}] {message.Message}";
            mClient.RaiseOnMessage(message.Level, strMessage, message);
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
