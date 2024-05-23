using DesktopApp.ViewModels;
using Mopups.Services;
using System.Diagnostics;

namespace DesktopApp.Views;

public partial class FileViewPopup
{
    private Models.File _file;
    private CancellationTokenSource _cancellationTokenSource;
    public FileViewPopup(Models.File file)
	{
        _file = file;
        InitializeComponent();
	}

    private bool isFileToBig()
    {
        if (_file.fileSize > 52428800)
        {
            return false;
        }
        return true;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        fileBtn.IsVisible = true;
        fileNameText.Text = _file.fileName;
        if (isFileToBig() == false)
        {
            await DisplayAlert("Information", "The file size exceeds the limit for in-app viewing. Please download the file to view it.", "Ok");
            await MopupService.Instance.PopAsync();
        }

    }



    private async void fileBtn_Clicked(object sender, EventArgs e)
    {
        fileBtn.IsVisible = false;
        loadingBar.IsVisible = true;
        _cancellationTokenSource = new CancellationTokenSource();
        if (BindingContext is MainWindowViewModel viewModel)
        {
            await viewModel.DownloadFile(_file.fullPath, _file.fileName, false, _cancellationTokenSource.Token);
            
            loadingBar.IsVisible = false;
            fileView.IsVisible = true;
            fileView.Source = _file.fileName;
        }
    }

    private async void exitButton_Clicked(object sender, EventArgs e)
    {
        _cancellationTokenSource?.Cancel();

        try
        {

            FileStream? f = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _file.fileName));
            if (f != null)
            {
                f.Close();
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _file.fileName));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        finally
        {
            await MopupService.Instance.PopAsync();
        }

    }
}