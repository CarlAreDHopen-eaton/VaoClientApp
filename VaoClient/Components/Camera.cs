using System;
using System.Collections.Generic;
using RestSharp;
using Vao.Client.Components.Interfaces;
using Vao.Client.Contracts;
using Vao.Client.Enum;
using Vao.Client.Utility;

namespace Vao.Client.Components
{
   public class Camera : BaseComponent, INamedComponent
   {
      // ReSharper disable once MemberInitializerValueIgnored
      private string mCameraName = string.Empty;
      private bool mCameraVideoStream1Ok = true;
      private bool mCameraVideoStream2Ok = true;
      private bool mCameraDataOk = true;
      private int? mCurrentZoomSpeed;
      private int? mCurrentPanSpeed;
      private int? mCurrentTiltSpeed;
      private string mCurrentFocus = "auto";
      private List<Preset> mPresetList;
      private Dictionary<Guid, List<PlaybackInfo>> mPlaybackInfoList = new Dictionary<Guid, List<PlaybackInfo>>();
      private JsonCameraObject mJsonCameraObject;
      private List<string> mFeatureList = new List<string>();
      private bool mHasLensControl = false;
      private bool mHasPanTiltControl = false;
      private bool mHasWipeWashControl = false;
      private bool mIslocked = false;
      private int mPriority = 0;
      private bool? mCanUnlock = null;
      private string mLockOwner = null;

      internal Camera(int cameraNumber, JsonCameraObject camera, FlexApiClient flexApiClient)
         : base(flexApiClient, cameraNumber)
      {
         mCameraName = camera.name;
         mJsonCameraObject = camera;
         mFeatureList.Clear();
         mFeatureList.AddRange(camera.features);
      }

      #region Events

      public event EventHandler<EventArgs> LockStatusChanged;

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
      /// The resolution of stream 1. (Typically higher resolution resolution than stream 2)
      /// </summary>
      public string Stream1Resolution { get; internal set; }

      /// <summary>
      /// The resolution of stream 2. (Typically lower resolution than stream 1)
      /// </summary>
      public string Stream2Resolution { get; internal set; }

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
      public List<Preset> PresetList
      {
         get
         {
            if (mPresetList == null)
               mPresetList = FlexApiClient.ExecuteGetPresetList(this);
            return mPresetList;
         }
      }

      /// <summary>
      /// List of recordings
      /// </summary>
      public List<PlaybackInfo> GetPlaybackInfoList(Guid viewerId)
      {
         if (!mPlaybackInfoList.ContainsKey(viewerId))
         {
            List<PlaybackInfo> playbackInfoList = FlexApiClient.ExecuteGetRecordingList(this, viewerId);
            mPlaybackInfoList.Add(viewerId, playbackInfoList);
         }       
         return mPlaybackInfoList[viewerId];
      }
      /// <summary>
      /// Gets whether the camera has Lens control
      /// </summary>
      public bool HasLensControl
      {
         get
         {
            return mHasLensControl;
         }
         internal set
         {
            if (mHasLensControl != value)
            {
               mHasLensControl = value;
               NotifyPropertyChanged();
            }
         }
      }
      /// <summary>
      /// Gets whether the camera has wipe and wash control
      /// </summary>
      public bool HasWipeWashControl
      {
         get
         {
            return mHasWipeWashControl;
         }
         internal set
         {
            if (mHasWipeWashControl != value)
            {
               mHasWipeWashControl = value;
               NotifyPropertyChanged();
            }
         }
      }
      /// <summary>
      /// Gets whether the camera has pan and tilt control
      /// </summary>
      public bool HasPanTiltControl
      {
         get
         {
            return mHasPanTiltControl;
         }
         internal set
         {
            if (mHasPanTiltControl != value)
            {
               mHasPanTiltControl = value;
               NotifyPropertyChanged();
            }
         }
      }
      /// <summary>
      /// Gets the priority of the current user has on the camera 
      /// </summary>
      public int Priority
      {
         get
         {
            return mPriority;
         }
         internal set
         {
            if (mPriority != value)
            {
               mPriority = value;
               NotifyPropertyChanged();
            }
         }
      }
      /// <summary>
      /// Indicates whether the camera is locked
      /// </summary>
      public bool IsLocked
      {
         get
         {
            return mIslocked;
         }
         private set
         {
            if (mIslocked != value)
            {
               mIslocked = value;
               NotifyPropertyChanged();
            }
         }
      }
      /// <summary>
      /// Indicates whether the logged-in user has access to unlock the camera
      /// </summary>
      public bool? CanUnlock
      {
         get
         {
            return mCanUnlock;
         }
         internal set
         {
            if (mCanUnlock != value)
            {
               mCanUnlock = value;
               NotifyPropertyChanged();
            }
         }
      }
      /// <summary>
      /// The owner of the lock on the camera
      /// </summary>
      public string LockOwner
      {
         get
         {
            return mLockOwner;
         }
         internal set
         {
            if (mLockOwner != value)
            {
               mLockOwner = value;
               NotifyPropertyChanged();
            }
         }
      }

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
         // Request new camera data in case redundant video server has taken over.
         UpdateCameraData();

         // Return correct url.
         if (iStream == 1)
         {
            return mJsonCameraObject.stream1url;
         }
         if (iStream == 2)
         {
            // If the 2nd stream is available we revert back to stream 1.
            if (string.IsNullOrEmpty(mJsonCameraObject.stream2url))
               return mJsonCameraObject.stream1url;

            return mJsonCameraObject.stream2url;
         }
         return "";
      }
      /// <summary>
      /// Updates the internal data of the camera
      /// </summary>
      public void UpdateCameraData()
      {
         RestResponse response = FlexApiClient.ExecuteGetCameraInternal(ComponentNumber);
         if (response != null && response.IsSuccessful)
         {
            Camera newCameraData = JsonParser.ParseSingleCamera(response.Content, FlexApiClient);
            // Update own data in case other properties has changed.
            UpdateData(newCameraData);
         }
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
         // Set to null since MoveTargetStop stops everything completely.
         mCurrentTiltSpeed = null;
         mCurrentPanSpeed = null;
         mCurrentZoomSpeed = null;
         mCurrentFocus = null;
         RestResponse response = FlexApiClient.ExecuteMoveTargetStop(ComponentNumber);
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

      /// <summary>
      /// Sets a new name for the camera.
      /// </summary>
      /// <param name="newName">The new name</param>
      /// <returns>True is success</returns>
      public bool SetName(string newName)
      {
         return FlexApiClient.ExecuteSetCameraName(ComponentNumber, newName);
      }

      /// <summary>
      /// Locks the camera for a specified duration. During the lock duration, only the user that has locked the camera can control it.
      /// </summary>
      /// <param name="duration">The duration for which the camera should be locked, in ISO 8601 format (e.g., "PT60S" for 60 seconds).</param>
      /// <returns><see langword="true"/> if the lock operation was successful; otherwise, <see langword="false"/>.</returns>
      public bool Lock(string duration = "PT60S")
      {
         RestResponse response = FlexApiClient.SendLockCamera(ComponentNumber, duration.ToString());
         return response?.IsSuccessStatusCode ?? false;
      }

      /// <summary>
      /// Unlocks the camera.
      /// </summary>
      /// <returns><see langword="true"/> if the unlock operation was successful; otherwise, <see langword="false"/>.</returns>
      public bool Unlock()
      {
         RestResponse response = FlexApiClient.SendUnlockCamera(ComponentNumber);
         return response?.IsSuccessStatusCode ?? false;
      }


      #endregion

      #region Internal Methods

      internal void UpdateCameraStatus(StatusMessage message)
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
            case MessageType.CameraLocked:
               IsLocked = true;
               break;
            case MessageType.CameraUnlocked:
               IsLocked = false;
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
         return FlexApiClient.ExecuteMoveTargetStart(ComponentNumber, mCurrentPanSpeed, mCurrentTiltSpeed, mCurrentZoomSpeed, mCurrentFocus);
      }

      /// <summary>
      /// Updates the internal data for the camera.
      /// </summary>
      /// <param name="camera"></param>
      internal void UpdateData(Camera camera)
      {
         if (camera != null && camera.ComponentNumber == ComponentNumber)
         {
            JsonCameraObject jsonCameraObject = camera.mJsonCameraObject;
            if (jsonCameraObject != null)
            {
               Name = jsonCameraObject.name;
               Stream1Resolution = jsonCameraObject.stream1resolution;
               Stream2Resolution = jsonCameraObject.stream2resolution;

               bool bHasPanTiltControl = false;
               bool bHasLensControl = false;
               bool bHasWipeWashControl = false;

               foreach (string feature in jsonCameraObject.features)
               {
                  switch (feature)
                  {
                     case "pan":
                     case "tilt":
                        bHasPanTiltControl = true;
                        break;
                     case "iris":
                     case "zoom":
                     case "focus":
                        bHasLensControl = true;
                        break;
                     case "wipe":
                     case "wash":
                        bHasWipeWashControl = true;
                        break;
                     default:
                        break;
                  }
               }
               HasLensControl = bHasLensControl;
               HasWipeWashControl= bHasWipeWashControl;
               HasPanTiltControl = bHasPanTiltControl;
               Priority = jsonCameraObject.priority;
               UpdateLockState(jsonCameraObject.locked, jsonCameraObject.lockOwner, jsonCameraObject.canUnlock);
               mJsonCameraObject = jsonCameraObject;
            }
         }
      }

      public override string ToString()
      {
         return Name;
      }

      private void UpdateLockState(bool isLocked, string lockOwner, bool? canUnlock)
      {
         IsLocked = isLocked;
         LockOwner = lockOwner;
         CanUnlock = canUnlock;

         LockStatusChanged?.Invoke(this, EventArgs.Empty);
      }
      #endregion
   }
}
