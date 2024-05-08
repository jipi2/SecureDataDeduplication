using DesktopApp.ViewModels;
using System.Diagnostics;

namespace DesktopApp
{ 
	public partial class UploadPage : ContentPage
	{
		private UploadPageViewModel _viewModel;
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
        }

        private async void OnSelectFileClicked(object sender, EventArgs e)
        {
			try
			{
				await _viewModel.SelectFile();
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
                await _viewModel.UploadFile();

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
            }
		}



    }
}