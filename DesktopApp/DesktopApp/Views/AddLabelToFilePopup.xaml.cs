using DesktopApp.ViewModels;
using System.Diagnostics;

namespace DesktopApp
{
	public partial class AddLabelToFilePopup
	{
		private string _fileName;
		private List<string> _labels;
        public AddLabelToFilePopup(string fileName, List<string> labels)
		{
			InitializeComponent();
			_fileName = fileName;
            _labels = labels;

           labelsCollectionView.ItemsSource = _labels;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            if(sender is Button button && BindingContext is MainWindowViewModel viewModel)
            {
                try
                {
                    string label = button.Text;
                    await viewModel.AddLabelToFile(_fileName, label);
                    this.Close();
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "An error has occured, please try later!", "OK");
                   // throw new Exception("Error while adding label");
                }
            }
        }
    }
}