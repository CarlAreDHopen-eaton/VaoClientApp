// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Vao.Client.Contracts
{
   class JsonPlaybackInfoObject
   {
      // Example JSON:
      //  {
      //  "playbackUrl":"rtsp://Service:9.EFJBO9@192.168.100.137:554/playback/camera1/stream1/12345678-1234-1234-1234-123456789012",
      //  "recorderAddress":"192.168.100.137",
      //  "inputId":"1",
      //  "stream":"1",
      //  "recordingLimit":"P7D",
      //  "recordingType":"Continuous"
      //  }

      public string playbackUrl { get; set; }
      public string recorderAddress { get; set; }
      public int inputId { get; set; }
      public int stream { get; set; }
      public string recordingLimit { get; set; }
      public string recordingType { get; set; }
   }
}
