using System;
using System.Collections.Generic;
using System.Globalization;
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
         request.AddHeader("If-Modified-Since", dateTime.ToString(CultureInfo.InvariantCulture));
         RestResponse response = client.Execute(request);

         return ValidateResponseContent(response);
      }

      private string ValidateResponseContent(RestResponse response)
      {
         if (!response.IsSuccessful)
         {
            string strMessage = response.ErrorException?.ToString() ?? response.ErrorMessage ?? $"Unknown connection error {response.StatusCode}";
            RaiseOnMessage(MessageType.Error, strMessage);
            return null;
         }
         return response.Content;
      }

      public Camera GetVaoCamera(int iCameraNo)
      {
         RestResponse response = GetVaoCameraInternal(iCameraNo);
         if (response == null)
            return null;
         return JsonParser.ParseSingleCamera(response.Content, this);
      }

      public List<Camera> GetVaoCameras()
      {
         RestClient client = GetRestClient();

         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest("inputs", Method.Get);
         RestResponse response = client.Execute(request);

         string strResponse = ValidateResponseContent(response);
         if (strResponse == null)
         {
            // Empty list.
            return null;
         }

         return JsonParser.ParseCameraList(strResponse, this);
      }

      public void StartStatusThread()
      {
         mFeedbackHandler = new FeedbackHandler(this);
         mFeedbackHandler.Start(); 
      }

      public void StopClient()
      {
         if (mFeedbackHandler != null)
         {
            mFeedbackHandler.Stop();
            mFeedbackHandler = null;
         }
         mRestClient = null;
      }

      #endregion

      #region Internal Methods

      internal RestResponse PanTiltStart(int cameraNumber, int panSpeed, int tiltSpeed)
      {
         RestClient client = GetRestClient();
         // ReSharper disable once RedundantArgumentDefaultValue
         RestRequest request = new RestRequest($"inputs/{cameraNumber}/MoveTarget", Method.Post);

         Contracts.JsonMoveTargetBody jsonMoveTarget = new Contracts.JsonMoveTargetBody();
         // Set pan speed.
         if (panSpeed != 0)
            jsonMoveTarget.pan = (panSpeed > 0) ? 100 : -100;
         // Set tilt speed.
         if (tiltSpeed != 0)
            jsonMoveTarget.tilt = (tiltSpeed > 0) ? 100 : -100;

         string serializedJsonMoveTarget = Json.Net.JsonNet.Serialize(jsonMoveTarget);
         request.AddJsonBody(serializedJsonMoveTarget);

         RestResponse response = client.Execute(request);
         return response;
      }

      internal RestResponse PanTiltStop(int cameraNumber)
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

      internal void RaiseOnMessage(MessageType messageType, string message)
      {
         OnMessage?.Invoke(this, new MessageEventArgs(messageType, message));
      }

      #endregion

      #region Private Methods

      private RestClient GetRestClient()
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
