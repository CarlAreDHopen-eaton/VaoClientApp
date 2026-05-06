using System;
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
      private readonly FlexApiClient mClient;
      private readonly ManualResetEvent mStopStatusCheckThread = new ManualResetEvent(false);

      public FeedbackHandler(FlexApiClient flexApiClient)
      {         
         mClient = flexApiClient;
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
            {
               Camera camera = FindCamera(message);
               if (camera != null)
               {
                  camera.UpdateCameraStatus(message);
               }
               break;
            }

            case MessageType.CameraLocked:
            case MessageType.CameraUnlocked:
            {
               Camera camera = FindCamera(message);
               if (camera != null)
               {
                  camera.UpdateCameraData();
               }
               break;
            }

            case MessageType.AlarmStatusDisabled:
            case MessageType.AlarmStatusTampered:
            case MessageType.AlarmStatusActive:
            case MessageType.AlarmStatusInactive:
            case MessageType.AlarmStatusAcknowledged:
            {
               Alarm alarm = FindAlarm(message);
               if (alarm != null)
               {
                  alarm.UpdateAlarmStatus(message);
               }
               break;
            }
         }
      }

      private static Camera FindCamera(StatusMessage message)
      {
         List<Camera> cameras = message.FlexApiClient.GetCameraList();

         foreach (Camera camera in cameras)
         {
            if (message.Message.Contains($"Camera_{camera.ComponentNumber}"))
            {
               return camera;
            }
         }

         return null;
      }

      private static Alarm FindAlarm(StatusMessage message)
      {
         List<Alarm> alarms = message.FlexApiClient.GetAlarmList();

         foreach (Alarm alarm in alarms)
         {
            if (message.Message.Contains($"Alarm_{alarm.ComponentNumber}"))
            {
               return alarm;
            }
         }

         return null;
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
