namespace Vao.Client.Enum
{
   /// <summary>
   /// Identifiest the type of message.
   /// </summary>
   public enum MessageId
   {
      /// <summary>
      /// Unknown message.
      /// </summary>
      Unknown,
      /// <summary>
      /// Session started.
      /// </summary>
      SesionStart,
      /// <summary>
      /// Logon success.
      /// </summary>
      SesionLogonSuccess,
      /// <summary>
      /// Status of video stream from the camera into the VMS system.
      /// </summary>
      CameraVideoStatus,
      /// <summary>
      /// Status of the data commnication between the VMS system and the camera.
      /// </summary>
      CameraDataStatus,
   }
}
