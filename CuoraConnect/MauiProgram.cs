using Microsoft.Extensions.Logging;
using CuoraConnect.Services;

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
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("Poppins-ExtraBold.ttf", "Poppins-ExtraBold");
                    fonts.AddFont("Poppins-Medium.ttf", "Poppins-Medium");
                    fonts.AddFont("Poppins-Bold.ttf", "Poppins-Bold");
                    fonts.AddFont("fontello.ttf", "IconsFont");
                });

            builder.Services.AddScoped<DigestAuthService>();


#if ANDROID
            builder.Services.AddSingleton<IFileUploadService, Platforms.Android.FileUploadService>();
            builder.Services.AddSingleton<IFileExportService, Platforms.Android.FileExportService>();
#elif WINDOWS
            builder.Services.AddSingleton<IFileUploadService, Platforms.Windows.FileUploadService>();
            // Se houver uma implementação para Windows, registre aqui
#endif
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

#if ANDROID
            builder.Services.AddSingleton<INetworkService, Platforms.Android.NetworkService>();
#elif IOS
            // Se houver uma implementação para iOS, registre aqui
            // builder.Services.AddSingleton<INetworkService, Platforms.iOS.NetworkService>();
#elif WINDOWS
            builder.Services.AddSingleton<INetworkService, Platforms.Windows.NetworkService>();
            // Se houver uma implementação para Windows, registre aqui
#endif

            return builder.Build();
        }
    }
}
