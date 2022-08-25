using Json.Net;
using RestSharp;
using Vao.Client.Contracts;

namespace Vao.Client.Components
{
   public class Preset
   {
      private string mPresetName;
      private int mPresetNumber;
      private VaoClient mVaoClient;

      internal Preset(int number, JsonPresetObject jsonObject, VaoClient vaoClient)
      {
         mPresetNumber = number;
         mVaoClient = vaoClient;
         mPresetName = jsonObject.name;
      }

      #region Public Properties

      public string Name
      {
         get { return mPresetName; }
         set { mPresetName = value; }
      }

      public int PresetNumber
      {
         get { return mPresetNumber; }
         set { mPresetNumber = value; }
      }

      public Camera OwnerCamera
      {
         get;
         set;
      }
      #endregion

      #region Public Methods

      public bool GotoPreset()
      {
         RestResponse response = mVaoClient.MoveCameraToPreset(OwnerCamera.CameraNumber, PresetNumber);
         if (response != null && response.IsSuccessful)
         {
            return true;
         }
         return false;
      }

      #endregion
   }
}
