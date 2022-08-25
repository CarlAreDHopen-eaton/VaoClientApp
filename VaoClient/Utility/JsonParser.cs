using System;
using System.Collections.Generic;
using Json.Net;
using Vao.Client.Components;
using Vao.Client.Contracts;

namespace Vao.Client.Utility
{
   public class JsonParser
   {
      internal static List<Camera> ParseCameraList(string strJson, VaoClient vaoClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            List<JsonCameraObject> list = JsonNet.Deserialize<List<JsonCameraObject>>(strJson);
            List<Camera> returnList = new List<Camera>();
            foreach (JsonCameraObject camera in list)
            {
               returnList.Add(new Camera(camera.inputId, camera, vaoClient));
            }
            return returnList;
         }
         return null;
      }

      internal static List<StatusMessage> ParseStatusMessages(string strJson, VaoClient vaoClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            List<JsonStatusMessage> list = JsonNet.Deserialize<List<JsonStatusMessage>>(strJson);
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

      internal static Camera ParseSingleCamera(string strJson, VaoClient vaoClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonCameraObject oCameraObject = JsonNet.Deserialize<JsonCameraObject>(strJson);
            return new Camera(oCameraObject.inputId, oCameraObject, vaoClient);
         }

         return null;
      }

      internal static List<Preset> ParsePresetList(string strJson, VaoClient vaoClient, Camera ownerCamera)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            List<JsonPresetObject> list = JsonNet.Deserialize<List<JsonPresetObject>>(strJson);
            List<Preset> returnList = new List<Preset>();
            foreach (JsonPresetObject preset in list)
            {
               returnList.Add(new Preset(preset.presetId, preset, vaoClient, ownerCamera));
            }
            return returnList;
         }
         return null;
      }
   }
}
