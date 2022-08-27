using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
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
      private Dictionary<int, Components.Monitor> mMonitorList = new Dictionary<int, Components.Monitor>();
      private ManualResetEvent mStopLoadData = new ManualResetEvent(false);
      private Thread mInitializeThred;
      private object mUpdateCameraListLocker = new object();
      #endregion

      #region Public Events

      public event EventHandler<MessageEventArgs> OnMessage;

      #endregion

      #region Public Methods
     
      /// <summary>
      /// Gets the status messages.
      /// </summary>
      /// <param name="lastCheck"></param>
      /// <returns></returns>
      internal string GetStatusMessages(DateTime lastCheck)
      {
         return this.GetVaoStatusMessages(lastCheck);
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
         lock (mUpdateCameraListLocker)
         {
            if (mCameraList.ContainsKey(cameraNo))
               return mCameraList[cameraNo];
         }

         RestResponse response = this.GetVaoCameraInternal(cameraNo);
         if (response == null)
            return null;
         var camera = JsonParser.ParseSingleCamera(response.Content, this);
         if (camera != null)
            AddOrUpdateCamera(camera);
         return camera;
      }
     
      public List<Camera> GetCameraList()
      {
         if (mCameraList != null && mCameraList.Count > 0)
         {
            return mCameraList.Values.ToList();
         }
         var cameras = this.RequestVaoCameraList();
         if (cameras != null)
         {
            foreach (var camera in cameras)
            {
               AddOrUpdateCamera(camera);
            }
         }
         return cameras;
      }

      public void StartStatusThread()
      {
         mFeedbackHandler = new FeedbackHandler(this);
         mFeedbackHandler.Start(); 
      }

      /// <summary>
      /// Starts the client.
      /// </summary>
      /// <returns>Returns true if start was successfull</returns>
      public bool StartClient()
      {
         if (mInitializeThred != null)
            StopClient();

         // Start async load.
         mInitializeThred = new Thread(StartLoadAsync);
         mInitializeThred.Start();
         mInitializeThred = null;

         StartStatusThread();

         string statusTime = GetStatusTime();
         if (statusTime != null)
         {
            return true;
         }
         return false;

         }

         private void StartLoadAsync()
      {
         int iLoadDelay = 200;
         int iState = 0;

         while (true)
         {
            switch (iState)
            {
               case 0:
                  // Filling the camera list.
                  GetCameraList();
                  break;
               case 1:
                  for (int i = 1; i < 255; i++)
                  {
                     if (!mMonitorList.ContainsKey(i))
                     {
                        // Request the monitor
                        Components.Monitor monitor = RequestHelper.RequestVaoMonitor(this, i);

                        // No more monitors.
                        if (monitor == null)
                        {
                           Debug.WriteLine($"Finished loading monitors at Monitor {i-1}");
                           break;
                        }

                        // Add to the monitor list (Note Need to fix as this is not thread safe)
                        mMonitorList.Add(i, monitor);

                        // Stop signaled
                        if (mStopLoadData.WaitOne(iLoadDelay))
                           return;
                     }
                  }
                  break;
               case 2:

                  break;
               default:
                  return;

            }

            if (mStopLoadData.WaitOne(iLoadDelay))
               return;

            iState++;
         }
      }

      /// <summary>
      /// Gets the latest status messeg time of the system
      /// </summary>
      /// <returns></returns>
      public string GetStatusTime()
      {
         return this.GetVaoStatus();
      }

      /// <summary>
      /// Stops  the client.
      /// </summary>
      public void StopClient()
      {
         mStopLoadData.Set();

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

      internal void RaiseOnMessage(MessageLevel messageType, string message)
      {
         OnMessage?.Invoke(this, new MessageEventArgs(messageType, message));
      }

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

      #endregion

      #region Private Methods

      private void AddOrUpdateCamera(Camera camera)
      {
         var key = camera.ComponentNumber;
         lock (mUpdateCameraListLocker)
         {
            if (!mCameraList.ContainsKey(key))
            {
               mCameraList.Add(key, camera);
            }
            else
            {
               mCameraList[key].Name = camera.Name;
            }
         }

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
