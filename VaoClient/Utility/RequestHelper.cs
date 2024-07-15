using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using RestSharp;
using Vao.Client.Components;
using Newtonsoft.Json;
using Vao.Client.Contracts;

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

      internal static List<PlaybackInfo> RequestVaoRecordingList(this VaoClient vaoClient, Camera ownerCamera, Guid viewerId)
      {
         RestClient client = vaoClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{ownerCamera.ComponentNumber}/recordings", Method.Post);

         Contracts.JsonViewerIdObject jsonViewerId = new Contracts.JsonViewerIdObject();
         jsonViewerId.viewerId = viewerId.ToString();

         string serializedJsonViewerId = JsonConvert.SerializeObject(jsonViewerId);
         request.AddJsonBody(serializedJsonViewerId);

         RestResponse response = client.Execute(request);
         string strResponse = vaoClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         List<PlaybackInfo> playbackInfoList = JsonParser.ParsePlaybackInfoList(strResponse, vaoClient);
         return playbackInfoList;
      }

      internal static DownloadInfo RequestVaoDownloadInfo(this VaoClient vaoClient, Camera ownerCamera, string recorderAddress, int streamNo, string startTime, string duration)
      {
         RestClient client = vaoClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{ownerCamera.ComponentNumber}/downloads", Method.Post);

         Contracts.JsonDownloadRequestObject jsonRequestobject = new Contracts.JsonDownloadRequestObject();
         jsonRequestobject.recorderAddress = recorderAddress;
         jsonRequestobject.stream = streamNo;
         jsonRequestobject.start = startTime;
         jsonRequestobject.duration = duration;

         string serializedJsonDownloadRequest = JsonConvert.SerializeObject(jsonRequestobject);
         request.AddJsonBody(serializedJsonDownloadRequest);

         RestResponse response = client.Execute(request);
         string strResponse = vaoClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         DownloadInfo downloadInfo = JsonParser.ParseDownloadResponse(strResponse, vaoClient);
         return downloadInfo;
      }

      internal static bool RequestVaoSetCameraName(this VaoClient vaoClient, int videoInput, string name)
      {
         RestClient client = vaoClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{videoInput}", Method.Post);

         JsonChangeName changeName = new JsonChangeName() { name = name };
         string serializedJsonDownloadRequest = JsonConvert.SerializeObject(changeName);
         request.AddJsonBody(serializedJsonDownloadRequest);

         RestResponse response = client.Execute(request);
         string strResponse = vaoClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return false;
         }

         return true;
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

      internal static ApiVersion GetVaoApiVersion(this VaoClient vaoClient)
      {
         RestClient client = vaoClient.GetRestClient();
         RestRequest request = new RestRequest("version/api", Method.Options);
         RestResponse response = client.Execute(request);
         string strResponse = vaoClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         ApiVersion apiVersion = JsonParser.ParseApiVersion(strResponse, vaoClient);
         return apiVersion;
      }

      internal static string GetVaoStatusMessages(this VaoClient vaoClient, DateTime dateTime)
      {
         RestClient client = vaoClient.GetRestClient();
         RestRequest request = new RestRequest("status");
         dateTime = dateTime.AddSeconds(1);
         string formattedDateTime = dateTime.ToUniversalTime().ToString("r");
         request.AddHeader("If-Modified-Since", formattedDateTime);
         RestResponse response = client.Execute(request);

         return vaoClient.ValidateResponseContent(response);
      }

      internal static RestResponse MoveTargetStart(this VaoClient vaoClient, int cameraNumber, int? panSpeed, int? tiltSpeed, int? zoomSpeed, string focus)
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

         string serializedJsonMoveTarget = JsonConvert.SerializeObject(jsonMoveTarget, Formatting.None,
            new JsonSerializerSettings
            {
               NullValueHandling = NullValueHandling.Ignore
            });
         request.AddJsonBody(serializedJsonMoveTarget);

         RestResponse response = client.Execute(request);

         string strResponse = vaoClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
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
