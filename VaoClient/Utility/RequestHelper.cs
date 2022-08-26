using System.Collections.Generic;
using RestSharp;
using Vao.Client.Components;

namespace Vao.Client.Utility
{
   internal static class RequestHelper
   {
      public static List<Camera> RequestVaoCameraList(this VaoClient vaoClient)
      {
         RestClient client = vaoClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest("inputs", Method.Get);
         RestResponse response = client.Execute(request);

         string strResponse = vaoClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         List<Camera> cameras = JsonParser.ParseCameraList(strResponse, vaoClient);
         return cameras;
      }

      internal static List<Preset> RequestVaoPresetList(this VaoClient vaoClient, Camera ownerCamera)
      {
         RestClient client = vaoClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{ownerCamera.ComponentNumber}/presets", Method.Get);
         RestResponse response = client.Execute(request);

         string strResponse = vaoClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         List<Preset> presets = JsonParser.ParsePresetList(strResponse, vaoClient, ownerCamera);
         return presets;
      }


      internal static Monitor RequestVaoMonitor(this VaoClient vaoClient, int iVideoOutput)
      {
         RestClient client = vaoClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"video-output/{iVideoOutput}", Method.Get);
         RestResponse response = client.Execute(request);

         string strResponse = vaoClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         Monitor monitor = JsonParser.ParseVideoOutput(strResponse, vaoClient, iVideoOutput);        
         return monitor;
      }


   }
}
