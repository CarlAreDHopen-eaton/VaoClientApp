using Vao.Client.Components.Interfaces;

namespace Vao.Client.Components
{
   public class Monitor : BaseComponent, INamedComponent
   {
      #region Private Members

      private string mMonitorName;
      private Camera mActiveCamera;

      #endregion

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="monitorNo">The monitor number</param>
      /// <param name="client">The client that this camera belong to</param>
      /// <param name="currentCamera">The camera currently active on this monitor</param>
      public Monitor(int monitorNo, VaoClient client, Camera currentCamera) 
         : base(client, monitorNo)
      {
         mMonitorName = $"Monitor {ComponentNumber}";
         mActiveCamera = currentCamera;
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
      /// Gets the currently active camera on this output/monitor.
      /// </summary>
      public Camera ActiveCamera 
      { 
         get
         {
            return mActiveCamera;
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

      /// <summary>
      /// Selects a camera to the specified output/monitor
      /// </summary>
      /// <param name="camera"></param>
      /// <returns></returns>
      public bool SelectCamera(Camera camera)
      {
         // TODO Send message.
         return false;
      }
   }
}
