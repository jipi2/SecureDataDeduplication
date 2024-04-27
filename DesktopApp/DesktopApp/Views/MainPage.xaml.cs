using CommunityToolkit.Maui.Views;
using DesktopApp.Dto;
using DesktopApp.ViewModels;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DesktopApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            //ShellContent content = AppShell.Current.FindByName<ShellContent>("SignInShell");
            //content.IsVisible = false;
            //content = AppShell.Current.FindByName<ShellContent>("SignUpShell");
            //content.IsVisible = false;
        }


        protected async override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is MainWindowViewModel viewModel)
            {
                viewModel.Files.Clear();
                viewModel.GetFilesAndNames();
                viewModel.GetTags();
                int numberOfRecFiles = await viewModel.GetReceivedFilesFromUsers();
                if (numberOfRecFiles > 0)
                {
                    emptyBellImage.IsVisible = false;
                    filledBellImage.IsVisible = true;
                    notificationNumberLabel.Text = numberOfRecFiles.ToString();
                }
                else 
                {
                    emptyBellImage.IsVisible = true;
                    filledBellImage.IsVisible = false;
                    notificationNumberLabel.Text = "0";
                }
                //viewModel.Test();

            }

        }



        private async void OnDownloadClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string fileName)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                await viewModel.DownloadFile(fileName);
                DisplayAlert("Info","Your file has downloaded","OK");
            }

        }


        private async void OnMenuFlyoutItemDownloadClick(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is string fileName)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                await viewModel.DownloadFile(fileName);
                DisplayAlert("Info", "Your file has downloaded", "OK");
            }
        }

        private async void OnMenuFlyoutItemSendClick(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is string fileName)
            {
                var sendPopup = new SendPopup(fileName);

                // Attach an event handler to the Closed event
                sendPopup.Closed += async (s, args) =>
                {
                    // Show the DisplayAlert after the popup is closed
                    await DisplayAlert("Info", "Your file has been sent", "OK");
                };

                this.ShowPopup(sendPopup);
            }
        }

        private async void OnSendClicked(object sender, EventArgs e)
        {
            //string dest = "test@mta";
            if (sender is Button button && button.CommandParameter is string fileName)
            {
                var sendPopup = new SendPopup(fileName);

                // Attach an event handler to the Closed event
                sendPopup.Closed += async (s, args) =>
                {
                    // Show the DisplayAlert after the popup is closed
                    await DisplayAlert("Info", "Your file has been sent", "OK");
                };

                this.ShowPopup(sendPopup);
            }
        }
       
        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string fileName)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                try
                {
                    await viewModel.DeleteFile(fileName);
                    fcView.ItemsSource = viewModel.Files;
                    DisplayAlert("Info", "Your file has been deleted", "OK");

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    DisplayAlert("Error", "Your could not be deleted", "OK");
                }

            }
        }

        private async void OnMenuItemDeleteClicked(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is string fileName)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                try
                {
                    await viewModel.DeleteFile(fileName);
                    DisplayAlert("Info", "Your file has been deleted", "OK");

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    DisplayAlert("Error", "Your could not be deleted", "OK");
                }

            }
        }


        private async void OnActionsButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string fileName)
            {
                var result = await DisplayActionSheet("Press one of the following: ", "Cancel", null, "Download", "Send", "Delete");

                switch (result)
                {
                    case "Download":
                        OnDownloadClicked(sender, e);
                        break;
                    case "Send":
                        OnSendClicked(sender, e);
                        break;
                    case "Delete":
                        OnDeleteClicked(sender, e);
                        break;
                }
            }

        }
        private async void OnAddButtonClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//UploadPage");

        }

        private async void OnFilledBellTapped(object sender, EventArgs e)
        {
            if (BindingContext is MainWindowViewModel viewModel)
            {

                try
                {
                    await this.ShowPopupAsync(new TransferPagePopout());
                   
                    viewModel.Files.Clear();
                    viewModel.GetFilesAndNames();
                    int numberOfRecFiles = await viewModel.GetReceivedFilesFromUsers();
                    if (numberOfRecFiles > 0)
                    {
                        emptyBellImage.IsVisible = false;
                        filledBellImage.IsVisible = true;
                        notificationNumberLabel.Text = numberOfRecFiles.ToString();
                    }
                    else
                    {
                        emptyBellImage.IsVisible = true;
                        filledBellImage.IsVisible = false;
                        notificationNumberLabel.Text = "0";
                    }

                }
                catch (Exception ex)
                {
                    DisplayAlert("Error", "Could not load the request", "OK");
                }
            }

        }

        private async void OnUploadDateClick(object sender, EventArgs e)
        {
            if (BindingContext is MainWindowViewModel viewModel)
            {
                await viewModel.SortByUploadDate();
                fcView.ItemsSource = viewModel.Files;
            }
        }

        private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BindingContext is MainWindowViewModel viewModel)
            {
                viewModel.FilterFiles();
                if(fcView != null)
                    fcView.ItemsSource = viewModel.FilteredFiles;
            }
        }
    }

}
