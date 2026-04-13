// ReSharper disable InconsistentNaming
using Vao.Client.Components;

namespace Vao.Client.Contracts
{
   internal class JsonAlarmCommandBody
   {
      // Example JSON:
      // {
      //    "command": activate
      // }

      // Updates the operational state of the specified alarm.Supported commands include:
      // Activate, Acknowledge, Enable, Disable and Expediate
      public string command { get; set; }
   }
}
