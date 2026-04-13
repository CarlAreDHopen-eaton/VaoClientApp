// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Vao.Client.Contracts
{
   class JsonUserObject
   {
      // Example JSON:
      // {
      //    "userId": "1",
      //    "name": "Alarm 1",
      //    "priority": 5,
      //    "privilege": "Service",
      //    "playbackAccess": true,
      //    "downloadAccess": true,
      //    "sessionTimeout": "PT10M"
      // }

      public int userId { get; set; }
      public string name { get; set; }
      public int priority { get; set; }
      public string privilege { get; set; }
      public bool playbackAccess { get; set; }
      public bool downloadAccess { get; set; }
      public string sessionTimeout { get; set; }
   }
}
