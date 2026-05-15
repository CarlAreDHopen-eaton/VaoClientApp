using System.ComponentModel;
using Avalonia.Media;
using LibVLCSharp.Shared;

namespace Vao.Sample.Controls
{
   public class MessageItem : INotifyPropertyChanged
   {
      private string mText;
      private string mTimestamp;
      private string mLevelText;
      private string mSourceText;
      private string mMessage;
      private IBrush mColor;
      private IBrush mBackground;

      public string Text
      {
         get => mText;
         set { if (mText == value) return; mText = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text))); }
      }

      public string Timestamp
      {
         get => mTimestamp;
         set { if (mTimestamp == value) return; mTimestamp = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Timestamp))); }
      }

      public string LevelText
      {
         get => mLevelText;
         set { if (mLevelText == value) return; mLevelText = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LevelText))); }
      }

      public string SourceText
      {
         get => mSourceText;
         set { if (mSourceText == value) return; mSourceText = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceText))); }
      }

      public string Message
      {
         get => mMessage;
         set { if (mMessage == value) return; mMessage = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message))); }
      }

      public IBrush Color
      {
         get => mColor;
         set { if (ReferenceEquals(mColor, value)) return; mColor = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color))); }
      }

      public IBrush Background
      {
         get => mBackground;
         set { if (ReferenceEquals(mBackground, value)) return; mBackground = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Background))); }
      }

      public MessageSource Source { get; set; }
      public LogLevel Level { get; set; }

      public event PropertyChangedEventHandler PropertyChanged;
   }
}
