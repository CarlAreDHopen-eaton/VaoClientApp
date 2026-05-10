using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LibVLCSharp.Shared;

namespace Vao.Sample.Controls
{
   public partial class MessageLogPanel : UserControl
   {
      private readonly ObservableCollection<MessageItem> mMessages = new();
      private readonly ObservableCollection<MessageItem> mFilteredMessages = new();
      private bool mIsMessagesCollapsed;
      private GridLength mMessagesExpandedRowHeight = new GridLength(1, GridUnitType.Star);

      private readonly Dictionary<MessageSource, bool> mSourceFilters = new()
      {
         { MessageSource.FlexApi, true },
         { MessageSource.LibVlc, true },
         { MessageSource.Config, true }
      };

      /// <summary>Fired when the user collapses/expands or drags the splitter, so settings can be saved.</summary>
      public event EventHandler LayoutChanged;

      public bool IsMessagesCollapsed => mIsMessagesCollapsed;

      public MessageLogPanel()
      {
         InitializeComponent();
         lstMessages.ItemsSource = mFilteredMessages;
      }

      // ── Public API ─────────────────────────────────────────────────────────

      public void WriteMessageLog(MessageSource source, string strMessage, LogLevel level)
      {
         if (!Dispatcher.UIThread.CheckAccess())
         {
            Dispatcher.UIThread.Post(() => WriteMessageLog(source, strMessage, level));
            return;
         }

         string strSource = source.ToString().PadRight(7);
         var strTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
         var strLevel = level.ToString().PadRight(7);
         var strMsg = $"{strTime} [{strLevel}][{strSource}] - {strMessage}";

#if ANDROID
         string adbTag = $"VaoClient/{source}";
         string adbMsg = $"[{strLevel.Trim()}] {strMessage}";
         switch (level)
         {
            case LogLevel.Debug:   Android.Util.Log.Debug(adbTag, adbMsg);   break;
            case LogLevel.Notice:  Android.Util.Log.Info (adbTag, adbMsg);   break;
            case LogLevel.Warning: Android.Util.Log.Warn (adbTag, adbMsg);   break;
            case LogLevel.Error:   Android.Util.Log.Error(adbTag, adbMsg);   break;
            default:               Android.Util.Log.Verbose(adbTag, adbMsg); break;
         }
#endif

         IBrush color = GetColorForLogLevel(level);

         var item = new MessageItem
         {
            Text = strMsg,
            Color = color,
            Background = GetBackgroundForLogLevel(level),
            Source = source,
            Level = level
         };

         mMessages.Add(item);
         if (mSourceFilters.TryGetValue(source, out bool visible) && visible)
         {
            bool wasAtEnd = mFilteredMessages.Count == 0 || IsScrolledToEnd();
            mFilteredMessages.Add(item);
            if (wasAtEnd && lstMessages != null)
               lstMessages.ScrollIntoView(mFilteredMessages.Count - 1);
         }
      }

      public void RefreshColors()
      {
         foreach (var item in mMessages)
         {
            item.Color = GetColorForLogLevel(item.Level);
            item.Background = GetBackgroundForLogLevel(item.Level);
         }
      }

      public void RestoreCollapsedState(bool collapsed)
      {
         if (collapsed)
         {
            mIsMessagesCollapsed = false;
            MessagesHeader_PointerPressed(null, null);
         }
      }

      public Border MessagesBorder => brdMessages;

      // ── Event handlers ─────────────────────────────────────────────────────

      private void MessagesHeader_PointerPressed(object sender, PointerPressedEventArgs e)
      {
         mIsMessagesCollapsed = !mIsMessagesCollapsed;
         var messagesGrid = this.Parent as Grid;
         if (messagesGrid == null) return;

         if (messagesGrid.RowDefinitions.Count <= 2) return;
         var messagesRow = messagesGrid.RowDefinitions[2];

         if (mIsMessagesCollapsed)
         {
            mMessagesExpandedRowHeight = messagesRow.Height;
            messagesRow.Height = GridLength.Auto;
            if (brdMessages != null) brdMessages.MaxHeight = 36;
            if (lstMessages != null) lstMessages.IsVisible = false;
            if (txtMessagesToggle != null) txtMessagesToggle.Text = "▶";
         }
         else
         {
            if (brdMessages != null) brdMessages.MaxHeight = double.PositiveInfinity;
            messagesRow.Height = mMessagesExpandedRowHeight;
            if (lstMessages != null) lstMessages.IsVisible = true;
            if (txtMessagesToggle != null) txtMessagesToggle.Text = "▼";
         }

         LayoutChanged?.Invoke(this, EventArgs.Empty);
      }

      private void btnClearMessages_Click(object sender, RoutedEventArgs e) { mMessages.Clear(); mFilteredMessages.Clear(); }

      private async void menuCopyMessages_Click(object sender, RoutedEventArgs e)
      {
         var text = string.Join(Environment.NewLine, mFilteredMessages.Select(m => m.Text));
         var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
         if (clipboard is { } cb) await cb.SetTextAsync(text);
      }

      private async void menuCopySelectedMessages_Click(object sender, RoutedEventArgs e) => await CopySelectedMessagesAsync();

      private async void lstMessages_KeyDown(object sender, KeyEventArgs e)
      {
         if (e.Key == Key.C && (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control)
         {
            await CopySelectedMessagesAsync();
            e.Handled = true;
         }
      }

      private async Task CopySelectedMessagesAsync()
      {
         var selectedText = string.Join(Environment.NewLine, lstMessages.SelectedItems.Cast<MessageItem>().Select(m => m.Text));
         var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
         if (!string.IsNullOrEmpty(selectedText) && clipboard is { } cb)
            await cb.SetTextAsync(selectedText);
      }

      private void menuScrollToEnd_Click(object sender, RoutedEventArgs e)
      {
         if (mFilteredMessages.Count > 0 && lstMessages != null)
            lstMessages.ScrollIntoView(mFilteredMessages.Count - 1);
      }

      private void menuFilterFlexApi_Click(object sender, RoutedEventArgs e) => ToggleSourceFilter(sender, MessageSource.FlexApi);
      private void menuFilterLibVlc_Click(object sender, RoutedEventArgs e)  => ToggleSourceFilter(sender, MessageSource.LibVlc);
      private void menuFilterConfig_Click(object sender, RoutedEventArgs e)  => ToggleSourceFilter(sender, MessageSource.Config);

      private void ToggleSourceFilter(object sender, MessageSource source)
      {
         if (sender is MenuItem menuItem && menuItem.Icon is CheckBox cb)
         {
            cb.IsChecked = !(cb.IsChecked ?? false);
            mSourceFilters[source] = cb.IsChecked ?? false;
            ApplyMessageFilter();
         }
      }

      // ── Helpers ────────────────────────────────────────────────────────────

      private void ApplyMessageFilter()
      {
         mFilteredMessages.Clear();
         foreach (var msg in mMessages)
            if (mSourceFilters.TryGetValue(msg.Source, out bool visible) && visible)
               mFilteredMessages.Add(msg);
         if (mFilteredMessages.Count > 0 && lstMessages != null)
            lstMessages.ScrollIntoView(mFilteredMessages.Count - 1);
      }

      private bool IsScrolledToEnd()
      {
         var scrollViewer = FindScrollViewer(lstMessages);
         if (scrollViewer != null)
            return scrollViewer.Offset.Y >= scrollViewer.Extent.Height - scrollViewer.Viewport.Height - 20;
         return true;
      }

      private ScrollViewer FindScrollViewer(Control parent)
      {
         if (parent is ScrollViewer sv) return sv;
         foreach (var child in Avalonia.VisualTree.VisualExtensions.GetVisualChildren(parent))
            if (child is Control c) { var result = FindScrollViewer(c); if (result != null) return result; }
         return null;
      }

      private IBrush GetColorForLogLevel(LogLevel level) => level switch
      {
         LogLevel.Error   => GetBrushResource("MessageErrorForeground", "#FF0000"),
         LogLevel.Warning => GetBrushResource("MessageWarningForeground", "#F0AA1F"),
         LogLevel.Debug   => GetBrushResource("MessageDebugForeground", "#ADD8E6"),
         _                => GetBrushResource("MessageDefaultForeground", "#FFFFFF"),
      };

      private IBrush GetBackgroundForLogLevel(LogLevel level) => GetBrushResource("MessageLogBackground", "#00000000");

      private IBrush GetBrushResource(string key, string fallback)
      {
         if (this.TryFindResource(key, this.ActualThemeVariant, out var resource) && resource is IBrush b)
            return b;
         return new SolidColorBrush(Color.Parse(fallback));
      }
   }
}
