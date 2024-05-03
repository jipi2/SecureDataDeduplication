using CommunityToolkit.Maui.Views;
using DesktopApp.Dto;
using DesktopApp.Models;
using DesktopApp.ViewModels;
using Microsoft.Maui.Storage;
using Mopups.Services;
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
            try
            {
                mainScrollView.IsVisible = false;
                loadingStackLayout.IsVisible = true;
                if (BindingContext is MainWindowViewModel viewModel)
                {
                    viewModel.Files.Clear();
                    viewModel.Labels.Clear();
                    viewModel.FilteredFiles.Clear();
                    await viewModel.GetFilesAndNames();
                    int numberOfLables = await viewModel.GetLabelFileNames();
                    //int numberOfLables = viewModel.GetTags();
                    if (numberOfLables <= 0)
                        labelCollectionView.WidthRequest = 650;
                    else
                        labelCollectionView.WidthRequest = -1;

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
                    mainScrollView.IsVisible = true;
                    loadingStackLayout.IsVisible = false;
   
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Your session expired!")
                {
                    await DisplayAlert("Error", "Your session expired!", "OK");
                    await Shell.Current.GoToAsync("//SignInPage");
                }
            }

        }



        private async void OnDownloadClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string fileName)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                if (viewModel != null)
                {
                    DisplayAlert("Info", "Your download has started. You will be notified upon completion.", "OK");
                    string dnFolder = await viewModel.DownloadFile(fileName);
                    await DisplayAlert("Info", fileName+" has downloaded in folder: "+dnFolder, "OK");
                }
            }

        }


        private async void OnMenuFlyoutItemDownloadClick(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is string fileName)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                if (viewModel != null)
                {
                    DisplayAlert("Info", "Your download has started. You will be notified upon completion.", "OK");
                    string dnFolder = await viewModel.DownloadFile(fileName);
                    await DisplayAlert("Info", fileName + " has been downloaded in folder: " + dnFolder, "OK");
                }
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
            try
            {

                if (sender is Button button && button.CommandParameter is string fileName)
                {
                    var sendPopup = new SendPopup(fileName);


                    sendPopup.Closed += async (s, args) =>
                    {

                        await DisplayAlert("Info", "Your file has been sent", "OK");
                    };

                    this.ShowPopup(sendPopup);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                await DisplayAlert("Error", "Your file could not be sent", "OK");
            }
        }
        

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string fileName)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                if (viewModel != null)
                {
                    bool result = await DisplayAlert("Info", "Are you sure do you want to delete this file", "YES", "NO");
                    if (result)
                    {
                        try
                        {
                            await viewModel.DeleteFile(fileName);
                            fcView.ItemsSource = viewModel.Files;
                            await DisplayAlert("Info", "Your file has been deleted", "OK");

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            await DisplayAlert("Error", "Your could not be deleted", "OK");

                        }
                    }
                }

            }
        }

        private async void OnMenuItemDeleteClicked(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is string fileName)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                if (viewModel != null)
                {
                    bool result = await DisplayAlert("Info", "Are you sure do you want to delete this file", "YES", "NO");
                    if (result)
                    {
                        try
                        {
                            await viewModel.DeleteFile(fileName);
                            await DisplayAlert("Info", "Your file has been deleted", "OK");

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            await DisplayAlert("Error", "Your could not be deleted", "OK");
                        }
                    }
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
                    await viewModel.GetFilesAndNames();
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
                   await DisplayAlert("Error", "Could not load the request", "OK");
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
                viewModel.FilterFilesBySearch();
                if (fcView != null)
                    fcView.ItemsSource = viewModel.FilteredFiles;
            }
        }
        private async void OnAddLabelClicked(object sender, EventArgs e)
        {
            if (BindingContext is MainWindowViewModel viewModel)
            {
                try
                {
                    await this.ShowPopupAsync(new CreateLabelPage());
                    await viewModel.GetLabelFileNames();
                    labelCollectionView.ItemsSource = viewModel.Labels;
                    if (viewModel.Labels.Count > 0)
                    {
                        labelCollectionView.WidthRequest = -1;
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", "Could not load the request", "OK");
                }
            }
        }


        private async void OnMenuItemAddLabelClicked(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is string fileName)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                if (viewModel != null)
                {
                    List<string> candidateLabels = await viewModel.GetCandidateLabels(fileName);
                    await this.ShowPopupAsync(new AddLabelToFilePopup(fileName, candidateLabels));
                    int numberOfLables = await viewModel.GetLabelFileNames();
                    if (numberOfLables <= 0)
                        labelCollectionView.WidthRequest = 650;
                    else
                        labelCollectionView.WidthRequest = -1;
                }
            }
        }
        private async void OnMenuItemRemoveLabelClicked(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is string fileName)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                if (viewModel != null)
                {
                    List<string> candidateLabels = await viewModel.GetCandidateLabelsForRemoving(fileName);
                    await this.ShowPopupAsync(new RemoveLabelFilePopup(fileName, candidateLabels));
                    int numberOfLables = await viewModel.GetLabelFileNames();
                    if (numberOfLables <= 0)
                        labelCollectionView.WidthRequest = 650;
                    else
                        labelCollectionView.WidthRequest = -1;
                }
            }
        }

        private async void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is LabelModel label)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                if (viewModel != null)
                {
                    if (label.isChecked == true)
                    {
                        label.isChecked = false;
                        await viewModel.RemoveLabelFromFiltering(label.labelName);
                        if (viewModel.FilteredFiles.Count == 0)
                        {
                            fcView.ItemsSource = viewModel.Files;
                        }
                        else
                        {
                            fcView.ItemsSource = viewModel.FilteredFiles;
                        }
                    }
                    else
                    {
                        label.isChecked = true;
                        await viewModel.FilterFilesByLabel(label.labelName);
                        fcView.ItemsSource = viewModel.FilteredFiles;

                    }
                }
            }
        }

        private async void OnLabelMenuDeleteClicked(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is string labelName)
            {
                bool result = await DisplayAlert("Info", "Are you sure do you want to delete the label", "YES", "NO");
                if (result)
                {
                    var viewModel = BindingContext as MainWindowViewModel;
                    if (viewModel != null)
                    {
                        await viewModel.DeleteLabel(labelName);
                        await viewModel.GetLabelFileNames();
                        labelCollectionView.ItemsSource = viewModel.Labels;
                        if (viewModel.Labels.Count > 0)
                        {
                            labelCollectionView.WidthRequest = -1;
                        }
                        else
                        {
                            labelCollectionView.WidthRequest = 650;
                        }
                    }
                }
            }
        }

        private void profileIconButton_Clicked(object sender, EventArgs e)
        {
            MopupService.Instance.PushAsync(new ProfilePopup());
        }

        private async void OnSizeButtonClicked(object sender, EventArgs e)
        {
            if (BindingContext is MainWindowViewModel viewModel)
            {
                await viewModel.SortBySize();
                fcView.ItemsSource = viewModel.Files;
            }
        }
    }
}


