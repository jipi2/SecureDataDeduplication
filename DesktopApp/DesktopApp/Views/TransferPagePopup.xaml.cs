using CommunityToolkit.Maui.Views;
using DesktopApp.Dto;
using DesktopApp.ViewModels;
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
                    await viewModel.AcceptReceivedFile(item);
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
                    await viewModel.RemoveReceivedFile(item);
                    //sincronizare cu listaaaa
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