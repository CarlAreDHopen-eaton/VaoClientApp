using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Json.Net;
using RestSharp;
using Vao.Client.Contracts;
using Vao.Client.Enum;
using Vao.Client.Utility;

namespace Vao.Client.Components
{
   public class Camera : INotifyPropertyChanged
   {
      // ReSharper disable once MemberInitializerValueIgnored
      private string mCameraName = string.Empty;
      private int mCameraNumber = 0;
      private VaoClient mVaoClient;
      private bool mCameraVideoStream1Ok = true;
      private bool mCameraVideoStream2Ok = true;
      private bool mCameraDataOk = true;
      private int mCurrentZoomSpeed;
      private int mCurrentPanSpeed;
      private int mCurrentTiltSpeed;
      private string mCurrentFocus = "auto";

      internal Camera(int cameraNumber, JsonCameraObject camera, VaoClient vaoClient)
      {
         mCameraNumber = cameraNumber;
         mVaoClient = vaoClient;
         mCameraName = camera.name;
      }

      #region Public Events

      public event PropertyChangedEventHandler PropertyChanged;

      #endregion

      #region Public Properties

      /// <summary>
      /// The name of the camera in the VMS system.
      /// </summary>
      public string Name
      {
         get { return mCameraName; }
         internal set { mCameraName = value; }
      }

      /// <summary>
      /// The number of the camera in the VMS system.
      /// </summary>
      public int CameraNumber
      {
         get { return mCameraNumber; }
      }

      /// <summary>
      /// Get the camera data status for this camera.
      /// </summary>
      public bool CameraDataOk
      {
         get { return mCameraDataOk; }
         internal set
         {
            if (mCameraDataOk != value)
            {
               mCameraDataOk = value;
               NotifyPropertyChanged();
            }
         }
      }

      /// <summary>
      /// Get the camera video status for stream 1 for this camera.
      /// </summary>
      public bool CameraVideoStream1Ok
      {
         get { return mCameraVideoStream1Ok; }
         internal set
         {
            if (mCameraVideoStream1Ok != value)
            {
               mCameraVideoStream1Ok = value;
               NotifyPropertyChanged();
            }
         }
      }

      /// <summary>
      /// Get the camera video status for stream 2 for this camera.
      /// </summary>
      public bool CameraVideoStream2Ok
      {
         get { return mCameraVideoStream2Ok; }
         internal set
         {
            if (mCameraVideoStream2Ok != value)
            {
               mCameraVideoStream2Ok = value;
               NotifyPropertyChanged();
            }
         }
      }

      /// <summary>
      /// List of preset positions on the camera.
      /// </summary>
      public List<Preset> PresetList { get; set; }

      #endregion

      #region Public Methods

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
            {
               return oCameraObject.stream1url;
            }
            if (iStream == 2)
            {
               // If the 2nd stream is available we revert back to stream 1.
               if (string.IsNullOrEmpty(oCameraObject.stream2url))
                  return oCameraObject.stream1url;

               return oCameraObject.stream2url;
            }
         }
         return "";
      }

      /// <summary>
      /// Starts panning the camera to the left.
      /// </summary>
      public void PanLeft(int speed)
      {
         mCurrentPanSpeed = -MathHelper.Clamp(speed, 0, 100);
         MoveTargetStart();
      }

      /// <summary>
      /// Starts panning the camera to the right.
      /// </summary>
      public void PanRight(int speed)
      {
         mCurrentPanSpeed = MathHelper.Clamp(speed, 0, 100);
         MoveTargetStart();
      }

      /// <summary>
      /// Stops the Pan/Tilt operation.
      /// </summary>
      public void PanTiltZoomStop()
      {
         mCurrentTiltSpeed = 0;
         mCurrentPanSpeed = 0;
         mCurrentZoomSpeed = 0;
         mCurrentFocus = "auto";
         RestResponse response = mVaoClient.MoveTargetStop(CameraNumber);
      }

      /// <summary>
      /// Stops the Pan/Tilt operation.
      /// </summary>
      public void PanStop(int speed)
      {
         mCurrentPanSpeed = MathHelper.Clamp(speed, 0, 100);
         MoveTargetStart();
      }

      /// <summary>
      /// Tilts the camera up.
      /// </summary>
      public void TiltUp(int speed)
      {
         mCurrentTiltSpeed = MathHelper.Clamp(speed, 0, 100);
         MoveTargetStart();
      }

      /// <summary>
      /// Tilts the camera down.
      /// </summary>
      public void TiltDown(int speed)
      {
         mCurrentTiltSpeed = -MathHelper.Clamp(speed, 0, 100);
         MoveTargetStart();
      }

      /// <summary>
      /// Start the camera zoom operation.
      /// </summary>
      public void ZoomIn(int speed)
      {
         mCurrentZoomSpeed = MathHelper.Clamp(speed, 0, 100);
         MoveTargetStart();
      }

      /// <summary>
      /// Start the camera zoom operation.
      /// </summary>
      public void ZoomOut(int speed)
      {
         mCurrentZoomSpeed = -MathHelper.Clamp(speed, 0, 100);
         MoveTargetStart();
      }

      /// <summary>
      /// Move focus near.
      /// </summary>
      public void FocusNear()
      {
         mCurrentFocus = "near";
         MoveTargetStart();
      }

      /// <summary>
      /// Move focus far.
      /// </summary>
      public void FocusFar()
      {
         mCurrentFocus = "far";
         MoveTargetStart();
      }

      #endregion

      #region Internal Methods

      internal void HandleStatusMessage(StatusMessage message)
      {
         switch (message.Type)
         {
            case MessageType.CameraVideoStream1Lost:
               CameraVideoStream1Ok = false;
               break;
            case MessageType.CameraVideoStream1Restored:
               CameraVideoStream1Ok = true;
               break;
            case MessageType.CameraVideoStream2Lost:
               CameraVideoStream2Ok = false;
               break;
            case MessageType.CameraVideoStream2Restored:
               CameraVideoStream2Ok = true;
               break;
            case MessageType.CameraDataLost:
               CameraDataOk = false;
               break;
            case MessageType.CameraDataRestored:
               CameraDataOk = true;
               break;
         }
      }

      #endregion

      #region Private Methods

      /// <summary>
      /// Starts move target operation
      /// </summary>
      /// <returns></returns>
      private RestResponse MoveTargetStart()
      {
         return mVaoClient.MoveTargetStart(CameraNumber, mCurrentPanSpeed, mCurrentTiltSpeed, mCurrentZoomSpeed, mCurrentFocus);
      }

      // This method is called by the Set accessor of each property.  
      // The CallerMemberName attribute that is applied to the optional propertyName  
      // parameter causes the property name of the caller to be substituted as an argument.  
      private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
      #endregion
   }
}
