using System;
using System.Collections.Generic;
using RestSharp;
using Vao.Client.Components;
using Newtonsoft.Json;
using Vao.Client.Contracts;
using Monitor = Vao.Client.Components.Monitor;

namespace Vao.Client.Utility
{
   internal static class RequestHelper
   {
      /// <summary>
      /// Rate limiter to limit API requests to 9 per second.
      /// NOTE: The max limit in the FLEX API is 10.
      /// </summary>
      private static readonly RateLimiter mRateLimiter = new RateLimiter(9);

      internal static List<Camera> ExecuteGetCameraList(this FlexRApiClient flexRApiClient)
      {
         RestClient client = flexRApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest("inputs", Method.Get);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         List<Camera> cameras = JsonParser.ParseCameraList(strResponse, flexRApiClient);
         return cameras;
      }

      internal static List<Preset> ExecuteGetPresetList(this FlexRApiClient flexRApiClient, Camera ownerCamera)
      {
         RestClient client = flexRApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{ownerCamera.ComponentNumber}/presets", Method.Get);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         List<Preset> presets = JsonParser.ParsePresetList(strResponse, flexRApiClient, ownerCamera);
         return presets;
      }

      internal static List<PlaybackInfo> ExecuteGetRecordingList(this FlexRApiClient flexRApiClient, Camera ownerCamera, Guid viewerId)
      {
         RestClient client = flexRApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{ownerCamera.ComponentNumber}/recordings", Method.Post);

         Contracts.JsonViewerIdObject jsonViewerId = new Contracts.JsonViewerIdObject();
         jsonViewerId.viewerId = viewerId.ToString();

         string serializedJsonViewerId = JsonConvert.SerializeObject(jsonViewerId);
         request.AddJsonBody(serializedJsonViewerId);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);
         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         List<PlaybackInfo> playbackInfoList = JsonParser.ParsePlaybackInfoList(strResponse, flexRApiClient);
         return playbackInfoList;
      }

      internal static DownloadInfo ExecuteGetDownloadInfo(this FlexRApiClient flexRApiClient, Camera ownerCamera, string recorderAddress, int streamNo, string startTime, string duration)
      {
         RestClient client = flexRApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{ownerCamera.ComponentNumber}/downloads", Method.Post);

         Contracts.JsonDownloadRequestObject jsonRequestobject = new Contracts.JsonDownloadRequestObject();
         jsonRequestobject.recorderAddress = recorderAddress;
         jsonRequestobject.stream = streamNo;
         jsonRequestobject.start = startTime;
         jsonRequestobject.duration = duration;

         string serializedJsonDownloadRequest = JsonConvert.SerializeObject(jsonRequestobject);
         request.AddJsonBody(serializedJsonDownloadRequest);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);
         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         DownloadInfo downloadInfo = JsonParser.ParseDownloadResponse(strResponse, flexRApiClient);
         return downloadInfo;
      }

      internal static bool ExecuteSetCameraName(this FlexRApiClient flexRApiClient, int videoInput, string name)
      {
         RestClient client = flexRApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{videoInput}", Method.Post);

         JsonChangeName changeName = new JsonChangeName() { name = name };
         string serializedJsonDownloadRequest = JsonConvert.SerializeObject(changeName);
         request.AddJsonBody(serializedJsonDownloadRequest);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);
         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return false;
         }

         return true;
      }

      internal static List<Monitor> ExecuteGetMonitorList(this FlexRApiClient flexRApiClient)
      {
         RestClient client = flexRApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"video-output", Method.Get);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         var monitors = JsonParser.ParseVideoOutputList(strResponse, flexRApiClient);
         return monitors;
      }
      internal static Monitor ExecuteGetMonitor(this FlexRApiClient flexRApiClient, int iVideoOutput)
      {
         RestClient client = flexRApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"video-output/{iVideoOutput}", Method.Get);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         Monitor monitor = JsonParser.ParseVideoOutput(strResponse, flexRApiClient, iVideoOutput);        
         return monitor;
      }

      internal static string ExecuteGetStatus(this FlexRApiClient flexRApiClient)
      {
         RestClient client = flexRApiClient.GetRestClient();
         RestRequest request = new RestRequest("status", Method.Options);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         return flexRApiClient.ValidateResponseContent(response);
      }

      internal static ApiVersion ExecuteGetApiVersion(this FlexRApiClient flexRApiClient)
      {
         RestClient client = flexRApiClient.GetRestClient();
         RestRequest request = new RestRequest("version/api", Method.Options);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);
         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         ApiVersion apiVersion = JsonParser.ParseApiVersion(strResponse, flexRApiClient);
         return apiVersion;
      }

      internal static ImplementationVersion ExecuteGetImplementationVersion(this FlexRApiClient flexRApiClient)
      {
         RestClient client = flexRApiClient.GetRestClient();
         RestRequest request = new RestRequest("version/implementation", Method.Options);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);
         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }

         ImplementationVersion implVersion = JsonParser.ParseImplementationVersion(strResponse, flexRApiClient);
         return implVersion;
      }

      internal static string ExecuteGetStatusMessages(this FlexRApiClient flexRApiClient, DateTime dateTime)
      {
         RestClient client = flexRApiClient.GetRestClient();
         RestRequest request = new RestRequest("status");
         dateTime = dateTime.AddSeconds(1);
         string formattedDateTime = dateTime.ToUniversalTime().ToString("r");
         request.AddHeader("If-Modified-Since", formattedDateTime);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         return flexRApiClient.ValidateResponseContent(response);
      }

      internal static RestResponse ExecuteMoveTargetStart(this FlexRApiClient flexRApiClient, int cameraNumber, int? panSpeed, int? tiltSpeed, int? zoomSpeed, string focus)
      {
         RestClient client = flexRApiClient.GetRestClient();
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

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal static RestResponse ExecuteMoveTargetStop(this FlexRApiClient flexRApiClient, int cameraNumber)
      {
         RestClient client = flexRApiClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{cameraNumber}/MoveTarget", Method.Delete);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);
         return response;
      }

      internal static RestResponse ExecuteGetCameraInternal(this FlexRApiClient flexRApiClient, int iCameraNo)
      {
         RestClient client = flexRApiClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{iCameraNo}", Method.Get);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal static RestResponse ExecuteMoveCameraToPreset(this FlexRApiClient flexRApiClient,  int iCameraNo, int iPresetNumber)
      {
         RestClient client = flexRApiClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{iCameraNo}/presets/{iPresetNumber}", Method.Post);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }


      internal static List<Alarm> ExecuteGetAlarmList(this FlexRApiClient flexRApiClient)
      {
         RestClient client = flexRApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest("alarms", Method.Get);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         List<Alarm> alarms = JsonParser.ParseAlarmList(strResponse, flexRApiClient);
         return alarms;
      }

      internal static Alarm ExecuteGetAlarm(this FlexRApiClient flexRApiClient, int iAlarmNo)
      {
         RestClient client = flexRApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"alarms/{iAlarmNo}", Method.Get);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         Alarm alarm = JsonParser.ParseSingleAlarm(strResponse, flexRApiClient);
         return alarm;
      }

      internal static RestResponse ExecuteAlarmCommand(this FlexRApiClient flexRApiClient, int iAlarmNo, string command)
      {
         RestClient client = flexRApiClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"alarms/{iAlarmNo}", Method.Post);

         JsonAlarmCommandBody jsonAlarmCommandBody = new JsonAlarmCommandBody();

         jsonAlarmCommandBody.command = command;

         string serializedJsonCommand = JsonConvert.SerializeObject(jsonAlarmCommandBody, Formatting.None,
            new JsonSerializerSettings
            {
               NullValueHandling = NullValueHandling.Ignore
            });
         request.AddJsonBody(serializedJsonCommand);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal static RestResponse ExecuteCameraAbsolutePosition(this FlexRApiClient flexRApiClient, int iCameraNo, float? pan, float? tilt, float? zoom)
      {
         RestClient client = flexRApiClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{iCameraNo}/target", Method.Post);

         JsonAbsolutePositionBody absolutePositionBody = new JsonAbsolutePositionBody();

         absolutePositionBody.pan = pan;
         absolutePositionBody.tilt = tilt;
         absolutePositionBody.zoom = zoom;

         string serializedJsonAbsolutePosition = JsonConvert.SerializeObject(absolutePositionBody, Formatting.None,
            new JsonSerializerSettings
            {
               NullValueHandling = NullValueHandling.Ignore
            });
         request.AddJsonBody(serializedJsonAbsolutePosition);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal static RestResponse ExecuteLockCamera(this FlexRApiClient flexRApiClient, int iCameraNo, string timeout = null)
      {
         RestClient client = flexRApiClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{iCameraNo}/lock", Method.Post);

         if (timeout != null)
         {
            JsonCameraLockRequestObject jsonRequestobject = new JsonCameraLockRequestObject();
            jsonRequestobject.timeout = timeout;

            string serializedJsonLockRequest = JsonConvert.SerializeObject(jsonRequestobject);
            request.AddJsonBody(serializedJsonLockRequest);
         }

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal static RestResponse ExecuteUnlockCamera(this FlexRApiClient flexRApiClient, int iCameraNo)
      {
         RestClient client = flexRApiClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{iCameraNo}/lock", Method.Delete);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal static User ExecuteGetLoggedInUserInfo(this FlexRApiClient flexRApiClient)
      {
         RestClient client = flexRApiClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"user", Method.Get);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexRApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         User user = JsonParser.ParseSingleUser(strResponse, flexRApiClient);
         return user;
      }
   }
}
