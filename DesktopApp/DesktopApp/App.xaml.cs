
using DesktopApp.HttpFolder;
using System.Diagnostics;

namespace DesktopApp
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
            
          
        }
        protected override async void OnStart()
        {
            // Navigate to SignUpPage
            //Shell.Current.GoToAsync("//SignInPage");


            if (MainPage is AppShell shell)
            {
                await shell.SetRootPageSignIn();

                //string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
                //var httpClient = HttpServiceCustom.GetApiClient();
                //httpClient.DefaultRequestHeaders.Remove("Authorization");
                //httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
                //try
                //{
                //    var response = await httpClient.GetAsync("/api/User/isConnected");
                //    if (response.IsSuccessStatusCode)
                //    {
                //        await shell.SetRootPageMainPage();
                //    }
                //    else
                //    {
                //        await shell.SetRootPageSignIn();
                //    }
                //}
                //catch (Exception e)
                //{
                //   Debug.WriteLine(e.Message);
                //}
            }
        }

        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    var window = base.CreateWindow(activationState);

        //    const int newWidth = 1920;
        //    const int newHeight = 1080;

        //    window.Width = newWidth;
        //    window.Height = newHeight;

        //    window.MaximumHeight = newHeight;
        //    window.MinimumHeight = newHeight;

        //    window.MinimumWidth = newWidth;
        //    window.MaximumWidth = newWidth;


        //    return window;
        //}
    }
}
