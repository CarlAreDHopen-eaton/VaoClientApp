namespace Vao.Client.Enum
{
   /// <summary>
   ///  The level of the message.
   /// </summary>
   public enum AlarmCommands
   {
      /// <summary>
      /// If the alarm is user acknowledged, the acknowledge flag will be removed.
      /// If the alarm is not activated (off) the alarm will be activated (user triggered).
      /// Alarms cannot be activated while they are disabled.
      /// </summary>
      Activate = 0,
      /// <summary>
      /// Hardware and software triggered alarms will go to the Acknowledged state. User triggered alarms will go to the inactive state.
      /// </summary>
      Acknowledge = 1,
      /// <summary>
      /// Enables handling of alarm input.
      /// </summary>
      Enable = 2,
      /// <summary>
      /// Disables handling of alarm input.
      /// While the alarm is disabled no alarm actions will be triggered when the alarm input is triggered.
      /// </summary>
      Disable = 3,
      /// <summary>
      /// Immediately triggers all time delayed alarm actions that has not reached their configured activation delay
      /// </summary>
      Expediate = 4
   } 
}
