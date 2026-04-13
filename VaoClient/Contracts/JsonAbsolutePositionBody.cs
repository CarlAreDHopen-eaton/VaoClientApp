// ReSharper disable InconsistentNaming
namespace Vao.Client.Contracts
{
   internal class JsonAbsolutePositionBody
   {
      // Example JSON:
      // {
      //    "pan": 100,
      //    "tilt": 150,
      //    "zoom": 55
      // }

      /// <summary>
      /// 0.0 - 360.0
      /// </summary>
      public float? pan { get; set; }

      /// <summary>
      /// 0.0 - 180.0
      /// </summary>
      public float? tilt { get; set; }

      /// <summary>
      /// 0.0 - 100.0
      /// </summary>
      public float? zoom { get; set; }
   }
}
