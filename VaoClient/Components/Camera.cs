using System.Collections.Generic;
using VaoClient.Contracts;
using Json.Net;
using RestSharp;

namespace VaoClient.Components
{
   public class Camera
   {
      private string mCameraName = "";
      private int mCameraNumber = 0;
      private VaoClient mVaoClient;

      internal Camera(int cameraNumber, JsonCameraObject camera, VaoClient vaoClient)
      {
         mCameraNumber = cameraNumber;
         mVaoClient = vaoClient;
         mCameraName = camera.name;
      }

      public static List<Camera> ParseCameraList(string strJson, VaoClient vaoClient)
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

      public static Camera ParseSingleCamera(string strJson, VaoClient vaoClient)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonCameraObject oCameraObject = JsonNet.Deserialize<JsonCameraObject>(strJson);
            return new Camera(oCameraObject.inputId, oCameraObject, vaoClient);
         }

         return null;
      }
      public string Name
      {
         get { return mCameraName; }
      }

      public int CameraNumber
      {
         get { return mCameraNumber; }
      }

      public string GetCameraLiveStreamUrl(int iStream)
      {
         RestResponse response = mVaoClient.GetVaoCameraInternal(CameraNumber);
         if (response != null && response.IsSuccessful)
         {
            JsonCameraObject oCameraObject = JsonNet.Deserialize<JsonCameraObject>(response.Content);
            if (iStream == 1)
               return oCameraObject.stream1url;
            if (iStream == 2)
               return oCameraObject.stream2url;
         }
         return "";
      }
   }
}
