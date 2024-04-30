using DesktopApp.HttpFolder;
using DesktopApp.ViewModels;
using FileStorageApp.Shared.Dto;
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
        private async void signOutButton_Clicked_1(object sender, EventArgs e)
        {
            Debug.WriteLine("test");
        }
        private void initialsButton_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Test");
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Test");
        }
    }
}