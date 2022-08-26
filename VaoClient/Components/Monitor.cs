using Vao.Client.Components.Interfaces;

namespace Vao.Client.Components
{
   internal class Monitor : BaseComponent, INamedComponent
   {
      #region Private Members

      private string mMonitorName;

      #endregion

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="monitorNo"></param>
      /// <param name="client"></param>
      public Monitor(int monitorNo, VaoClient client, Camera currentCamera) 
         : base(client, monitorNo)
      {
         mMonitorName = $"Monitor {ComponentNumber}";
      }

      /// <summary>
      /// Gets the name of the monitor
      /// NOTE The VAO API does not support monitor names, therefor they will all have default names.
      /// </summary>
      public string Name
      {
         get
         {
            return mMonitorName;
         }
      }
     
      /// <summary>
      /// This method does not have any functionallity.
      /// </summary>
      /// <param name="newName"></param>
      /// <returns></returns>
      public bool SetName(string newName)
      {
         // Not allowed by API.
         return false;
      }

      public bool SelectCamera(Camera camera)
      {
         // TODO Send message.
         return false;
      }

      public Camera ActiveCamera { get; }
   }
}
