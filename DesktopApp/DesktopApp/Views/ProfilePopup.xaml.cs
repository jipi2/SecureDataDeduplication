using DesktopApp.HttpFolder;
using DesktopApp.ViewModels;
using FileStorageApp.Shared.Dto;
using Mopups.Services;
using System.Diagnostics;
using System.Net.Http.Json;

namespace DesktopApp
{

	public partial class ProfilePopup
	{
        public ProfilePopup()
		{
			InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            BindingContext = new ProfileViewModel();

            if (BindingContext is ProfileViewModel viewModel)
            {
                await viewModel.GetEmail();
                await viewModel.GetMailDomain();
                mailDomainLabel.Text = viewModel.MailDomain;
                mailText.Text = viewModel.UserEmail;
                await viewModel.GetFirstNameAndLastName();
                nameText.Text = viewModel.Name;
                await viewModel.GetInitials();
                initialsButton.Text = viewModel.Initials;
            }

        }
        private async void signOutButton_Clicked(object sender, EventArgs e)
        {
            bool result = await DisplayAlert("Sign out", "Are you sure you want to sign out?", "Yes", "No");
            if(result)
            {
                await MopupService.Instance.PopAsync();
                await Shell.Current.GoToAsync("//SignInPage");
            }
        }
        private void initialsButton_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Test");
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            this.IsVisible = false;
            await Navigation.PushAsync(new ResetPasswordPage());
        }

    }
}