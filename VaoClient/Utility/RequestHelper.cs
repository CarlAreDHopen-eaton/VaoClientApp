using System;
using System.Collections.Generic;
using System.Globalization;
using RestSharp;
using Vao.Client.Components;

namespace Vao.Client.Utility
{
   internal static class RequestHelper
   {
      internal static List<Camera> RequestVaoCameraList(this VaoClient vaoClient)
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

      internal static string GetVaoStatus(this VaoClient vaoClient)
      {
         RestClient client = vaoClient.GetRestClient();
         RestRequest request = new RestRequest("status", Method.Options);
         RestResponse response = client.Execute(request);

         return vaoClient.ValidateResponseContent(response);
      }

      internal static string GetVaoStatusMessages(this VaoClient vaoClient, int iMinutesBeforeNow = 1)
      {
         RestClient client = vaoClient.GetRestClient();
         RestRequest request = new RestRequest("status");
         request.AddHeader("If-Modified-Since", DateTime.Now.AddMinutes(-iMinutesBeforeNow).ToString(CultureInfo.InvariantCulture));
         RestResponse response = client.Execute(request);

         return vaoClient.ValidateResponseContent(response);
      }

      internal static string GetVaoStatusMessages(this VaoClient vaoClient, DateTime dateTime)
      {
         RestClient client = vaoClient.GetRestClient();
         RestRequest request = new RestRequest("status");
         request.AddHeader("If-Modified-Since", dateTime.ToString("r"));
         RestResponse response = client.Execute(request);

         return vaoClient.ValidateResponseContent(response);
      }

      internal static RestResponse MoveTargetStart(this VaoClient vaoClient, int cameraNumber, int panSpeed, int tiltSpeed, int zoomSpeed, string focus)
      {
         RestClient client = vaoClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{cameraNumber}/MoveTarget", Method.Post);

         Contracts.JsonMoveTargetBody jsonMoveTarget = new Contracts.JsonMoveTargetBody();

         // Set pan speed.
         jsonMoveTarget.pan = panSpeed;
         // Set tilt speed.
         jsonMoveTarget.tilt = tiltSpeed;
         // Set zoom speed.
         jsonMoveTarget.zoom = zoomSpeed;
         // Set focus
         jsonMoveTarget.focus = focus;

         string serializedJsonMoveTarget = Json.Net.JsonNet.Serialize(jsonMoveTarget);
         request.AddJsonBody(serializedJsonMoveTarget);

         RestResponse response = client.Execute(request);
         return response;
      }

      internal static RestResponse MoveTargetStop(this VaoClient vaoClient, int cameraNumber)
      {
         RestClient client = vaoClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{cameraNumber}/MoveTarget", Method.Delete);

         RestResponse response = client.Execute(request);
         return response;
      }

      internal static RestResponse GetVaoCameraInternal(this VaoClient vaoClient, int iCameraNo)
      {
         RestClient client = vaoClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{iCameraNo}", Method.Get);
         RestResponse response = client.Execute(request);

         string strResponse = vaoClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal static RestResponse MoveCameraToPreset(this VaoClient vaoClient,  int iCameraNo, int iPresetNumber)
      {
         RestClient client = vaoClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{iCameraNo}/presets/{iPresetNumber}", Method.Post);
         RestResponse response = client.Execute(request);

         string strResponse = vaoClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

   }
}
