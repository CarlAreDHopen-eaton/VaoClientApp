namespace Vao.Client.Contracts
{
   internal class JsonVideoOutput
   {
      public int inputId { get; set; }
      public string name { get; set; }
      /// <summary>
      /// The features of the camera
      /// </summary>
      public string[] features { get; set; }
   }

   internal class JsonVideoOutputEx
   {
      public int outputId { get; set; }
      public string outputName { get; set; }
      public int inputId { get; set; }
      public string inputName { get; set; }
   }  
}
