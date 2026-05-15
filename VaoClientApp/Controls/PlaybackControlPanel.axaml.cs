using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Vao.Client.Components;

namespace Vao.Sample.Controls
{
   public partial class PlaybackControlPanel : UserControl
   {
      /// <summary>Fired when the user clicks Play with a valid PlaybackInfo selected. String is the playback URL.</summary>
      public event EventHandler<string> PlayRequested;

      /// <summary>Fired when the user clicks Stop.</summary>
      public event EventHandler StopRequested;

      /// <summary>Fired when the user clicks Go to Time. String is the URL with ?start= appended.</summary>
      public event EventHandler<string> GotoTimeRequested;

      /// <summary>Fired when the user clicks the date picker button.</summary>
      public event EventHandler PickDateRequested;

      /// <summary>Fired when the user clicks the time picker button.</summary>
      public event EventHandler PickTimeRequested;

      public PlaybackControlPanel()
      {
         InitializeComponent();
      }

      // ── Public API ─────────────────────────────────────────────────────────

      public StackPanel PlaybackGroup
      {
         get { return grpSelectPlayback; }
      }

      public void FillRecordings(Camera camera, Guid viewerId)
      {
         var list = camera?.GetPlaybackInfoList(viewerId);
         if (list != null && list.Count > 0)
            selPlayback.ItemsSource = list;
         else
            ClearRecordings(false);
      }

      public void ClearRecordings(bool apiSupportsPlayback)
      {
         if (selPlayback != null)
         {
            selPlayback.ItemsSource = null;
            var items = new List<object>();
            items.Add(!apiSupportsPlayback ? "Api Version does not support playback" : "No camera selected");
            selPlayback.ItemsSource = items;
            selPlayback.SelectedIndex = 0;
         }
      }

      public void UpdateButtonStates(bool canUse, bool isCameraSelected, bool isPlaybackStarted, bool apiSupportsPlayback, bool isPlayback)
      {
         if (btnStopPlayback != null) btnStopPlayback.IsEnabled = canUse && isPlayback && apiSupportsPlayback;
         if (btnPlayPlayback != null) btnPlayPlayback.IsEnabled = canUse && isCameraSelected && !isPlaybackStarted && apiSupportsPlayback;
         if (btnGotoTime != null) btnGotoTime.IsEnabled = canUse && isCameraSelected && apiSupportsPlayback;
      }

      public string DateText
      {
         get { return txtDatePlayback?.Text; }
         set
         {
            if (txtDatePlayback != null)
               txtDatePlayback.Text = value;
         }
      }

      public string TimeText
      {
         get { return txtTimePlayback?.Text; }
         set
         {
            if (txtTimePlayback != null)
               txtTimePlayback.Text = value;
         }
      }

      // ── Event handlers ─────────────────────────────────────────────────────

      private void btnPlayPlayback_Click(object sender, RoutedEventArgs e)
      {
         if (selPlayback?.SelectedItem is PlaybackInfo recording)
         {
            string url = recording.PlaybackUrl;
            if (!string.IsNullOrEmpty(url))
               PlayRequested?.Invoke(this, url);
         }
      }

      private void btnStopPlayback_Click(object sender, RoutedEventArgs e)
      {
         StopRequested?.Invoke(this, EventArgs.Empty);
      }

      private void btnGotoTime_Click(object sender, RoutedEventArgs e)
      {
         if (selPlayback?.SelectedItem is not PlaybackInfo recording) return;
         try
         {
            DateTime date = DateTime.Now.Date;
            if (!string.IsNullOrWhiteSpace(txtDatePlayback?.Text) && DateTime.TryParse(txtDatePlayback.Text, out DateTime parsedDate))
               date = parsedDate.Date;

            TimeSpan time = TimeSpan.Zero;
            if (!string.IsNullOrWhiteSpace(txtTimePlayback?.Text) && TimeSpan.TryParse(txtTimePlayback.Text, out TimeSpan parsedTime))
               time = parsedTime;

            var dt = date + time;
            string url = recording.PlaybackUrl + $"?start={dt:yyyyMMddHHmmss}";
            GotoTimeRequested?.Invoke(this, url);
         }
         catch { }
      }

      private void btnPickDate_Click(object sender, RoutedEventArgs e) => PickDateRequested?.Invoke(this, EventArgs.Empty);
      private void btnPickTime_Click(object sender, RoutedEventArgs e) => PickTimeRequested?.Invoke(this, EventArgs.Empty);

      private void selPlayback_SelectedIndexChanged(object sender, SelectionChangedEventArgs e) { }
   }
}
