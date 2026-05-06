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
using Monitor = Vao.Client.Components.Monitor;

namespace Vao.Client
{
   public class FlexApiClient
   {
      #region Private Members

      private RestClient mRestClient;
      private FeedbackHandler mFeedbackHandler;
      private readonly DataManager mDataManager;
      private readonly ManualResetEvent mStopLoadData = new ManualResetEvent(false);
      private Thread mInitializeThread;
      private readonly object mUpdateCameraListLocker = new object();
      #endregion

      #region Public Events

      public event EventHandler<MessageEventArgs> OnMessage;

      #endregion

      #region Constructors

      public FlexApiClient()
      {
         mDataManager = new DataManager(this);
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Gets the status messages.
      /// </summary>
      /// <param name="lastCheck"></param>
      /// <returns></returns>
      internal string GetStatusMessages(DateTime lastCheck)
      {
         return this.ExecuteGetStatusMessages(lastCheck);
      }

      /// <summary>
      /// Gets a camera object.
      /// NOTE: The <see cref="FlexApiClient"/> caches cameras to limit requests to the API.
      /// </summary>
      /// <param name="cameraNo">The camera number to get</param>
      /// <param name="forceRequest">If true a request will be sent to the API even if the camera is already available.</param>
      /// <returns></returns>
      public Camera GetCamera(int cameraNo, bool forceRequest = false)
      {
         var camera = mDataManager.GetCamera(cameraNo);
         if (camera != null)
         {
            if (forceRequest)
               camera.UpdateCameraData();
            return camera;
         }

         RestResponse response = this.ExecuteGetCameraInternal(cameraNo);
         if (response == null)
            return null;
         Camera loadedCamera = JsonParser.ParseSingleCamera(response.Content, this);
         if (loadedCamera != null)
            mDataManager.AddOrUpdateCamera(loadedCamera);
         return loadedCamera;
      }

      /// <summary>
      /// Gets the list of cameras for this client.
      /// </summary>
      /// <returns></returns>
      public List<Camera> GetCameraList()
      {
         var cached = mDataManager.GetCameraList();
         if (cached != null)
            return cached;
         return LoadCamerasFromApi();
      }

      private List<Camera> LoadCamerasFromApi()
      {
         List<Camera> cameras = this.ExecuteGetCameraList();
         if (cameras != null)
         {
            foreach (Camera camera in cameras)
            {
               mDataManager.AddOrUpdateCamera(camera);
            }
         }

         return cameras;
      }

      /// <summary>
      /// Gets download information for video download
      /// </summary>
      /// <param name="ownerCamera">The camera which the recording is from.</param>
      /// <param name="recorderAddress">The HVR address.</param>
      /// <param name="streamNo">The stream number (1 for Main channel and 2 for Sub channel).</param>
      /// <param name="startTime">Start time of the video recording.</param>
      /// <param name="duration">Duration of the video recording.</param>
      /// <returns></returns>
      public DownloadInfo GetDownloadInfo(Camera ownerCamera, string recorderAddress, int streamNo, string startTime, string duration)
      {
         DownloadInfo downloadInfo = this.ExecuteGetDownloadInfo(ownerCamera, recorderAddress, streamNo, startTime, duration);
         return downloadInfo;
      }

      /// <summary>
      /// Gets a list of monitors.
      /// </summary>
      /// <returns></returns>
      public List<Monitor> GetMonitorList()
      {
         var cached = mDataManager.GetMonitorList();
         if (cached != null)
            return cached;

         return LoadMonitorsFromApi();
      }

      private List<Monitor> LoadMonitorsFromApi()
      {
         List<Monitor> monitors = new List<Monitor>();

         Version minVersion = new Version(1, 2);
         if (GetApiVersion().ToVersion() >= minVersion)
         {
            var receivedMonitors = this.ExecuteGetMonitorList();
            foreach (var monitor in receivedMonitors)
            {
               mDataManager.AddOrUpdateMonitor(monitor, monitor.ActiveCamera);
            }
         }

         // Fallback in case we have an older API version that does not support getting all monitors.
         if (mDataManager.GetMonitorCount() == 0)
         {
            for (int i = 1; i < 255; i++)
            {
               if (!mDataManager.ContainsMonitor(i))
               {
                  // Request the monitor
                  Monitor monitor = this.ExecuteGetMonitor(i);

                  // No more monitors.
                  if (monitor == null)
                  {
                     Debug.WriteLine($"Finished loading monitors at Monitor {i - 1}");
                     break;
                  }

                  // Add to the monitor list
                  mDataManager.AddMonitor(i, monitor);

                  // Stop signaled
                  if (mStopLoadData.WaitOne(0))
                  {
                     RaiseOnMessage(MessageLevel.Info, "Stopping async load of data.", null);
                     return monitors;
                  }
               }
            }
         }
         return monitors;
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
            CurrentUser = GetLoggedInUserInfo();
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
         return this.ExecuteGetStatus();
      }

      /// <summary>
      /// Gets the VaoRApi version number
      /// </summary>
      /// <returns></returns>
      public ApiVersion GetApiVersion()
      {
         return this.ExecuteGetApiVersion();
      }

      /// <summary>
      /// Gets the system implementation version (name and version string).
      /// </summary>
      /// <returns></returns>
      public ImplementationVersion GetImplementationVersion()
      {
         return this.ExecuteGetImplementationVersion();
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

         mDataManager.ClearDataManager();

         mRestClient = null;
      }

      public List<Alarm> GetAlarmList()
      {
         var cached = mDataManager.GetAlarmList();
         if (cached != null)
            return cached;
         List<Alarm> alarms = this.ExecuteGetAlarmList();
         if (alarms != null)
         {
            foreach (Alarm alarm in alarms)
            {
               mDataManager.AddOrUpdateAlarm(alarm);
            }
         }
         return alarms;
      }

      public Alarm GetSingleAlarm(int alarmNo)
      {
         return this.ExecuteGetAlarm(alarmNo);
      }

      public RestResponse SendAlarmCommand(int iAlarmNo, string command)
      {
         return this.ExecuteAlarmCommand(iAlarmNo, command);
      }

      public RestResponse SendAbsolutePosition(int iCameraNo, float? pan, float? tilt, float? zoom)
      {
         return this.ExecuteCameraAbsolutePosition(iCameraNo, pan, tilt, zoom);
      }

      public RestResponse SendLockCamera(int iCameraNo, string timeout)
      {
         return this.ExecuteLockCamera(iCameraNo, timeout);
      }
      public RestResponse SendUnlockCamera(int iCameraNo)
      {
         return this.ExecuteUnlockCamera(iCameraNo);
      }

      public User GetLoggedInUserInfo()
      {
         return this.ExecuteGetLoggedInUserInfo();
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

         string addressLine = $"{(UseHttps ? "https" : "http")}://{Host}:{Port}";
         var requestTimeout = TimeSpan.FromMilliseconds(ConnectionTimeoutMs <= 0 ? 8000 : ConnectionTimeoutMs);

         RestClientOptions options;
         if (UseHttps && IgnoreCertificateErrors)
         {
            // Bypass ssl validation check.
            options = new RestClientOptions(addressLine)
            {
               Authenticator = new HttpBasicAuthenticator(User, Password),
               RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
               Timeout = requestTimeout
            };
         }
         else
         {
            // Bypass ssl validation check.
            options = new RestClientOptions(addressLine)
            {
               Authenticator = new HttpBasicAuthenticator(User, Password),
               Timeout = requestTimeout
            };
         }
         mRestClient = new RestClient(options);
         return mRestClient;
      }

      internal string ValidateResponseContent(RestResponse response)
      {
         if (!response.IsSuccessful)
         {
            string strMessage;
            if (response.ErrorException != null)
            {
               var inner = response.ErrorException.InnerException;
               var detail = inner != null ? $" [{inner.GetType().Name}: {inner.Message}]" : string.Empty;
               strMessage = $"{response.ErrorException.Message}{detail} {response.StatusDescription}";
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






      private void StartLoadAsync()
      {
         RaiseOnMessage(MessageLevel.Info, "Async load of data started", null);

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
                  // Filling the monitor list.
                  GetMonitorList();
                  break;
               case 2:
                  // TODO add additional loading.
                  break;
               default:
                  RaiseOnMessage(MessageLevel.Info, "Async load of data completed", null);
                  return;
            }

            if (mStopLoadData.WaitOne())
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

      public int ConnectionTimeoutMs { get; set; } = 8000;
      public User CurrentUser { get; private set; }

      #endregion
   }
}
