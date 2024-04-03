// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Vao.Client.Contracts
{
   internal class JsonDownloadNotification
   {
      public string downloadId { get; set; }

      [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
      public string downloadUrl { get; set; }

      [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
      public string name { get; set; }

      [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
      public string recorderAddress { get; set; }
      public string status { get; set; }
   }
}


