using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RestSharp;
using RestSharp.Authenticators;
using Vao.Client.Components;
using Vao.Client.Enum;
using Vao.Client.Utility;

namespace Vao.Client
{
   public class VaoClient
   {
      #region Private Members

      private RestClient mRestClient;
      private FeedbackHandler mFeedbackHandler;
      private Dictionary<int, Camera> mCameraList = new Dictionary<int, Camera>();

      #endregion

      #region Public Events

      public event EventHandler<MessageEventArgs> OnMessage;

      #endregion

      #region Public Methods

      public string GetVaoStatus()
      {
         var client = GetRestClient();
         RestRequest request = new RestRequest("status", Method.Options);
         RestResponse response = client.Execute(request);
         
         return ValidateResponseContent(response);
      }

      public string GetVaoStatusMessages(int iMinutesBeforeNow = 1)
      {
         var client = GetRestClient();
         RestRequest request = new RestRequest("status");
         request.AddHeader("If-Modified-Since", DateTime.Now.AddMinutes(-iMinutesBeforeNow).ToString(CultureInfo.InvariantCulture));
         RestResponse response = client.Execute(request);

         return ValidateResponseContent(response);
      }

      public string GetVaoStatusMessages(DateTime dateTime)
      {
         var client = GetRestClient();
         RestRequest request = new RestRequest("status");
         request.AddHeader("If-Modified-Since", dateTime.ToString("r"));
         RestResponse response = client.Execute(request);

         return ValidateResponseContent(response);
      }

      internal string ValidateResponseContent(RestResponse response)
      {
         if (!response.IsSuccessful)
         {
            string strMessage = response.ErrorException?.ToString() ?? response.ErrorMessage ?? $"Unknown connection error {response.StatusCode}";
            RaiseOnMessage(MessageLevel.Error, strMessage);
            return null;
         }
         return response.Content;
      }

      /// <summary>
      /// Gets a camera object.
      /// NOTE: The <see cref="VaoClient"/> caches cameras to limit requests to the API.
      /// </summary>
      /// <param name="cameraNo">The camera number to get</param>
      /// <param name="forceRequest">If true a request will be sent to the API even if the camera is already available.</param>
      /// <returns></returns>
      public Camera GetVaoCamera(int cameraNo, bool forceRequest = false)
      {
         if (mCameraList.ContainsKey(cameraNo))
            return mCameraList[cameraNo];

         RestResponse response = GetVaoCameraInternal(cameraNo);
         if (response == null)
            return null;
         var camera = JsonParser.ParseSingleCamera(response.Content, this);
         if (camera != null)
            AddOrUpdateCamera(camera);
         return camera;
      }

      private void AddOrUpdateCamera(Camera camera)
      {
         if (!mCameraList.ContainsKey(camera.CameraNumber))
            mCameraList.Add(camera.CameraNumber, camera);
         else
         {
            mCameraList[camera.CameraNumber].Name = camera.Name;
         }

      }

      public List<Camera> GetVaoCameras()
      {
         if (mCameraList != null && mCameraList.Count > 0)
         {
            return mCameraList.Values.ToList();
         }
         var cameras = this.RequestVaoCameraList();
         foreach (var camera in cameras)
         {
            AddOrUpdateCamera(camera);
         }
         return cameras;
      }

      public void StartStatusThread()
      {
         mFeedbackHandler = new FeedbackHandler(this);
         mFeedbackHandler.Start(); 
      }

      public void StartClient()
      {
         
      }

      public void StopClient()
      {
         if (mFeedbackHandler != null)
         {
            mFeedbackHandler.Stop();
            mFeedbackHandler = null;
         }
         
         mCameraList.Clear();

         mRestClient = null;
      }

      #endregion

      #region Internal Methods

      internal RestResponse MoveTargetStart(int cameraNumber, int panSpeed, int tiltSpeed, int zoomSpeed, string focus)
      {
         RestClient client = GetRestClient();
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

      internal RestResponse MoveTargetStop(int cameraNumber)
      {
         RestClient client = GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{cameraNumber}/MoveTarget", Method.Delete);        
         
         RestResponse response = client.Execute(request);
         return response;
      }

      internal RestResponse GetVaoCameraInternal(int iCameraNo)
      {
         RestClient client = GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{iCameraNo}", Method.Get);
         RestResponse response = client.Execute(request);

         string strResponse = ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal RestResponse MoveCameraToPreset(int iCameraNo, int iPresetNumber)
      {
         RestClient client = GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{iCameraNo}/presets/{iPresetNumber}", Method.Post);
         RestResponse response = client.Execute(request);

         string strResponse = ValidateResponseContent(response);
         if (strResponse == null)
         {
            return null;
         }
         return response;
      }

      internal void RaiseOnMessage(MessageLevel messageType, string message)
      {
         OnMessage?.Invoke(this, new MessageEventArgs(messageType, message));
      }

      #endregion

      #region Private Methods

      internal RestClient GetRestClient()
      {
         if (mRestClient != null)
            return mRestClient;

         RestClient client;
         string addressLine = string.Format(@"{0}://{1}:{2}", UseHttps ? "https" : "http", Host, Port);
         if (UseHttps && IgnoreCertificateErrors)
         {
            // Bypass ssl validation check.
            var options = new RestClientOptions(addressLine)
            {
               RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            client = new RestClient(options);
         }
         else
         {
            client = new RestClient(addressLine);
         }

         client.Authenticator = new HttpBasicAuthenticator(User, Password);
         mRestClient = client;
         return client;
      }

      #endregion

      #region Public Properties

      public string Password { get; set; }

      public string User { get; set; }

      public bool IgnoreCertificateErrors { get; set; }

      public string Host { get; set; }

      public string Port { get; set; }

      public bool UseHttps { get; set; }

      #endregion
   }
}
