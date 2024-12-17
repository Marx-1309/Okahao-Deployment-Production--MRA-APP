namespace SampleMauiMvvmApp.Controls;

public partial class FlyOutHeader : StackLayout
{
	public FlyOutHeader()
	{
		InitializeComponent();
		SetValues();
	}

	private void SetValues()
	{
		if(App.UserInfo != null)
		{
			lblUserName.Text = App.UserInfo.Username;
			
		}
	}
}