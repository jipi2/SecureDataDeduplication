using DesktopApp.ViewModels;

namespace DesktopApp
{

	public partial class ResetPasswordPage : ContentPage
	{
		public ResetPasswordPage()
		{
			InitializeComponent();
		}

        private async void resetPassButton_Clicked(object sender, EventArgs e)
        {
			if(BindingContext is ResetPasswordViewModel viewModel)
			{
				try
				{
					await viewModel.ResetPassword();
					await DisplayAlert("Info", "Your password has been changed", "OK");
					await Navigation.PopAsync();
				}
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }
            }
        }
    }
}