using System;
using System.Collections.Generic;
using System.Diagnostics;
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
      private readonly Dictionary<int, Camera> mCameraList = new Dictionary<int, Camera>();
      private readonly Dictionary<int, Components.Monitor> mMonitorList = new Dictionary<int, Components.Monitor>();
      private readonly ManualResetEvent mStopLoadData = new ManualResetEvent(false);
      private Thread mInitializeThread;
      private readonly object mUpdateCameraListLocker = new object();
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
      public Camera GetCamera(int cameraNo, bool forceRequest = false)
      {
         lock (mUpdateCameraListLocker)
         {
            if (mCameraList.TryGetValue(cameraNo, out var camera1))
               return camera1;
         }

         RestResponse response = this.GetVaoCameraInternal(cameraNo);
         if (response == null)
            return null;
         var camera = JsonParser.ParseSingleCamera(response.Content, this);
         if (camera != null)
            AddOrUpdateCamera(camera);
         return camera;
      }
     
      /// <summary>
      /// Gets the list of cameras for this client.
      /// </summary>
      /// <returns></returns>
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
      /// <summary>
      /// Gets download information for video download
      /// </summary>
      /// <param name="ownerCamera">The camera which the recording is from.</param>
      /// <param name="recorderAddress">the HVR address.</param>
      /// <param name="streamNo">The stream number (1 for Main channel and 2 for Sub channel).</param>
      /// <param name="startTime">Start time of the video recording.</param>
      /// <param name="duration">duartion of the video recording.</param>
      /// <returns></returns>
      public DownloadInfo GetDownloadInfo(Camera ownerCamera, string recorderAddress, int streamNo, string startTime, string duration)
      {
         DownloadInfo downloadInfo = this.RequestVaoDownloadInfo(ownerCamera, recorderAddress, streamNo, startTime, duration);
         return downloadInfo;
      }

      /// <summary>
      /// Gets a list of monitors.
      /// </summary>
      /// <returns></returns>
      public List<Components.Monitor> GetMonitorList()
      {
         if (mMonitorList != null && mMonitorList.Count > 0)
         {
            return mMonitorList.Values.ToList();
         }
         
         // NOTE there is no single request method to get all monitors.

         return null;
      }

      /// <summary>
      /// Starts the client.
      /// </summary>
      /// <returns>Returns true if start was successful</returns>
      public bool StartClient()
      {
         if (mInitializeThread != null)
            StopClient();

         // Start async load.
         mInitializeThread = new Thread(StartLoadAsync);
         mInitializeThread.Start();
         mInitializeThread = null;

         StartStatusThread();

         string statusTime = GetLastStatusTime();
         if (statusTime != null)
         {
            return true;
         }
         return false;

         }

      /// <summary>
      /// Gets the latest status message time of the system
      /// </summary>
      /// <returns></returns>
      public string GetLastStatusTime()
      {
         return this.GetVaoStatus();
      }

      /// <summary>
      /// Gets the VaoRApi version number
      /// </summary>
      /// <returns></returns>
      public ApiVersion GetApiVersion()
      {
         return this.GetVaoApiVersion();
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

      internal void RaiseOnMessage(MessageLevel messageType, string message, StatusMessage statusMessage)
      {
         OnMessage?.Invoke(this, new MessageEventArgs(messageType, message, statusMessage));
      }

      internal RestClient GetRestClient()
      {
         if (mRestClient != null)
            return mRestClient;

         RestClient client;
         string addressLine = $"{(UseHttps ? "https" : "http")}://{Host}:{Port}";
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
            string strMessage;
            if (response.ErrorException != null)
            {
               strMessage = $"{response.ErrorException.Message} {response.StatusDescription}";
            }
            else
            {
               strMessage = response.ErrorMessage ?? $"Unknown connection error {response.StatusCode}";
            }
            RaiseOnMessage(MessageLevel.Error, strMessage, null);
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

      private void StartLoadAsync()
      {
         RaiseOnMessage(MessageLevel.Info, "Async load of data started", null);

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
                        Components.Monitor monitor = this.RequestVaoMonitor(i);

                        // No more monitors.
                        if (monitor == null)
                        {
                           Debug.WriteLine($"Finished loading monitors at Monitor {i - 1}");
                           break;
                        }

                        // Add to the monitor list (Note Need to fix as this is not thread safe)
                        mMonitorList.Add(i, monitor);

                        // Stop signaled
                        if (mStopLoadData.WaitOne(iLoadDelay))
                        {
                           RaiseOnMessage(MessageLevel.Info, "Async load of data started", null);
                           return;
                        }
                     }
                  }
                  break;
               case 2:
                  // TODO add additional loading.
                  break;
               default:
                  RaiseOnMessage(MessageLevel.Info, "Async load of data completed", null);
                  return;

            }

            if (mStopLoadData.WaitOne(iLoadDelay))
            {
               RaiseOnMessage(MessageLevel.Info, "Async load of data aborted", null);
               return;
            }

            iState++;
         }
      }

      private void StartStatusThread()
      {
         mFeedbackHandler = new FeedbackHandler(this);
         mFeedbackHandler.Start();
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
