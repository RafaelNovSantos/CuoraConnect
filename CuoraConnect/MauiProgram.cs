﻿using Microsoft.Extensions.Logging;
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
                });




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
            // Se houver uma implementação para Windows, registre aqui
            // builder.Services.AddSingleton<INetworkService, Platforms.Windows.NetworkService>();
#endif

            return builder.Build();
        }
    }
}
