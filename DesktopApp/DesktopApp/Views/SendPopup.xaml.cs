

using CommunityToolkit.Maui.Views;
using DesktopApp.ViewModels;

namespace DesktopApp
{
	public partial class SendPopup
	{
		private string _fileName;
		public SendPopup(string fileName)
		{
			InitializeComponent();
			_fileName = fileName;
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

                    await viewModel.SendFile(_fileName, destEmail);

					this.Close();
                }
                catch (Exception ex)
				{
					this.Close();
                    throw new Exception("Error while sending file");
                }
            }
        }
	}
}