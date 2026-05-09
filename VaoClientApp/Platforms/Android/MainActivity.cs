using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Avalonia;
using Avalonia.Android;
using AndroidX.Core.View;
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

        // Apply window insets so the UI is not overlapped by the status bar and navigation bar.
        var androidWindow = ((Activity)this).Window;
        ViewCompat.SetOnApplyWindowInsetsListener(androidWindow.DecorView, new WindowInsetsCallback(this));
        ApplySystemBarColors();

        // App.OnFrameworkInitializationCompleted() was called by base.OnCreate,
        // so AndroidMainView is now set. Create the video controller here so we
        // can pass the Activity reference it needs for native view management.
        if (Avalonia.Application.Current is App app && app.AndroidMainView != null)
            mVideoController = new AndroidVideoController(app.AndroidMainView, this);
    }

    public override void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);
        ApplySystemBarColors();
    }

    private void ApplySystemBarColors()
    {
        bool isDarkMode = (Resources.Configuration.UiMode & UiMode.NightMask) == UiMode.NightYes;

        // In dark mode the nav bar buttons are light, so use a dark background so they are visible.
        // In light mode the nav bar buttons are dark, so use a light background.
        var barColor = isDarkMode ? Color.Black : Color.White;
        var androidWindow = ((Activity)this).Window;

        // SetStatusBarColor/SetNavigationBarColor are ignored on Android 15+ (API 35) when
        // targetSdkVersion >= 35 because the system enforces edge-to-edge transparent bars.
        // Setting the DecorView background covers the bar areas (which are exposed as padding)
        // on all API levels, and works as the primary color mechanism on API 35+.
        androidWindow.DecorView.SetBackgroundColor(barColor);
        androidWindow.SetStatusBarColor(barColor);
        androidWindow.SetNavigationBarColor(barColor);

        // Tell the system whether the bar icons should be drawn dark (for light backgrounds)
        // or light (for dark backgrounds).
        var controller = WindowCompat.GetInsetsController(androidWindow, androidWindow.DecorView);
        controller.AppearanceLightStatusBars = !isDarkMode;
        controller.AppearanceLightNavigationBars = !isDarkMode;
    }

    private sealed class WindowInsetsCallback : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        private readonly Activity mActivity;

        public WindowInsetsCallback(Activity activity)
        {
            mActivity = activity;
        }

        public WindowInsetsCompat OnApplyWindowInsets(View v, WindowInsetsCompat insets)
        {
            var bars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
            v.SetPadding(bars.Left, bars.Top, bars.Right, bars.Bottom);
            return WindowInsetsCompat.Consumed;
        }
    }
}
