using DesktopApp.ViewModels;

namespace DesktopApp
{
	public partial class RemoveLabelFilePopup
	{
        private string _fileName;
        private List<string> _labels;

        public RemoveLabelFilePopup(string fileName, List<string> labels)
		{
			InitializeComponent();
            _fileName = fileName;
            _labels = labels;

            labelsCollectionView.ItemsSource = _labels;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && BindingContext is MainWindowViewModel viewModel)
            {
                try
                {
                    string label = button.Text;
                    await viewModel.RemoveLabelFile(_fileName, label);
                    this.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error while adding label");
                }
            }
        }
    }
}