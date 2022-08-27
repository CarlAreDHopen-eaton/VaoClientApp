// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Vao.Client.Contracts
{
   class JsonCameraObject
   {
      // Example JSON:
      // {"inputId":"1",
      // "name":"C1 HVS Ch1",
      // "features":["wash","wipe","zoom","iris","focus","pan","tilt"],
      // "stream1url":"rtsp:\/\/Service:70lhFvTe@192.168.101.150:5554\/encoder\/camera1\/stream1",
      // "stream2url":"rtsp:\/\/Service:70lhFvTe@192.168.101.150:5554\/encoder\/camera1\/stream2",
      // "stream1resolution":"704x576",
      // "stream2resolution":"352x240"}

      public int inputId { get; set; }
      public string name { get; set; }
      public string stream1url { get; set; }
      public string stream2url { get; set; }
      public string stream1resolution { get; set; }
      public string stream2resolution { get; set; }
      public string[] features { get; set; }
   }
}
