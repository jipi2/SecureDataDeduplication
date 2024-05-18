using CommunityToolkit.Maui.Views;
using DesktopApp.Dto;
using DesktopApp.ViewModels;
using DesktopApp.Views;
using Mopups.Services;
using System.Diagnostics;

namespace DesktopApp
{
	public partial class TransferPagePopout 
	{
        public TransferPagePopout()
		{
			InitializeComponent();
		}
        private async void OnAcceptButtonClicked(object sender, EventArgs e)
		{
            if (sender is Button button && button.CommandParameter is RecievedFilesDto item && BindingContext is MainWindowViewModel viewModel)
            {
                try
                {
                    loadingFrame.IsVisible = true;
                    var popup = new FolderViewPopup();
                    await MopupService.Instance.PushAsync(popup);
                    var fullpath = await popup.PopupDismissedTask;
                    if (fullpath == "")
                    {
                        await Application.Current.MainPage.DisplayAlert("Info", "You need to select a folder to upload the file", "OK");
                        return;
                    }
                    await viewModel.AcceptReceivedFile(item, fullpath);
                    loadingFrame.IsVisible = false;
                    filesCollectionView.ItemsSource = viewModel.RecFiles;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error while accepting file");
                }
            }
        }
        private async void OnDeclineButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is RecievedFilesDto item && BindingContext is MainWindowViewModel viewModel)
            {
                try
                {

                    loadingFrame.IsVisible = true;
                    await viewModel.RemoveReceivedFile(item);
                    //sincronizare cu listaaaa
                    loadingFrame.IsVisible = false;
                    filesCollectionView.ItemsSource = viewModel.RecFiles;
                    
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

    }
}