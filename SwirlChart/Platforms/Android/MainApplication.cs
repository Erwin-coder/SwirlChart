using System;
using Android.App;
using Android.Runtime;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace SwirlChart;

// This project has no Platforms/Android/AndroidManifest.xml - the manifest is
// generated from $(ApplicationId) plus these attributes, so the launcher icon has to
// be declared here or the app installs with Android's default placeholder icon.
// @mipmap/appicon and @mipmap/appicon_round are what MauiIcon emits from
// Resources/AppIcon/appicon.png (adaptive icon XML + per-density PNGs).
[Application(Icon = "@mipmap/appicon", RoundIcon = "@mipmap/appicon_round")]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
