using System.ComponentModel;
using Avalonia.Media;
using LibVLCSharp.Shared;

namespace Vao.Sample
{
   public class MessageItem : INotifyPropertyChanged
   {
      private string mText;
      private IBrush mColor;
      private IBrush mBackground;

      public string Text
      {
         get => mText;
         set { if (mText == value) return; mText = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text))); }
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
