using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views;

public partial class LoadingPage : ContentPage
{
    LoadingViewModel _viewModel;
    public LoadingPage(LoadingViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel;
		this.BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.GetInitializationDataCommand.Execute(null);
    }
}