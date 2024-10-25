using Microsoft.Extensions.Logging;
using CuoraConnect.Services;
using MudBlazor.Services;
using Microsoft.Maui.LifecycleEvents;

namespace CuoraConnect
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Poppins-ExtraBold.ttf", "Poppins-ExtraBold");
                    fonts.AddFont("Poppins-Medium.ttf", "Poppins-Medium");
                    fonts.AddFont("Poppins-Bold.ttf", "Poppins-Bold");
                    fonts.AddFont("fontello.ttf", "IconsFont");
                });

            builder.Services.AddSingleton<NavigationInterceptor>();

            builder.Services.AddMudServices();
            builder.Services.AddScoped<DigestAuthService>();
            builder.Services.AddScoped<IExportInfoService, ExportInfoService>();

            builder.ConfigureLifecycleEvents(events => {
#if ANDROID
                events.AddAndroid(android=> android.OnCreate((activity,bundle) => MakeNavigationBarTranslucent(activity)));
                static void MakeNavigationBarTranslucent(Android.App.Activity activity){
                activity.Window.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#00357a"));
    }
#endif

            });

#if ANDROID
            builder.Services.AddSingleton<IFileUploadService, Platforms.Android.FileUploadService>();
            builder.Services.AddSingleton<IFileExportService, Platforms.Android.FileExportService>();
             builder.Services.AddSingleton<INetworkService, Platforms.Android.NetworkService>();
#elif WINDOWS
            builder.Services.AddSingleton<IFileUploadService, Platforms.Windows.FileUploadService>();
              builder.Services.AddSingleton<INetworkService, Platforms.Windows.NetworkService>();
            // Se houver uma implementação para Windows, registre aqui
#endif
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}
