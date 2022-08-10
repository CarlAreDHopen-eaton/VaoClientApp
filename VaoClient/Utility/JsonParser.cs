using System;
using System.Collections.Generic;
using System.Text;
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

      internal static Camera ParseSingleCamera(string strJson, VaoClient vaoClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonCameraObject oCameraObject = JsonNet.Deserialize<JsonCameraObject>(strJson);
            return new Camera(oCameraObject.inputId, oCameraObject, vaoClient);
         }

         return null;
      }
   }
}
