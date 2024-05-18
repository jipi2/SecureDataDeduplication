using CommunityToolkit.Maui.Views;
using DesktopApp.Models;
using DesktopApp.ViewModels;
using Mopups.Services;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DesktopApp
{
    public partial class MainPage : ContentPage
    {
        private bool isFirst = true;
        public MainPage()
        {
            InitializeComponent();

            //ShellContent content = AppShell.Current.FindByName<ShellContent>("SignInShell");
            //content.IsVisible = false;
            //content = AppShell.Current.FindByName<ShellContent>("SignUpShell");
            //content.IsVisible = false;

            fcView.SelectionMode = SelectionMode.Single;
            fcView.SelectionChanged += OnCollectionViewSelectionChanged;
        }

        private async void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection != null && e.CurrentSelection.Any())
            {
                Models.File selectedFile = e.CurrentSelection.FirstOrDefault() as Models.File;

                if (selectedFile.isFolder == true)
                {
                    if (BindingContext is MainWindowViewModel viewModel)
                    {
                        await viewModel.GetFolderFiles(selectedFile.fullPath);
                        backArrow.IsVisible = true;
                    }
                }
            }
            
        }
        protected async override void OnAppearing()
        {
            base.OnAppearing();

            //if (isFirst == true)
            //{
            //    await Shell.Current.GoToAsync("//SignInPage");
            //    isFirst = false;
            //}


            try
            {
                backArrow.IsVisible = false;
                mainScrollView.IsVisible = false;
                loadingStackLayout.IsVisible = true;
                if (BindingContext is MainWindowViewModel viewModel)
                {
                    viewModel.Files.Clear();
                    viewModel.Labels.Clear();
                    viewModel.FilteredFiles.Clear();
                    //await viewModel.GetFilesAndNames();
                    await viewModel.GetFolderWithFilesHierarchy();
                    await viewModel.RenewAllFolders();
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
                    ////viewModel.Test();
                    mainScrollView.IsVisible = true;
                    loadingStackLayout.IsVisible = false;

                    //viewModel.Test();

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
            if (sender is Button button && button.CommandParameter is Models.File f)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                if (viewModel != null)
                {
                    DisplayAlert("Info", "Your download has started. You will be notified upon completion.", "OK");
                    string dnFolder = await viewModel.DownloadFile(f.fullPath, f.fileName);
                    await DisplayAlert("Info", f.fileName+" has downloaded in folder: "+dnFolder, "OK");
                }
            }

        }


        private async void OnMenuFlyoutItemDownloadClick(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is Models.File f)
            {

                if (f.isFolder == true)
                {
                    mf.IsEnabled = false;
                    await DisplayAlert("Info", "Your can not do this operations with a folder", "OK");
                }

                else
                {

                    var viewModel = BindingContext as MainWindowViewModel;
                    if (viewModel != null)
                    {

                        DisplayAlert("Info", "Your download has started. You will be notified upon completion.", "OK");
                        string dnFolder = await viewModel.DownloadFile(f.fullPath, f.fileName);
                        await DisplayAlert("Info", f.fileName + " has been downloaded in folder: " + dnFolder, "OK");
                    }
                }
            }
        }

        private async void OnMenuFlyoutItemSendClick(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is Models.File f)
            {
                if (f.isFolder == true)
                {
                    mf.IsEnabled = false;
                    await DisplayAlert("Info", "Your can not do this operations with a folder", "OK");
                }
                else
                {

                    var sendPopup = new SendPopup(f.fileName, f.fullPath);

                    // Attach an event handler to the Closed event
                    sendPopup.Closed += async (s, args) =>
                    {
                        // Show the DisplayAlert after the popup is closed
                        await DisplayAlert("Info", "Your file has been sent", "OK");
                    };

                    this.ShowPopup(sendPopup);
                }
            }
        }

        private async void OnSendClicked(object sender, EventArgs e)
        {
            try
            {

                if (sender is Button button && button.CommandParameter is Models.File f)
                {
                    var sendPopup = new SendPopup(f.fileName, f.fullPath);


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
            if (sender is Button button && button.CommandParameter is Models.File f)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                if (viewModel != null)
                {
                    bool result = await DisplayAlert("Info", "Are you sure do you want to delete this file", "YES", "NO");
                    if (result)
                    {
                        try
                        {
                            await viewModel.DeleteFile(f.fullPath);
                            await viewModel.RenewFolder(viewModel.GetParentFolderFormPath(f.fullPath));
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
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is Models.File f)
            {

                var viewModel = BindingContext as MainWindowViewModel;
                if (viewModel != null)
                {
                    bool result = await DisplayAlert("Info", "Are you sure do you want to delete this file", "YES", "NO");
                    if (result)
                    {
                        try
                        {
                            await viewModel.DeleteFile(f.fullPath);    
                            await viewModel.RenewFolder(viewModel.GetParentFolderFormPath(f.fullPath));
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

        private async void OnActionsButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Models.File f)
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
                    //await viewModel.GetFilesAndNames();
                    await viewModel.RenewAllFolders();
                    await viewModel.GetFolderWithFilesHierarchy();
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
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is Models.File f)
            {
                if (f.isFolder == true)
                {
                    mf.IsEnabled = false;
                    await DisplayAlert("Info", "Your can not do this operations with a folder", "OK");
                }
                else
                {
                    var viewModel = BindingContext as MainWindowViewModel;
                    if (viewModel != null)
                    {
                        List<string> candidateLabels = await viewModel.GetCandidateLabels(f.fileName);
                        await this.ShowPopupAsync(new AddLabelToFilePopup(f.fileName, candidateLabels));
                        int numberOfLables = await viewModel.GetLabelFileNames();
                        if (numberOfLables <= 0)
                            labelCollectionView.WidthRequest = 650;
                        else
                            labelCollectionView.WidthRequest = -1;
                    }
                }
            }
        }
        private async void OnMenuItemRemoveLabelClicked(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is Models.File f)
            {
                if (f.isFolder == true)
                {
                    mf.IsEnabled = false;
                    await DisplayAlert("Info", "Your can not do this operations with a folder", "OK");
                }
                else
                {
                    var viewModel = BindingContext as MainWindowViewModel;
                    if (viewModel != null)
                    {
                        List<string> candidateLabels = await viewModel.GetCandidateLabelsForRemoving(f.fileName);
                        await this.ShowPopupAsync(new RemoveLabelFilePopup(f.fileName, candidateLabels));
                        int numberOfLables = await viewModel.GetLabelFileNames();
                        if (numberOfLables <= 0)
                            labelCollectionView.WidthRequest = 650;
                        else
                            labelCollectionView.WidthRequest = -1;
                    }
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

        private async void backArrow_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is MainWindowViewModel viewModel)
            {
                if (viewModel.PrevFolder == "/")
                {
                    backArrow.IsVisible = false;
                }
                await viewModel.GetFolderFiles(viewModel.PrevFolder);

            }
        }

        private async void CreateFolder_Clicked(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is SimpleFileHierarchyModel parentFolder)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                if (viewModel != null)
                {
                    string newFolderName = await DisplayPromptAsync("Folder Name", "Enter your new folder name", "Ok", "Cancel", "Folder Name", maxLength: 100, keyboard: Keyboard.Text, initialValue: "");
                    if (newFolderName != null)
                    {
                        var invalidCharsPattern = @"[\/~!@#$%^&*(){}\[\]?;.,\""]";
                        var regex = new Regex(invalidCharsPattern);
                        if (!regex.IsMatch(newFolderName))
                        {
                            try
                            {
                                await viewModel.CreateFolder(parentFolder.FullPathName, newFolderName);
                                await viewModel.RenewAllFolders();
                                await viewModel.GetFolderWithFilesHierarchy();
                                await DisplayAlert("Succes", "The folder has been created!", "Ok"); 
                            }
                            catch (Exception ex)
                            {
                                await DisplayAlert("Error",ex.Message, "Ok");
                            }
                        }
                        else
                        {
                            // Show an alert if the input is invalid
                            await DisplayAlert("Invalid Input", "The folder name contains invalid characters. Please avoid using /~!@#$%^&*(){}?;.,\".", "Ok");
                        }
                    }
                }
            }

        }
        private async void DeleteFolder_Clicked(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem mf && mf.CommandParameter is SimpleFileHierarchyModel folder)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                if (viewModel != null)
                {
  
                    try
                    {
                        bool result = await DisplayAlert("Info", "Are you sure do you want to delete this folder?", "Ok", "Cancel");
                        if (result == true)
                        {
                            await viewModel.DeleteFolder(folder);
                            await viewModel.RenewAllFolders();
                            await viewModel.GetFolderWithFilesHierarchy();
                            await DisplayAlert("Succes", "The folder has been deleted!", "Ok");
                        }
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", ex.Message, "Ok");
                    }
 
                }
            }

        }
    }
}


