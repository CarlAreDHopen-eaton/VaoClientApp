using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Media;
using Vao.Client.Components;

namespace Vao.Sample.Controls
{
   public class AlarmSelectionItem : INotifyPropertyChanged
   {
      private IBrush mStatusBrush;
      private string mStatusText;
      private string mAlarmIcon = "\uE7F4";

      public AlarmSelectionItem(Alarm alarm, IBrush statusBrush, string statusText)
      {
         Alarm = alarm;
         mStatusBrush = statusBrush;
         mStatusText = statusText;
      }

      public Alarm Alarm { get; }
      public string AlarmName       => Alarm?.Name ?? string.Empty;
      public string AlarmNumberText => Alarm == null ? string.Empty : $"#{Alarm.ComponentNumber:D4}";

      public IBrush StatusBrush
      {
         get => mStatusBrush;
         set { if (ReferenceEquals(mStatusBrush, value)) return; mStatusBrush = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusBrush))); }
      }

      public string StatusText
      {
         get => mStatusText;
         set { if (mStatusText == value) return; mStatusText = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText))); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TooltipText))); }
      }

      public string TooltipText => $"{AlarmNumberText} {AlarmName} {StatusText}".Trim();

      public string AlarmIcon
      {
         get => mAlarmIcon;
         set { if (mAlarmIcon == value) return; mAlarmIcon = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlarmIcon))); }
      }

      public bool Matches(string query)
      {
         if (Alarm == null || string.IsNullOrWhiteSpace(query)) return Alarm != null;
         return Alarm.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) == true
            || Alarm.ComponentNumber.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.OrdinalIgnoreCase);
      }

      public event PropertyChangedEventHandler PropertyChanged;
   }
}
