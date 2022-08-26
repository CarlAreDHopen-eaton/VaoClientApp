using RestSharp;
using Vao.Client.Components.Interfaces;
using Vao.Client.Contracts;

namespace Vao.Client.Components
{
   public class Preset : BaseComponent, INamedComponent
   {
      private string mPresetName;
      private int mPresetNumber;
      private Camera mOwnerCamera;

      internal Preset(int number, JsonPresetObject jsonObject, VaoClient vaoClient, Camera ownerCamera)
         : base(vaoClient)
      {
         mPresetNumber = number;
         mPresetName = jsonObject.name;
         mOwnerCamera = ownerCamera;
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
         get { return mOwnerCamera; }
         set { mOwnerCamera = value; }
      }

      #endregion

      #region Public Methods

      public bool GotoPreset()
      {
         RestResponse response = VaoClient.MoveCameraToPreset(OwnerCamera.CameraNumber, PresetNumber);
         if (response != null && response.IsSuccessful)
         {
            return true;
         }
         return false;
      }

      public bool SetName(string newName)
      {
         return false;
      }

      #endregion
   }
}
