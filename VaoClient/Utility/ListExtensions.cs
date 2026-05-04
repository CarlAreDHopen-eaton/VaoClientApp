using System.Collections.Generic;
using Vao.Client.Components;

namespace Vao.Client.Utility
{
   internal static class ListExtensions
   {
      internal static Camera GetByLocalNo(this List<Camera> cameras, int cameraNo, FlexRApiClient client)
      {
         foreach (Camera camera in client.GetCameraList())
         {
            if (camera.ComponentNumber == cameraNo)
            {
               return camera;
            }
         }

         return null;
      }
   }
}
