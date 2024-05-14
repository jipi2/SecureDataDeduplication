using CommunityToolkit.Maui.Views;
using DesktopApp.PopupResponse;
using DesktopApp.ViewModels;
using Mopups.Pages;
using Mopups.Services;
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
}