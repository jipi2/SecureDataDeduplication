using DesktopApp.ViewModels;
using DesktopApp.Views;
using Mopups.Services;
using System.Diagnostics;

namespace DesktopApp
{ 
	public partial class UploadPage : ContentPage
	{
		private UploadPageViewModel _viewModel;
        public string path = "";
        public UploadPage()
		{
			InitializeComponent();
			BindingContext = _viewModel = new UploadPageViewModel();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();
            fileNameFrame.IsVisible = false;
            uploadButton.IsVisible = false;
            uploadText.IsVisible = false;
			plusButton.IsVisible = true;

            path = "";
        }

        private async void OnSelectFileClicked(object sender, EventArgs e)
        {
			try
			{
				await _viewModel.SelectFile();

                var popup = new FolderViewPopup();
                await MopupService.Instance.PushAsync(popup);
                var result = await popup.PopupDismissedTask;
                if (result == "")
                {
                    await DisplayAlert("Info", "You need to select a folder to upload the file", "OK");
                    return;
                }
                path = result;
                fileNameFrame.IsVisible = true;
				uploadButton.IsVisible = true;
				uploadText.IsVisible = true;
				plusButton.IsVisible = false;
            }
			catch (Exception ex) 
			{
				DisplayAlert("Error", ex.Message, "OK");
			}
        }

        private async void OnUploadFileClicked(object sender, EventArgs e)
        {
			try
			{

                uploadButton.IsEnabled = false;
				//await DisplayAlert("Info", "Your file is being encrypted", "OK");

				mainBorder.IsVisible = false;
				loadingBorder.IsVisible = true;
                await _viewModel.EncryptFile();

				loadingBorder.IsVisible = false;
				progressBarBorder.IsVisible = true;
                await _viewModel.UploadFile(path);

                progressBarBorder.IsVisible = false;
                mainBorder.IsVisible = true;

                await DisplayAlert("Info", "Your file has been uploaded", "OK");
                uploadButton.IsEnabled = true;
                fileNameFrame.IsVisible = false;
                uploadButton.IsVisible = false;
                uploadText.IsVisible = false;
                plusButton.IsVisible = true;

            }
			catch (Exception ex)
			{
				uploadButton.IsEnabled = true;
				DisplayAlert("Error", ex.Message, "OK");
                uploadButton.IsEnabled = true;
                fileNameFrame.IsVisible = false;
                uploadButton.IsVisible = false;
                uploadText.IsVisible = false;
                plusButton.IsVisible = true;
                progressBarBorder.IsVisible = false;
            }
		}



    }
}