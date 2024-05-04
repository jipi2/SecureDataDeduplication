using DesktopApp.ViewModels;

namespace DesktopApp
{

	public partial class CodePage 
	{
		private string _email;
        private string _password;
        private bool _fromRegisterPage = true;
        public CodePage(string email, string password, bool fromRegisterPage)
		{
			InitializeComponent();
            _email = email;
            _password = password;
            _fromRegisterPage = fromRegisterPage;
        }

        private async void sendButton_Clicked(object sender, EventArgs e)
        {
            mainFrame.IsVisible = false;
            loadingFrame.IsVisible = true;
            BindingContext = new VerificationCodeViewModel();
            if (BindingContext != null && BindingContext is VerificationCodeViewModel viewModel)
            {
                try
                {
                    string code = codeTxt.Text;
                    await viewModel.VerifyCode(_email, _password, code, _fromRegisterPage);
                    await Application.Current.MainPage.DisplayAlert("Info", "Your email has been verified", "OK");

                    await CloseAsync(true);

                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                    await CloseAsync(false);
                }
            }
        }
        private async void cancelButton_Clicked(object sender, EventArgs e)
        {
            await CloseAsync(false);
        }
    }
}