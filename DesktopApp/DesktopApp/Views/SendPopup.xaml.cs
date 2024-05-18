
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using DesktopApp.ViewModels;


namespace DesktopApp
{
	public partial class SendPopup 
	{
		private string _fileName;
		private string _fullPath;
		public SendPopup(string fileName, string fullPath)
		{
			InitializeComponent();
			_fileName = fileName;
            _fullPath = fullPath;
        }

		private async void OnSendButtonClicked(object sender, EventArgs e)
		{
            if (sender is Button button && BindingContext is MainWindowViewModel viewModel)
			{
                try
				{
					string destEmail = destEmailTxt.Text;
					if(destEmail == null || destEmail == "")
						throw new Exception("Destination email is empty");

					mainFrame.IsVisible = false;
					loadingFrame.IsVisible = true;
					await viewModel.SendFile(_fullPath ,_fileName, destEmail);
                    await Application.Current.MainPage.DisplayAlert("Info", "Your file has been sent!", "OK");

                    this.Close();
                }
                catch (Exception ex)
				{
					await Application.Current.MainPage.DisplayAlert("Error", "The file could not be sent", "OK");
                    this.Close();
                    
                }
            }
        }
	}
}