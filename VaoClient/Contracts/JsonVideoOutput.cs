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
}
