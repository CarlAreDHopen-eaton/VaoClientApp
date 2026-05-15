// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Vao.Client.Contracts
{
   class JsonImplementationVersion
   {
      // Example JSON:
      //{
      //    "name": "Hernis",
      //    "version": "6.5.0.0"
      //}

      public string name { get; set; }
      public string version { get; set; }
      public string systemName { get; set; }
      public string systemId { get; set; }
   }
}
