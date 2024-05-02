using Microsoft.Maui.Controls;
using DesktopApp.ViewModels;
using CryptoLib;
using Org.BouncyCastle.Security;
using DesktopApp.HttpFolder;

namespace DesktopApp
{

    public partial class SignInPage : ContentPage
	{
		public SignInPage()
		{
			InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            SecureStorage.Default.RemoveAll();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SecureStorage.Default.RemoveAll();
        }
        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignUpPage());
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (BindingContext is SignInViewModel viewModel)
            {
                loadingFrame.IsVisible = true;
                int login = await viewModel.Login();
                loadingFrame.IsVisible = false;
                if (login == 1)
                {

                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    await DisplayAlert("Error", "Invalid email or password", "OK");
                }
                
                
            }
        }
    }
}