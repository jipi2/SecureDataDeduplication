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
						await DisplayAlert(Enums.Symbol.Success.ToString(), "User registered successfully", "OK");
                        loadingFrame.IsVisible = false;
                        await Navigation.PopAsync();
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
    }
}