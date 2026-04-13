namespace Vao.Client.Enum
{
   /// <summary>
   ///  The level of the message.
   /// </summary>
   public enum UserPrivilege
   {
      /// <summary>
      /// No privileges. (Also called Guest privilege.)
      /// </summary>
      None = 0,
      /// <summary>
      /// Normal user privileges.
      /// </summary>
      Operator = 1,
      /// <summary>
      /// Supervisor privileges.
      /// </summary>
      Supervisor = 2,
      /// <summary>
      /// Extended privileges intended for service personnel.
      /// </summary>
      Service = 3
   } 
}
