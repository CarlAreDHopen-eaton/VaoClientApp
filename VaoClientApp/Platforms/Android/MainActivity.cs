using Android.App;
using Android.Content.PM;
using Android.OS;
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
    private AndroidVideoController mVideoController;

    protected override AppBuilder CreateAppBuilder()
    {
        return AppBuilder.Configure<App>()
            .UseAndroid()
            .LogToTrace();
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // App.OnFrameworkInitializationCompleted() was called by base.OnCreate,
        // so AndroidMainView is now set. Create the video controller here so we
        // can pass the Activity reference it needs for native view management.
        if (Avalonia.Application.Current is App app && app.AndroidMainView != null)
            mVideoController = new AndroidVideoController(app.AndroidMainView, this);
    }
}
