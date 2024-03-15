// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Vao.Client.Contracts
{
   class JsonApiVersion
   {
      // Example JSON:
      //{
      //    "name": "VaoRApi",
      //    "majorversion": "1",
      //    "minorversion": "0"
      //}

      public string name { get; set; }
      public int majorVersion { get; set; }
      public int minorVersion { get; set; }

   }
}