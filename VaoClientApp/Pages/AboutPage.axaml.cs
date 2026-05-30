using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Vao.Sample.Navigation;

namespace Vao.Sample.Pages;

/// <summary>
/// About page showing application information, links, and open source library credits.
/// </summary>
public partial class AboutPage : NavigableViewBase
{
   public AboutPage()
   {
      InitializeComponent();
   }

   private void InitializeComponent()
   {
      AvaloniaXamlLoader.Load(this);
   }

   /// <inheritdoc/>
   public override string GetPageTitle() => "About";

   private void btnBack_Click(object sender, RoutedEventArgs e) => GoBack();

   private void Hyperlink_Click(object sender, RoutedEventArgs e)
   {
      if (sender is Button button && button.Tag is string url)
      {
         OpenUrl(url);
      }
   }

   private static void OpenUrl(string url)
   {
      try
      {
         if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
         {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
         }
         else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
         {
            Process.Start("xdg-open", url);
         }
         else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
         {
            Process.Start("open", url);
         }
      }
      catch (Exception)
      {
         // Silently ignore if browser cannot be opened
      }
   }
}
