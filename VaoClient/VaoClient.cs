using System;
using System.Collections.Generic;
using System.Globalization;
using RestSharp;
using RestSharp.Authenticators;
using Vao.Client.Components;
using Vao.Client.Utility;

namespace Vao.Client
{
   public class VaoClient
   {
      public event EventHandler<ConnectEventArgs> OnError;
      private RestClient mRestClient;

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

      private string ValidateResponseContent(RestResponse response)
      {
         if (!response.IsSuccessful)
         {
            string strMessage = response.ErrorException?.ToString() ?? response.ErrorMessage ?? $"Unknown connection error {response.StatusCode}";
            OnError?.Invoke(this, new ConnectEventArgs(strMessage));
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

      public void StopClient()
      {
         mRestClient = null;
      }

      public string Password { get; set; }

      public string User { get; set; }

      public bool IgnoreCertificateErrors { get; set; }

      public string Host { get; set; }

      public string Port { get; set; }

      public bool UseHttps { get; set; }
   }
}
