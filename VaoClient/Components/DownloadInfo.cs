using System;
using Vao.Client.Contracts;

namespace Vao.Client.Components
{
   public class DownloadInfo : BaseComponent
   {
      #region Private Members

      private Guid mDownloadId;
      private int mInputId;
      private int mStream;
      private string mStart;
      private string mDuration;
      private string mEstimatedTime;

      #endregion

      #region Constructor

      internal DownloadInfo(JsonDownloadResponseObject jsonObject, VaoClient vaoClient)
         : base(vaoClient, 0)
      {
         mDownloadId = jsonObject.downloadId;  
         mInputId = jsonObject.inputId;
         mStream = jsonObject.stream;
         mStart = jsonObject.start;
         mDuration = jsonObject.duration;
         mEstimatedTime = jsonObject.estimatedTime;
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// Download identification guid.
      /// </summary>
      public Guid DownloadId
      { 
         get { return mDownloadId; } 
      }

      /// <summary>
      /// Identification number of the camera.
      /// </summary>
      public int InputId
      {
         get { return mInputId; }
      }

      /// <summary>
      /// Stream number of the recorded camera stream.
      /// </summary>
      public int Stream
      {
         get { return mStream; }
      }

      /// <summary>
      /// Start time for the video extraction
      /// </summary>
      public string Start
      {
         get { return mStart; }
      }

      /// <summary>
      /// Duration of the video extraction
      /// </summary>
      public string Duration
      {
         get { return mDuration; }
      }

      /// <summary>
      /// Estimated time for the video extraction to be ready for download
      /// </summary>
      public string EstimatedTime
      {
         get { return mEstimatedTime; }
      }

      #endregion
   }
}
