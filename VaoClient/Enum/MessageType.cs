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
   }
}
