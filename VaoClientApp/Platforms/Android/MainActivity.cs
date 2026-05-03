using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Vao.Sample;

namespace Com.Vao.Clientapp;

[Activity(
   Name = "com.vao.clientapp.MainActivity",
   Label = "VaoClientApp",
   Exported = true,
   MainLauncher = true,
   ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    protected override AppBuilder CreateAppBuilder()
    {
        return AppBuilder.Configure<App>()
            .UseAndroid()
            .LogToTrace();
    }
}
