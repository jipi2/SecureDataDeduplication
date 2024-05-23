using CommunityToolkit.Maui;
using DesktopApp.KeysService;
using DesktopApp.ViewModels;
using FileStorageApp.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using Python.Runtime;
using Syncfusion.Maui.Core.Hosting;
using System.Reflection;
using System.Security.Cryptography;
using UraniumUI;

namespace DesktopApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
 
            //Runtime.PythonDLL = "C:\\Users\\Jipi\\AppData\\Local\\Programs\\Python\\Python311\\python311.dll";
            var builder = MauiApp.CreateBuilder();

            var getAssembly = Assembly.GetExecutingAssembly();
            using var stream = getAssembly.GetManifestResourceStream("DesktopApp.appsettings.json");

            var config = new ConfigurationBuilder()
              .AddJsonStream(stream)
              .Build();

            builder.Configuration.AddConfiguration(config);

            var pythonPath = config.GetSection("PythonDllPath");
            Runtime.PythonDLL = pythonPath.Value;

            builder
                .UseMauiApp<App>()
                .ConfigureMopups()
                .UseMauiCommunityToolkit()
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Merriweather-Black.ttf", "FontText1");

                    //Font Awesome
                    fonts.AddFont("Brands-Regular-400.otf", "FAB");
                    fonts.AddFont("Free-Regular-400.otf", "FAR");
                    fonts.AddFont("Free-Solid-900.otf", "FAS");

                });

            builder.Services.AddTransient<MainWindowViewModel>();

            builder.Services.AddScoped<CryptoService>();
            builder.Services.AddScoped<SignUpViewModel>();
            builder.Services.AddSingleton<IConfiguration>(config);
            builder.Services.AddSingleton<RSAKeyService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif


            return builder.Build();
        }
    }
}
