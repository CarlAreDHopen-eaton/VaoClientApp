using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Vao.Client.Components;
using Vao.Client.Contracts;

namespace Vao.Client.Utility
{
   /// <summary>
   /// Support class for parsing JSON messages to .NET objects.
   /// </summary>
   public class JsonParser
   {
      /// <summary>
      /// Parses the JSON message text to a list of camera objects.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="flexApiClient">The client that received the message</param>
      /// <returns></returns>
      internal static List<Camera> ParseCameraList(string strJson, FlexApiClient flexApiClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            List<JsonCameraObject> list = JsonConvert.DeserializeObject<List<JsonCameraObject>>(strJson);
            List<Camera> returnList = new List<Camera>();
            foreach (JsonCameraObject camera in list)
            {
               returnList.Add(new Camera(camera.inputId, camera, flexApiClient));
            }
            return returnList;
         }
         return null;
      }

      /// <summary>
      /// Parses the JSON message text to a list of status message objects.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="flexApiClient">The client that received the message</param>
      /// <returns></returns>
      internal static List<StatusMessage> ParseStatusMessages(string strJson, FlexApiClient flexApiClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            List<JsonStatusMessage> list = JsonConvert.DeserializeObject<List<JsonStatusMessage>>(strJson);
            List<StatusMessage> returnList = new List<StatusMessage>();
            foreach (JsonStatusMessage statusMessage in list)
            {
               if (statusMessage.description.Equals("no entries", StringComparison.InvariantCultureIgnoreCase))
                  continue;             

               returnList.Add(new StatusMessage(statusMessage, flexApiClient));
            }
            return returnList;
         }
         return null; 
      }

      internal static Monitor ParseVideoOutput(string strJson, FlexApiClient flexApiClient, int iVideoOutput)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonVideoOutput jsonObject = JsonConvert.DeserializeObject<JsonVideoOutput>(strJson);
            Camera camera = flexApiClient.GetCamera(jsonObject.inputId);
            // Older API versions does not send the name.
            string name = string.IsNullOrEmpty(jsonObject.name) ? $"Monitor {iVideoOutput}" : jsonObject.name;
            Monitor monitor = new Monitor(iVideoOutput, name, flexApiClient, camera);
            return monitor;
         }
         return null;
      }

      /// <summary>
      /// Parses the JSON message text to a list of camera objects.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="flexApiClient">The client that received the message</param>
      /// <returns></returns>
      internal static List<Monitor> ParseVideoOutputList(string strJson, FlexApiClient flexApiClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            List<JsonVideoOutputEx> list = JsonConvert.DeserializeObject<List<JsonVideoOutputEx>>(strJson);
            List<Monitor> returnList = new List<Monitor>();
            foreach (JsonVideoOutputEx videoOutput in list)
            {
               Camera camera = flexApiClient.GetCameraList().GetByLocalNo(videoOutput.inputId, flexApiClient);
               returnList.Add(new Monitor(videoOutput.outputId, videoOutput.outputName, flexApiClient, camera));
            }
            return returnList;
         }
         return null;
      }



      /// <summary>
      /// Parses the JSON message text to a camera object.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="flexApiClient">The client that received the message</param>
      /// <returns></returns>
      internal static Camera ParseSingleCamera(string strJson, FlexApiClient flexApiClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonCameraObject oCameraObject = JsonConvert.DeserializeObject<JsonCameraObject>(strJson);
            return new Camera(oCameraObject.inputId, oCameraObject, flexApiClient);
         }

         return null;
      }

      /// <summary>
      /// Parses the JSON message text to a list of preset position message objects.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="flexApiClient">The client that received the message</param>
      /// <param name="ownerCamera">The camera that these presets belong to.</param>
      /// <returns></returns>
      internal static List<Preset> ParsePresetList(string strJson, FlexApiClient flexApiClient, Camera ownerCamera)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            List<JsonPresetObject> list = JsonConvert.DeserializeObject<List<JsonPresetObject>>(strJson);
            List<Preset> returnList = new List<Preset>();
            foreach (JsonPresetObject preset in list)
            {
               returnList.Add(new Preset(preset.presetId, preset, flexApiClient, ownerCamera));
            }
            return returnList;
         }
         return null;
      }

      /// <summary>
      /// Parses the JSON message text to a list of playback info message objects.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="flexApiClient">The client that received the message</param>
      /// <returns>A list of playback info message objects</returns>
      internal static List<PlaybackInfo> ParsePlaybackInfoList(string strJson, FlexApiClient flexApiClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            List<JsonPlaybackInfoObject> list = JsonConvert.DeserializeObject<List<JsonPlaybackInfoObject>>(strJson);
            List<PlaybackInfo> returnList = new List<PlaybackInfo>();
            foreach (JsonPlaybackInfoObject playbackInfo in list)
            {
               returnList.Add(new PlaybackInfo(playbackInfo, flexApiClient));
            }
            return returnList;
         }
         return null;
      }

      /// <summary>
      /// Parses the JSON message text to a list of playback info message objects.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="flexApiClient">The client that received the message</param>
      /// <returns>A list of playback info message objects</returns>
      internal static DownloadInfo ParseDownloadResponse(string strJson, FlexApiClient flexApiClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonDownloadResponseObject downloadResponseObject = JsonConvert.DeserializeObject<JsonDownloadResponseObject>(strJson);
            return new DownloadInfo(downloadResponseObject, flexApiClient);
         }

         return null;
      }

      /// <summary>
      /// Parses the JSON message text to an Api Version object.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="flexApiClient">The client that received the message</param>
      /// <returns></returns>
      internal static ApiVersion ParseApiVersion(string strJson, FlexApiClient flexApiClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonApiVersion apiVersion = JsonConvert.DeserializeObject<JsonApiVersion>(strJson);
            return new ApiVersion(apiVersion, flexApiClient);
         }

         return null;
      }

      internal static SystemInformation ParseImplementationVersion(string strJson, FlexApiClient flexApiClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonImplementationVersion implVersion = JsonConvert.DeserializeObject<JsonImplementationVersion>(strJson);
            return new SystemInformation(implVersion, flexApiClient);
         }

         return null;
      }

      /// <summary>
      /// Parses the JSON message text to a list of Alarm objects.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="flexApiClient">The client that received the message</param>
      /// <returns>
      /// A list of <see cref="Alarm"/> objects, or null if input is invalid
      /// </returns>
      internal static List<Alarm> ParseAlarmList(string strJson, FlexApiClient flexApiClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            List<JsonAlarmObject> list = JsonConvert.DeserializeObject<List<JsonAlarmObject>>(strJson);
            List<Alarm> returnList = new List<Alarm>();
            foreach (JsonAlarmObject alarm in list)
            {
               returnList.Add(new Alarm(alarm.alarmId, alarm, flexApiClient));
            }
            return returnList;
         }
         return null;
      }

      /// <summary>
      /// Parses the JSON message text to a single Alarm object.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="flexApiClient">The client that received the message</param>
      /// <returns>
      /// An <see cref="Alarm"/> instance, or null if input is invalid
      /// </returns>
      internal static Alarm ParseSingleAlarm(string strJson, FlexApiClient flexApiClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonAlarmObject alarmObject = JsonConvert.DeserializeObject<JsonAlarmObject>(strJson);
            return new Alarm(alarmObject.alarmId, alarmObject, flexApiClient);
         }

         return null;
      }

      /// <summary>
      /// Parses the JSON message text to a single User object.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="flexApiClient">The client that received the message</param>
      /// <returns>
      /// A <see cref="User"/> instance, or null if input is invalid
      /// </returns>
      internal static User ParseSingleUser(string strJson, FlexApiClient flexApiClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonUserObject userObject = JsonConvert.DeserializeObject<JsonUserObject>(strJson);
            return new User(userObject.userId, userObject, flexApiClient);
         }

         return null;
      }
   }
}
