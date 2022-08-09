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

      public Camera(int cameraNumber, string jsonData, VaoClient vaoClient)
      {
         mCameraNumber = cameraNumber;
         mVaoClient = vaoClient;

         Parse(jsonData);
      }

      public void Parse(string strJson)
      {
         if (!string.IsNullOrEmpty(strJson))
         {
            JsonCameraObject oCameraObject = JsonNet.Deserialize<JsonCameraObject>(strJson);
            mCameraName = oCameraObject.name;
         }
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
