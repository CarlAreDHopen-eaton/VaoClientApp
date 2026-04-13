namespace Vao.Client.Enum
{
   /// <summary>
   ///  The level of the message.
   /// </summary>
   public enum AlarmGeneralStatus
   {
      /// <summary>
      /// Alarm is triggered
      /// </summary>
      Active = 1,
      /// <summary>
      /// Alarm is inactive/not triggered
      /// </summary>
      Inactive = 2,
      /// <summary>
      /// Alarm is triggered and acknowledged
      /// </summary>
      Acknowledged = 3,
      /// <summary>
      /// Alarm is tampered
      /// </summary>
      Tampered = 4,
      /// <summary>
      /// Alarm is disabled
      /// </summary>
      Disabled = 5,
      /// <summary>
      /// Alarm state is unknown
      /// </summary>
      Unknown = 6,
   } 
}
