using Microsoft.Maui.Controls;
using DesktopApp.ViewModels;
using CryptoLib;
using Org.BouncyCastle.Security;
using DesktopApp.HttpFolder;
using CommunityToolkit.Maui.Views;
using Syncfusion.Maui.Core.Carousel;


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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            SecureStorage.Default.RemoveAll();

            //stergem asta dupa testare
            //if (BindingContext is SignInViewModel viewModel)
            //{
            //    await viewModel.Login();
            //    await Shell.Current.GoToAsync("//MainPage");
            //}
            //atat stergem
        }
        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new SignUpPage());
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            try
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
                    else if (login == 2)
                    {
                        await DisplayAlert("Info", "You need to verify your email address", "OK");
                        await viewModel.sendEmail();
                        var result = await this.ShowPopupAsync(new CodePage(viewModel.Email, viewModel.Password, false));
                        string? jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
                        if (jwt != null)
                        {
                            await Shell.Current.GoToAsync("//MainPage");
                        }
                        else
                            await DisplayAlert("Error", "Login Failed!", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Invalid email or password", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}