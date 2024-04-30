using CommunityToolkit.Maui;
using FileStorageApp.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using Python.Runtime;
using System.Reflection;

namespace DesktopApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Runtime.PythonDLL = "C:\\Users\\Jipi\\AppData\\Local\\Programs\\Python\\Python311\\python311.dll";
            var builder = MauiApp.CreateBuilder();

            var getAssembly = Assembly.GetExecutingAssembly();
            using var stream = getAssembly.GetManifestResourceStream("DesktopApp.appsettings.json");

            var config = new ConfigurationBuilder()
              .AddJsonStream(stream)
              .Build();

            builder.Configuration.AddConfiguration(config);

            builder
                .UseMauiApp<App>()
                .ConfigureMopups()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                    //Font Awesome
                    fonts.AddFont("Brands-Regular-400.otf", "FAB");
                    fonts.AddFont("Free-Regular-400.otf", "FAR");
                    fonts.AddFont("Free-Solid-900.otf", "FAS");

                });


            builder.Services.AddScoped<CryptoService>();
            builder.Services.AddSingleton<IConfiguration>(config);

#if DEBUG
            builder.Logging.AddDebug();
#endif


            return builder.Build();
        }
    }
}
