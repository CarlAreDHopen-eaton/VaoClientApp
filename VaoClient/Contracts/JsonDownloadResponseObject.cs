// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
using System;

namespace Vao.Client.Contracts
{
   class JsonDownloadResponseObject
   {
      // Example JSON:
      //  {
      //    "downloadId":"676f681f-41d2-43e4-92fa-cc9dba07ea07",
      //    "inputId":"1",
      //    "stream":"1",
      //    "start":"2023-01-02 13:30:00",
      //    "duration":"PT10M",
      //    "estimatedTime":"PT1M20S"
      //  }

      public Guid downloadId { get; set; }
      public int inputId { get; set; }
      public int stream { get; set; }
      public string start { get; set; }
      public string duration { get; set; }
      public string estimatedTime { get; set; } 
   }
}
