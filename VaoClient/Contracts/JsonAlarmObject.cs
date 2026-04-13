// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Vao.Client.Contracts
{
   class JsonAlarmObject
   {
      // Example JSON:
      // {
      //    "alarmId": 1,
      //    "name":"Alarm 1",
      //    "status":"Active",
      //    "extendedStatus":"HardwareTriggered",
      //    "priority": 1,
      //    "actions": 2,
      // }

      public int alarmId { get; set; }
      public string name { get; set; }
      public string status { get; set; }
      public string extendedStatus { get; set; }
      public int priority { get; set; }
      public int actions { get; set; }
   }
}
