using CommunityToolkit.Maui.Views;
using DesktopApp.PopupResponse;
using DesktopApp.ViewModels;
using FileStorageApp.Shared.Dto;
using Mopups.Pages;
using Mopups.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DesktopApp.Views;

public partial class FolderViewPopup 
{
    TaskCompletionSource<string> _taskCompletionSource;
    public Task<string> PopupDismissedTask => _taskCompletionSource.Task;
    public string FolderName { get; set; }
    public FolderViewPopup()
	{
		InitializeComponent();
        FolderName = "";
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        _taskCompletionSource = new TaskCompletionSource<string>();
        if (BindingContext is FolderPopupViewModel viewModel)
        {
            viewModel.GetFolders();
        }
    }

    private async void selectButton_Clicked(object sender, EventArgs e)
    {
        if (BindingContext is FolderPopupViewModel viewModel)
        {
            FolderName = viewModel.SelectedItem.FullPathName;
            _taskCompletionSource.SetResult(FolderName);
            await MopupService.Instance.PopAsync();
        }
    }

    private async void cancelButton_Clicked(object sender, EventArgs e)
    {
        FolderName = "";
        _taskCompletionSource.SetResult(FolderName);
        await MopupService.Instance.PopAsync();
    }

    private async void newFolder_Clicked(object sender, EventArgs e)
    {
        if (sender is MenuFlyoutItem mf && mf.CommandParameter is FolderHierarchy parentFolder)
        {
            var viewModel = BindingContext as FolderPopupViewModel;

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
                        await DisplayAlert("Succes", "The folder has been created!", "Ok");
                        viewModel.GetFolders();
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", ex.Message, "Ok");
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