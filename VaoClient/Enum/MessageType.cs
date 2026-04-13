namespace Vao.Client.Enum
{
   /// <summary>
   /// Identifies the type of message.
   /// </summary>
   public enum MessageType
   {
      /// <summary>
      /// Unknown message.
      /// </summary>
      Unknown,
      /// <summary>
      /// Session started.
      /// </summary>
      SessionStart,
      /// <summary>
      /// Logon success.
      /// </summary>
      SessionLogonSuccess,
      /// <summary>
      /// Logon success.
      /// </summary>
      SessionLogonFail,
      /// <summary>
      /// Video stream 1 from the camera into the VMS system is lost.
      /// </summary>
      CameraVideoStream1Lost,
      /// <summary>
      /// Video stream 1 from the camera into the VMS system is restored.
      /// </summary>
      CameraVideoStream1Restored,
      /// <summary>
      /// Video stream 2 from the camera into the VMS system is lost.
      /// </summary>
      CameraVideoStream2Lost,
      /// <summary>
      /// Video stream 2 from the camera into the VMS system is restored.
      /// </summary>
      CameraVideoStream2Restored,
      /// <summary>
      /// Lost the data communication between the VMS system and the camera.
      /// </summary>
      CameraDataLost,
      /// <summary>
      /// Restored the data communication between the VMS system and the camera.
      /// </summary>
      CameraDataRestored,
      /// <summary>
      /// The extracted video is ready for download
      /// </summary>
      ExtractedVideoReadyForDownload,
      /// <summary>
      /// The HVR with recording is currently offline
      /// </summary>
      HvrWithRecordingIsOffline,
      /// <summary>
      /// There is no video in the requested download
      /// </summary>
      NoVideoInRequestedDownload,
      /// <summary>
      /// The camera is locked and cannot be operated.
      /// </summary>
      CameraLocked,
      /// <summary>
      /// The camera is unlocked and can be operated.
      /// </summary>
      CameraUnlocked,
      /// <summary>
      /// The alarm function for the device is disabled.
      /// </summary>
      AlarmStatusDisabled,
      /// <summary>
      /// The alarm reports a tamper condition.
      /// </summary>
      AlarmStatusTampered,
      /// <summary>
      /// The alarm is active.
      /// </summary>
      AlarmStatusActive,
      /// <summary>
      /// The alarm is inactive.
      /// </summary>
      AlarmStatusInactive,
      /// <summary>
      /// The alarm has been acknowledged.
      /// </summary>
      AlarmStatusAcknowledged
   }
}
