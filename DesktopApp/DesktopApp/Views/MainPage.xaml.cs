using DesktopApp.ViewModels;
using Microsoft.Maui.Storage;
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

            // Call your function here
            if (BindingContext is MainWindowViewModel viewModel)
            {
                //viewModel.GetFilesAndNames();
                viewModel.Test();
            }
        }
        private void OnCounterClicked(object sender, EventArgs e)
        {

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
        private async void OnSendClicked(object sender, EventArgs e)
        {
            string dest = "test@mta";
            if (sender is Button button && button.CommandParameter is string fileName)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                await viewModel.SendFile(fileName, dest);
                DisplayAlert("Info", "Your file sent", "OK");
            }
        }
       
        private void OnDeleteClicked(object sender, EventArgs e)
        {

        }

        private async void OnThreeDotsTapped(object sender, EventArgs e)
        {
            
        }

        private void FilesListView_Scrolled(object sender, ScrolledEventArgs e)
        {

        }

        private async void OnActionsButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string fileName)
            {
                var result = await DisplayActionSheet("Press one of the following: ", "Cancel", null, "Download", "Send", "Delete");

                switch (result)
                {
                    case "Download":
                        Debug.WriteLine(fileName);
                        break;
                    case "Send":
                        Debug.WriteLine("Optiune 2 clicked");
                        break;
                    case "Delete":
                        Debug.WriteLine("Optiune 3 clicked");
                        break;
                }
            }

        }
 
    }

}
