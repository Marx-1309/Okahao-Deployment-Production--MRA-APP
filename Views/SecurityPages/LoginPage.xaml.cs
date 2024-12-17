using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views.SecurityPages;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;
	}

    protected override bool OnBackButtonPressed()
    {
        return true;
    }

}