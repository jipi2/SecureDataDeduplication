using DesktopApp.ViewModels;

namespace DesktopApp
{

	public partial class CreateLabelPage
	{
		public CreateLabelPage()
		{
			InitializeComponent();
		}

        private async void addLabelBtn_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && BindingContext is MainWindowViewModel viewModel)
			{
				try
				{
					string labelName = labelNameText.Text;
                    if (labelName == null || labelName == "")
                        throw new Exception("Label name is empty");

                    await viewModel.CreateLabel(labelName);
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