// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Vao.Client.Contracts
{
   class JsonDownloadRequestObject
   {
      // Example JSON:
      //  {
      //    "recorderAddress":"192.168.100.137",
      //    “stream”:”1”,
      //    "start":"2023-01-02 13:30:00",
      //    "duration”:"PT10M"
      //  }

      public string recorderAddress { get; set; }
      public int stream { get; set; }
      public string start { get; set; }
      public string duration { get; set; }
   }
}
