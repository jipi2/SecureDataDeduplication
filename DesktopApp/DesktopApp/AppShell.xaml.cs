namespace DesktopApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("SignInPage", typeof(SignInPage));
            Routing.RegisterRoute("MainPage", typeof(MainPage));
/*            Routing.RegisterRoute("SignUpPage", typeof(SignUpPage));*/
            Routing.RegisterRoute("UploadPage", typeof(UploadPage));

       
            
        }

        public async Task SetRootPageSignIn()
        {
            await Shell.Current.GoToAsync("//SignInPage");
        }

        public async Task SetRootPageMainPage()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
