﻿using RestSharp;
using Vao.Client.Components.Interfaces;
using Vao.Client.Contracts;
using Vao.Client.Utility;

namespace Vao.Client.Components
{
   public class Preset : BaseComponent, INamedComponent
   {
      #region Private Members

      private string mPresetName;
      private Camera mOwnerCamera;

      #endregion

      #region Constructor

      internal Preset(int number, JsonPresetObject jsonObject, VaoClient vaoClient, Camera ownerCamera)
         : base(vaoClient, number)
      {
         mPresetName = jsonObject.name;
         mOwnerCamera = ownerCamera;
      }

      #endregion

      #region Public Properties

      public string Name
      {
         get { return mPresetName; }
      }

      public Camera OwnerCamera
      {
         get { return mOwnerCamera; }
         set { mOwnerCamera = value; }
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Moves the camera to the preset position.
      /// </summary>
      /// <returns></returns>
      public bool GotoPreset()
      {
         int iPreset = ComponentNumber;
         int iCamera = OwnerCamera.ComponentNumber;
         RestResponse response = VaoClient.MoveCameraToPreset(iCamera, iPreset);
         if (response != null && response.IsSuccessful)
         {
            return true;
         }
         return false;
      }

      /// <summary>
      /// Sets the name of the preset position.
      /// </summary>
      /// <param name="newName"></param>
      /// <returns></returns>
      public bool SetName(string newName)
      {
         //TODO implement.
         mPresetName = newName;
         return false;
      }

      /// <summary>
      /// Deletes the preset position.
      /// </summary>
      /// <returns></returns>
      public bool Delete()
      { return false;}

      /// <summary>
      /// Sets the preset position
      /// </summary>
      /// <returns></returns>
      public bool Set()
      { return false; }


      public override string ToString()
      {
         return Name;
      }
      #endregion
   }
}
