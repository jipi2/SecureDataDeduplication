using CommunityToolkit.Maui.Views;
using DesktopApp.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace DesktopApp
{

	public partial class SignUpPage : ContentPage
	{
		public SignUpPage()
		{
			InitializeComponent();
        }
        //protected async override void OnAppearing()
        //{
        //    base.OnAppearing();
        //    this.ShowPopup(new CodePage("", "", true));
        //}
        private async void registerButton_Clicked(object sender, EventArgs e)
        {
            loadingFrame.IsVisible = true;
            if (BindingContext is SignUpViewModel viewModel)
			{
			
				try
				{
					bool registered = await viewModel.Register();
					if (registered)
					{
						//await DisplayAlert(Enums.Symbol.Success.ToString(), "User registered successfully", "OK");
						await DisplayAlert("Info", "You need to verify your email address", "OK");
						this.ShowPopup(new CodePage(viewModel.Email, viewModel.Password, true));

                        loadingFrame.IsVisible = false;
						await Navigation.PopModalAsync();
					}
					else
						loadingFrame.IsVisible = false;
                }
				catch (Exception ex)
				{
					loadingFrame.IsVisible = false;
					await DisplayAlert(Enums.Symbol.Error.ToString(), ex.Message, "OK");
				}

			}
			else
			{
                loadingFrame.IsVisible = false;
                await DisplayAlert(Enums.Symbol.Error.ToString(), "Error registering user", "OK");
			}

        }

        private async void cancelButton_Clicked(object sender, EventArgs e)
        {
			await Navigation.PopModalAsync();
        }
    }
}