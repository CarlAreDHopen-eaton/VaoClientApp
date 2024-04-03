// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
using Newtonsoft.Json;

namespace Vao.Client.Contracts
{
   internal class JsonStatusMessage
   {
      public string description { get; set; }
      public string type { get; set; }
      public string timestamp { get; set; }

      [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
      public JsonDownloadNotification downloadNotification { get; set; }

   }
}


