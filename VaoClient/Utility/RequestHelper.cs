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

      internal static List<Camera> ExecuteGetCameraList(this FlexApiClient flexApiClient)
      {
         RestClient client = flexApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest("inputs", Method.Get);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         List<Camera> cameras = JsonParser.ParseCameraList(strResponse, flexApiClient);
         return cameras;
      }

      internal static List<Preset> ExecuteGetPresetList(this FlexApiClient flexApiClient, Camera ownerCamera)
      {
         RestClient client = flexApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{ownerCamera.ComponentNumber}/presets", Method.Get);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         List<Preset> presets = JsonParser.ParsePresetList(strResponse, flexApiClient, ownerCamera);
         return presets;
      }

      internal static List<PlaybackInfo> ExecuteGetRecordingList(this FlexApiClient flexApiClient, Camera ownerCamera, Guid viewerId)
      {
         RestClient client = flexApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{ownerCamera.ComponentNumber}/recordings", Method.Post);

         Contracts.JsonViewerIdObject jsonViewerId = new Contracts.JsonViewerIdObject();
         jsonViewerId.viewerId = viewerId.ToString();

         string serializedJsonViewerId = JsonConvert.SerializeObject(jsonViewerId);
         request.AddJsonBody(serializedJsonViewerId);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);
         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         List<PlaybackInfo> playbackInfoList = JsonParser.ParsePlaybackInfoList(strResponse, flexApiClient);
         return playbackInfoList;
      }

      internal static DownloadInfo ExecuteGetDownloadInfo(this FlexApiClient flexApiClient, Camera ownerCamera, string recorderAddress, int streamNo, string startTime, string duration)
      {
         RestClient client = flexApiClient.GetRestClient();

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
         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         DownloadInfo downloadInfo = JsonParser.ParseDownloadResponse(strResponse, flexApiClient);
         return downloadInfo;
      }

      internal static bool ExecuteSetCameraName(this FlexApiClient flexApiClient, int videoInput, string name)
      {
         RestClient client = flexApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{videoInput}", Method.Post);

         JsonChangeName changeName = new JsonChangeName() { name = name };
         string serializedJsonDownloadRequest = JsonConvert.SerializeObject(changeName);
         request.AddJsonBody(serializedJsonDownloadRequest);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);
         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return false;
         }

         return true;
      }

      internal static List<Monitor> ExecuteGetMonitorList(this FlexApiClient flexApiClient)
      {
         RestClient client = flexApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"video-output", Method.Get);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         var monitors = JsonParser.ParseVideoOutputList(strResponse, flexApiClient);
         return monitors;
      }
      internal static Monitor ExecuteGetMonitor(this FlexApiClient flexApiClient, int iVideoOutput)
      {
         RestClient client = flexApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"video-output/{iVideoOutput}", Method.Get);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         Monitor monitor = JsonParser.ParseVideoOutput(strResponse, flexApiClient, iVideoOutput);        
         return monitor;
      }

      internal static string ExecuteGetStatus(this FlexApiClient flexApiClient)
      {
         RestClient client = flexApiClient.GetRestClient();
         RestRequest request = new RestRequest("status", Method.Options);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         return flexApiClient.ValidateResponseContent(response);
      }

      internal static ApiVersion ExecuteGetApiVersion(this FlexApiClient flexApiClient)
      {
         RestClient client = flexApiClient.GetRestClient();
         RestRequest request = new RestRequest("version/api", Method.Options);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);
         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         ApiVersion apiVersion = JsonParser.ParseApiVersion(strResponse, flexApiClient);
         return apiVersion;
      }

      internal static ImplementationVersion ExecuteGetImplementationVersion(this FlexApiClient flexApiClient)
      {
         RestClient client = flexApiClient.GetRestClient();
         RestRequest request = new RestRequest("version/implementation", Method.Options);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);
         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }

         ImplementationVersion implVersion = JsonParser.ParseImplementationVersion(strResponse, flexApiClient);
         return implVersion;
      }

      internal static string ExecuteGetStatusMessages(this FlexApiClient flexApiClient, DateTime dateTime)
      {
         RestClient client = flexApiClient.GetRestClient();
         RestRequest request = new RestRequest("status");
         dateTime = dateTime.AddSeconds(1);
         string formattedDateTime = dateTime.ToUniversalTime().ToString("r");
         request.AddHeader("If-Modified-Since", formattedDateTime);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         return flexApiClient.ValidateResponseContent(response);
      }

      internal static RestResponse ExecuteMoveTargetStart(this FlexApiClient flexApiClient, int cameraNumber, int? panSpeed, int? tiltSpeed, int? zoomSpeed, string focus)
      {
         RestClient client = flexApiClient.GetRestClient();
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

         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal static RestResponse ExecuteMoveTargetStop(this FlexApiClient flexApiClient, int cameraNumber)
      {
         RestClient client = flexApiClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{cameraNumber}/MoveTarget", Method.Delete);

         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);
         return response;
      }

      internal static RestResponse ExecuteGetCameraInternal(this FlexApiClient flexApiClient, int iCameraNo)
      {
         RestClient client = flexApiClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{iCameraNo}", Method.Get);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal static RestResponse ExecuteMoveCameraToPreset(this FlexApiClient flexApiClient,  int iCameraNo, int iPresetNumber)
      {
         RestClient client = flexApiClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{iCameraNo}/presets/{iPresetNumber}", Method.Post);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }


      internal static List<Alarm> ExecuteGetAlarmList(this FlexApiClient flexApiClient)
      {
         RestClient client = flexApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest("alarms", Method.Get);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         List<Alarm> alarms = JsonParser.ParseAlarmList(strResponse, flexApiClient);
         return alarms;
      }

      internal static Alarm ExecuteGetAlarm(this FlexApiClient flexApiClient, int iAlarmNo)
      {
         RestClient client = flexApiClient.GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"alarms/{iAlarmNo}", Method.Get);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         Alarm alarm = JsonParser.ParseSingleAlarm(strResponse, flexApiClient);
         return alarm;
      }

      internal static RestResponse ExecuteAlarmCommand(this FlexApiClient flexApiClient, int iAlarmNo, string command)
      {
         RestClient client = flexApiClient.GetRestClient();
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

         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal static RestResponse ExecuteCameraAbsolutePosition(this FlexApiClient flexApiClient, int iCameraNo, float? pan, float? tilt, float? zoom)
      {
         RestClient client = flexApiClient.GetRestClient();
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

         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal static RestResponse ExecuteLockCamera(this FlexApiClient flexApiClient, int iCameraNo, string timeout = null)
      {
         RestClient client = flexApiClient.GetRestClient();
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

         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal static RestResponse ExecuteUnlockCamera(this FlexApiClient flexApiClient, int iCameraNo)
      {
         RestClient client = flexApiClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{iCameraNo}/lock", Method.Delete);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal static User ExecuteGetLoggedInUserInfo(this FlexApiClient flexApiClient)
      {
         RestClient client = flexApiClient.GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"user", Method.Get);
         
         mRateLimiter.WaitForSlot();
         RestResponse response = client.Execute(request);

         string strResponse = flexApiClient.ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         User user = JsonParser.ParseSingleUser(strResponse, flexApiClient);
         return user;
      }
   }
}
