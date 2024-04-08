using DesktopApp.ViewModels;
using FileStorageApp.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Python.Runtime;
using System.Reflection;
using System.Reflection.PortableExecutable;

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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
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
