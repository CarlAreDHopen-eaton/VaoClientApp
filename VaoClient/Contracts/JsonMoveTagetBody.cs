namespace Vao.Client.Contracts
{
   internal class JsonMoveTagetBody
   {
      /// <summary>
      /// -100 to 100
      /// </summary>
      public int pan { get; set; } = 0;

      /// <summary>
      /// -100 to 100
      /// </summary>
      public int zoom { get; set; } = 0;

      /// <summary>
      /// -100 to 100
      /// </summary>
      public int tilt { get; set; } = 0;

      /// <summary>
      /// 
      /// </summary>
      public string focus { get; set; } = "auto";

      /// <summary>
      /// 
      /// </summary>
      public string iris { get; set; } = "auto";

      /// <summary>
      /// onOff|smooth|linear|easeInOutCubic
      /// </summary>
      public string accelleration { get; set; } = "linear";
   }
}
