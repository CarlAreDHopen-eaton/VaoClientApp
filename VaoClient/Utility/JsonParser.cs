using System;
using System.Collections.Generic;
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
      /// <param name="vaoClient">The client that received the message</param>
      /// <returns></returns>
      internal static List<Camera> ParseCameraList(string strJson, VaoClient vaoClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            List<JsonCameraObject> list = JsonConvert.DeserializeObject<List<JsonCameraObject>>(strJson);
            List<Camera> returnList = new List<Camera>();
            foreach (JsonCameraObject camera in list)
            {
               returnList.Add(new Camera(camera.inputId, camera, vaoClient));
            }
            return returnList;
         }
         return null;
      }

      /// <summary>
      /// Parses the JSON message text to a list of status message objects.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="vaoClient">The client that received the message</param>
      /// <returns></returns>
      internal static List<StatusMessage> ParseStatusMessages(string strJson, VaoClient vaoClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            List<JsonStatusMessage> list = JsonConvert.DeserializeObject<List<JsonStatusMessage>>(strJson);
            List<StatusMessage> returnList = new List<StatusMessage>();
            foreach (JsonStatusMessage statusMessage in list)
            {
               if (statusMessage.description.Equals("no entries", StringComparison.InvariantCultureIgnoreCase))
                  continue;             

               returnList.Add(new StatusMessage(statusMessage, vaoClient));
            }
            return returnList;
         }
         return null; 
      }

      internal static Monitor ParseVideoOutput(string strJson, VaoClient vaoClient, int iVideoOutput)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonVideoOutput jsonObject = JsonConvert.DeserializeObject<JsonVideoOutput>(strJson);
            Camera camera = vaoClient.GetCamera(jsonObject.inputId);
            Monitor monitor = new Monitor(iVideoOutput, vaoClient, camera);
            return monitor;
         }
         return null;
      }

         /// <summary>
         /// Parses the JSON message text to a camera object.
         /// </summary>
         /// <param name="strJson">The JSON formatted message text</param>
         /// <param name="vaoClient">The client that received the message</param>
         /// <returns></returns>
         internal static Camera ParseSingleCamera(string strJson, VaoClient vaoClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonCameraObject oCameraObject = JsonConvert.DeserializeObject<JsonCameraObject>(strJson);
            return new Camera(oCameraObject.inputId, oCameraObject, vaoClient);
         }

         return null;
      }

      /// <summary>
      /// Parses the JSON message text to a list of preset position message objects.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="vaoClient">The client that received the message</param>
      /// <param name="ownerCamera">The camera that these presets belong to.</param>
      /// <returns></returns>
      internal static List<Preset> ParsePresetList(string strJson, VaoClient vaoClient, Camera ownerCamera)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            List<JsonPresetObject> list = JsonConvert.DeserializeObject<List<JsonPresetObject>>(strJson);
            List<Preset> returnList = new List<Preset>();
            foreach (JsonPresetObject preset in list)
            {
               returnList.Add(new Preset(preset.presetId, preset, vaoClient, ownerCamera));
            }
            return returnList;
         }
         return null;
      }

      /// <summary>
      /// Parses the JSON message text to a list of playback info message objects.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="vaoClient">The client that received the message</param>
      /// <returns>A list of playback info message objects</returns>
      internal static List<PlaybackInfo> ParsePlaybackInfoList(string strJson, VaoClient vaoClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            List<JsonPlaybackInfoObject> list = JsonConvert.DeserializeObject<List<JsonPlaybackInfoObject>>(strJson);
            List<PlaybackInfo> returnList = new List<PlaybackInfo>();
            foreach (JsonPlaybackInfoObject playbackInfo in list)
            {
               returnList.Add(new PlaybackInfo(playbackInfo, vaoClient));
            }
            return returnList;
         }
         return null;
      }

      /// <summary>
      /// Parses the JSON message text to a list of playback info message objects.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="vaoClient">The client that received the message</param>
      /// <returns>A list of playback info message objects</returns>
      internal static DownloadInfo ParseDownloadResponse(string strJson, VaoClient vaoClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonDownloadResponseObject downloadResponseObject = JsonConvert.DeserializeObject<JsonDownloadResponseObject>(strJson);
            return new DownloadInfo(downloadResponseObject, vaoClient);
         }

         return null;
      }

      /// <summary>
      /// Parses the JSON message text to an Api Version object.
      /// </summary>
      /// <param name="strJson">The JSON formatted message text</param>
      /// <param name="vaoClient">The client that received the message</param>
      /// <returns></returns>
      internal static ApiVersion ParseApiVersion(string strJson, VaoClient vaoClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonApiVersion apiVersion = JsonConvert.DeserializeObject<JsonApiVersion>(strJson);
            return new ApiVersion(apiVersion, vaoClient);
         }

         return null;
      }
   }
}
