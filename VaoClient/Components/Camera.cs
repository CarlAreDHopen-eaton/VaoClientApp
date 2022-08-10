using System.Collections.Generic;
using Json.Net;
using RestSharp;
using Vao.Client.Contracts;

namespace Vao.Client.Components
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

      /// <summary>
      /// The name of the camera in the VMS system.
      /// </summary>
      public string Name
      {
         get { return mCameraName; }
      }

      /// <summary>
      /// The number of the camera in the VMS system.
      /// </summary>
      public int CameraNumber
      {
         get { return mCameraNumber; }
      }
      
      /// <summary>
      /// Gets the live stream RTSP url for the camera.
      /// Note, always call this method before connecting to the camera as the URL may change between calls.
      /// </summary>
      /// <param name="iStream">The stream number/quality, typically 1 for high quality and 2 for low quality.</param>
      /// <returns>The RTSP url for the camera live stream.</returns>
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
