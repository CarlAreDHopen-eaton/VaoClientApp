using Vao.Client.Contracts;

namespace Vao.Client.Components
{
   public class PlaybackInfo : BaseComponent
   {
      #region Private Members

      private static int mCurrentComponentNumber = 0;

      private string mPlaybackUrl;
      private string mRecorderAddress;
      private int mInputId;
      private int mStream;
      private string mRecordingLimit;
      private string mRecordingType;

      #endregion

      #region Constructor

      internal PlaybackInfo(JsonPlaybackInfoObject jsonObject, VaoClient vaoClient)
         : base(vaoClient, mCurrentComponentNumber)
      {
         mPlaybackUrl = jsonObject.playbackUrl;
         mRecorderAddress = jsonObject.recorderAddress;
         mInputId = jsonObject.inputId;   
         mStream = jsonObject.stream;
         mRecordingLimit = jsonObject.recordingLimit;
         mRecordingType = jsonObject.recordingType;
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// Rtsp url for playback of recording
      /// </summary>
      public string PlaybackUrl
      {
         get { return mPlaybackUrl; }
      }

      /// <summary>
      /// The address of the HVR with the recording
      /// </summary>
      public string RecorderAddress
      {
         get { return mRecorderAddress; }
      }

      /// <summary>
      /// The camera number
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
      /// Timespan for recording limit (number of days)
      /// </summary>
      public string RecordingLimit
      {
         get { return mRecordingLimit; }
      }

      /// <summary>
      /// Type of recording (Continous, HomePosition or Manual(User)
      /// </summary>
      public string RecordingType
      {
         get { return mRecordingType; }
      }

      #endregion

      #region Public Methods
           
      /// <inheritdoc/>
      public override string ToString()
      {
         if (RecorderAddress != null &&  RecordingType != null)
         {
            if (Stream == 1)
            {
               return $"{RecorderAddress}, Mainchannel, {RecordingType}";
            }
            if (Stream == 2)
            {
               return $"{RecorderAddress}, Subchannel, {RecordingType}";
            }
            else return null;
         }

         else return null;

      }

      #endregion
   }
}
