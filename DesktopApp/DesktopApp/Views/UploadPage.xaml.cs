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
            //uploadButton.IsVisible = false;
            //uploadText.IsVisible = false;
            uploadGrid.IsVisible = false;
            plusButton.IsVisible = true;

            path = "";
        }

        private async void OnSelectFileClicked(object sender, EventArgs e)
        {
			try
			{
				await _viewModel.SelectFile();
                int numberOfFiles = _viewModel.GetNumberOfFilesSelected();
                if (numberOfFiles > 0)
                {
                    var popup = new FolderViewPopup();
                    await MopupService.Instance.PushAsync(popup);
                    var result = await popup.PopupDismissedTask;
                    if (result == "")
                    {
                        await DisplayAlert("Info", "You need to select a folder to upload the file", "OK");
                        return;
                    }
                    path = result;
                    uploadGrid.IsVisible = true;
                    fileNameFrame.IsVisible = true;
                    //uploadButton.IsVisible = true;
                    //uploadText.IsVisible = true;
                    plusButton.IsVisible = false;
                }
                
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
                int numberOfFiles = _viewModel.GetNumberOfFilesSelected();
                for(int i=0;i<numberOfFiles; i++)
                { 
                    //uploadFileName.Text = _viewModel.GetFileModelAt(i).fileName;
                    uploadButton.IsEnabled = false;
                    //await DisplayAlert("Info", "Your file is being encrypted", "OK");
                    uploadGrid.IsVisible = false;
                    fileNameFrame.IsVisible = false;

                    mainBorder.IsVisible = false;
                    loadingBorder.IsVisible = true;
                    await _viewModel.EncryptFile(_viewModel.GetFileModelAt(i), _viewModel.GetHashTaskAt(i));

                    loadingBorder.IsVisible = false;
                    progressBarBorder.IsVisible = true;
                    await _viewModel.UploadFile(path, _viewModel.GetFileModelAt(i));

                    progressBarBorder.IsVisible = false;
                    mainBorder.IsVisible = true;
                    uploadButton.IsEnabled = true;

                    //uploadButton.IsVisible = false;
                    //uploadText.IsVisible = false;
                    plusButton.IsVisible = true;
                }
                if(numberOfFiles > 1)
                    await DisplayAlert("Info", "Your files has been uploaded", "Ok");
                else
                    await DisplayAlert("Info", "Your file has been uploaded", "Ok");

                _viewModel.ClearFiles();

            }
			catch (Exception ex)
			{
				DisplayAlert("Error","Something went wrong, please try again later.", "OK");
                uploadButton.IsEnabled = false;
                fileNameFrame.IsVisible = false;
                //uploadButton.IsVisible = false;
                uploadGrid.IsVisible = false;
                //uploadText.IsVisible = false;
                mainBorder.IsVisible = true;
                plusButton.IsVisible = true;
                progressBarBorder.IsVisible = false;
                _viewModel.ClearFiles();
            }
		}

        private async void mainPageButtonArrow_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        private async void cancelButton_Clicked(object sender, EventArgs e)
        {
            uploadGrid.IsVisible = false;
            fileNameFrame.IsVisible = false;
            plusButton.IsVisible = true;
            await _viewModel.CancelBeforeUploading();
            _viewModel.ClearFiles();
        }
    }
}