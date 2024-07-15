// ReSharper disable InconsistentNaming
namespace Vao.Client.Contracts
{
   internal class JsonMoveTargetBody
   {
      /// <summary>
      /// -100 to 100
      /// </summary>
      public int? pan { get; set; }

      /// <summary>
      /// -100 to 100
      /// </summary>
      public int? zoom { get; set; }

      /// <summary>
      /// -100 to 100
      /// </summary>
      public int? tilt { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public string focus { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public string iris { get; set; }

      /// <summary>
      /// onOff|smooth|linear|easeInOutCubic
      /// </summary>
      public string acceleration { get; set; }
   }
}
