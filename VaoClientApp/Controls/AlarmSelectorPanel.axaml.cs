using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Vao.Client.Components;
using Vao.Client.Enum;

namespace Vao.Sample.Controls
{
   public partial class AlarmSelectorPanel : UserControl
   {
      private readonly ObservableCollection<AlarmSelectionItem> mAlarmSelectionItems = new();
      private readonly ObservableCollection<AlarmSelectionItem> mFilteredAlarmSelectionItems = new();
      private readonly Dictionary<Alarm, PropertyChangedEventHandler> mAlarmStatusChangedHandlers = new();
      private bool mIsUpdatingSelection;
      private Alarm mCurrentAlarm;

      private bool mIsResizing;
      private double mResizeStartHeight;
      private Point mResizeStartPoint;

      private Dictionary<AlarmGeneralStatus, bool> mAlarmStatusFilters = new()
      {
         { AlarmGeneralStatus.Active, true },
         { AlarmGeneralStatus.Inactive, true },
         { AlarmGeneralStatus.Acknowledged, true },
         { AlarmGeneralStatus.Tampered, true },
         { AlarmGeneralStatus.Disabled, true },
         { AlarmGeneralStatus.Unknown, true }
      };

      /// <summary>Fired when the user selects an alarm. EventArgs contains the component number.</summary>
      public event EventHandler<int> AlarmSelected;

      /// <summary>Fired when the user clicks the edit button on an alarm.</summary>
      public event EventHandler<Alarm> AlarmEditRequested;

      /// <summary>Fired when layout changes (resize, filter) so settings can be saved.</summary>
      public event EventHandler LayoutChanged;

      /// <summary>Fired when the active alarm count changes so the sidebar icon can be updated.</summary>
      public event EventHandler ActiveAlarmStateChanged;

      public AlarmSelectorPanel()
      {
         InitializeComponent();
         lstAlarmSelection.ItemsSource = mFilteredAlarmSelectionItems;
         InitializeFilterMenu();
      }

      // ── Public API ─────────────────────────────────────────────────────────

      public Grid SelectionPanel
      {
         get { return pnlAlarmsSelection; }
      }

      public bool HasActiveAlarm
      {
         get { return mAlarmSelectionItems.Any(item => item.Alarm?.Status == AlarmGeneralStatus.Active); }
      }

      public void Fill(List<Alarm> alarmList, Func<AlarmGeneralStatus, IBrush> brushResolver)
      {
         Clear();
         if (alarmList == null) return;

         foreach (var alarm in alarmList.OrderBy(a => a.ComponentNumber))
         {
            var item = new AlarmSelectionItem(alarm, brushResolver(alarm.Status), GetTextForStatus(alarm.Status))
            {
               AlarmIcon = GetIconForAlarmStatus(alarm.Status)
            };
            mAlarmSelectionItems.Add(item);
            PropertyChangedEventHandler alarmPropertyChangedHandler = (s, ev) =>
            {
               if (ev.PropertyName == nameof(Alarm.Status))
               {
                  Dispatcher.UIThread.Post(() =>
                  {
                     item.StatusBrush = brushResolver(alarm.Status);
                     item.StatusText = GetTextForStatus(alarm.Status);
                     item.AlarmIcon = GetIconForAlarmStatus(alarm.Status);
                     ApplySearchFilter();
                     ActiveAlarmStateChanged?.Invoke(this, EventArgs.Empty);
                  });
               }
            };
            mAlarmStatusChangedHandlers[alarm] = alarmPropertyChangedHandler;
            alarm.PropertyChanged += alarmPropertyChangedHandler;
         }

         ApplySearchFilter();
         ActiveAlarmStateChanged?.Invoke(this, EventArgs.Empty);
      }

      public void Clear()
      {
         UnsubscribeAlarmHandlers();
         mAlarmSelectionItems.Clear();
         mFilteredAlarmSelectionItems.Clear();
         mIsUpdatingSelection = true;
         if (lstAlarmSelection != null) lstAlarmSelection.SelectedItem = null;
         mIsUpdatingSelection = false;
         if (txtAlarmSearch != null) txtAlarmSearch.Text = string.Empty;
      }

      public void SyncSelection(Alarm currentAlarm)
      {
         mCurrentAlarm = currentAlarm;
         mIsUpdatingSelection = true;
         if (lstAlarmSelection != null)
         {
            lstAlarmSelection.SelectedItem = mCurrentAlarm == null
               ? null
               : mFilteredAlarmSelectionItems.FirstOrDefault(item => item.Alarm?.ComponentNumber == mCurrentAlarm.ComponentNumber);
         }
         mIsUpdatingSelection = false;
      }

      public void InitializeHeight(double height, double minHeight, double maxHeight)
      {
         if (lstAlarmSelection == null) return;
         lstAlarmSelection.MinHeight = minHeight;
         lstAlarmSelection.MaxHeight = maxHeight;
         lstAlarmSelection.Height = height;
      }

      public double ListHeight
      {
         get { return lstAlarmSelection?.Height ?? 0; }
      }

      public void SetStatusFilters(Dictionary<AlarmGeneralStatus, bool> filters)
      {
         foreach (var kvp in filters)
            mAlarmStatusFilters[kvp.Key] = kvp.Value;
         InitializeFilterMenu();
         ApplySearchFilter();
      }

      public Dictionary<AlarmGeneralStatus, bool> GetStatusFilters() => new(mAlarmStatusFilters);

      // ── Event handlers ─────────────────────────────────────────────────────

      private void txtAlarmSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplySearchFilter();

      private void lstAlarmSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         if (mIsUpdatingSelection) return;
         if (lstAlarmSelection?.SelectedItem is not AlarmSelectionItem selected || selected.Alarm == null) return;
         AlarmSelected?.Invoke(this, selected.Alarm.ComponentNumber);
      }

      private void btnAlarmEdit_Click(object sender, RoutedEventArgs e)
      {
         if (sender is not Button btn) return;
         var item = btn.DataContext as AlarmSelectionItem;
         if (item?.Alarm == null) return;
         AlarmEditRequested?.Invoke(this, item.Alarm);
      }

      private void AlarmStatusFilter_Click(object sender, RoutedEventArgs e)
      {
         if (sender is not MenuItem menuItem || menuItem.Tag is not string tagValue) return;
         if (!Enum.TryParse<AlarmGeneralStatus>(tagValue, ignoreCase: true, out var status)) return;
         mAlarmStatusFilters[status] = !mAlarmStatusFilters[status];
         menuItem.Icon = mAlarmStatusFilters[status] ? new TextBlock { Text = "\u2713" } : null;
         ApplySearchFilter();
         LayoutChanged?.Invoke(this, EventArgs.Empty);
      }

      private void InitializeFilterMenu()
      {
         if (ctxAlarmStatusFilter?.Items == null) return;
         foreach (var item in ctxAlarmStatusFilter.Items.OfType<MenuItem>())
         {
            if (item.Tag is string tagValue && Enum.TryParse<AlarmGeneralStatus>(tagValue, ignoreCase: true, out var status))
               item.Icon = mAlarmStatusFilters.GetValueOrDefault(status, true) ? new TextBlock { Text = "\u2713" } : null;
         }
      }

      private void ApplySearchFilter()
      {
         string query = txtAlarmSearch?.Text?.Trim() ?? string.Empty;
         var filtered = string.IsNullOrWhiteSpace(query)
            ? mAlarmSelectionItems.AsEnumerable()
            : mAlarmSelectionItems.Where(item => item.Matches(query));

         filtered = filtered.Where(item => item.Alarm != null && mAlarmStatusFilters.GetValueOrDefault(item.Alarm.Status, true));

         mFilteredAlarmSelectionItems.Clear();
         foreach (var item in filtered)
            mFilteredAlarmSelectionItems.Add(item);

         mIsUpdatingSelection = true;
         if (lstAlarmSelection != null)
         {
            lstAlarmSelection.SelectedItem = mCurrentAlarm == null
               ? null
               : mFilteredAlarmSelectionItems.FirstOrDefault(item => item.Alarm?.ComponentNumber == mCurrentAlarm.ComponentNumber);
         }
         mIsUpdatingSelection = false;
      }

      // ── Resize ─────────────────────────────────────────────────────────────

      private void ResizeHandle_PointerPressed(object sender, PointerPressedEventArgs e)
      {
         if (lstAlarmSelection == null) return;
         mIsResizing = true;
         mResizeStartPoint = e.GetPosition(this);
         mResizeStartHeight = lstAlarmSelection.Height > 0 ? lstAlarmSelection.Height : lstAlarmSelection.Bounds.Height;
         if (sender is InputElement ie) e.Pointer.Capture(ie);
      }

      private void ResizeHandle_PointerMoved(object sender, PointerEventArgs e)
      {
         if (!mIsResizing || lstAlarmSelection == null) return;
         var minH = AppConstants.Default.ResizableSidebarMenuMinHeight;
         var maxH = AppConstants.Default.ResizableSidebarMenuMaxHeight;
         var delta = e.GetPosition(this).Y - mResizeStartPoint.Y;
         lstAlarmSelection.Height = Math.Clamp(mResizeStartHeight + delta, minH, maxH);
      }

      private void ResizeHandle_PointerReleased(object sender, PointerReleasedEventArgs e) => EndResize(e.Pointer);
      private void ResizeHandle_PointerCaptureLost(object sender, PointerCaptureLostEventArgs e) => EndResize(null);

      private void EndResize(IPointer pointer)
      {
         if (!mIsResizing) return;
         mIsResizing = false;
         pointer?.Capture(null);
         LayoutChanged?.Invoke(this, EventArgs.Empty);
      }

      private void UnsubscribeAlarmHandlers()
      {
         foreach (var kvp in mAlarmStatusChangedHandlers)
            kvp.Key.PropertyChanged -= kvp.Value;

         mAlarmStatusChangedHandlers.Clear();
      }

      // ── Helpers ────────────────────────────────────────────────────────────

      private static string GetTextForStatus(AlarmGeneralStatus status) => status switch
      {
         AlarmGeneralStatus.Active       => "(Active)",
         AlarmGeneralStatus.Inactive     => "(Inactive)",
         AlarmGeneralStatus.Acknowledged => "(Acknowledged)",
         AlarmGeneralStatus.Tampered     => "(Tampered)",
         AlarmGeneralStatus.Disabled     => "(Disabled)",
         _                               => "(Unknown)",
      };

      private static string GetIconForAlarmStatus(AlarmGeneralStatus status) => status switch
      {
         AlarmGeneralStatus.Active   => "\uE7F7",
         AlarmGeneralStatus.Tampered => "\uE004",
         AlarmGeneralStatus.Inactive => "\uE7F4",
         AlarmGeneralStatus.Disabled => "\uE7F6",
         _                           => "\uE7F4",
      };
   }
}
