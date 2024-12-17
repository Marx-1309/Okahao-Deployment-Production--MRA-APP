using SampleMauiMvvmApp.Services;
using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views;

[QueryProperty("Refresh", "Refresh")]
public partial class SynchronizationPage : ContentPage
{
    ReadingViewModel viewModel;
	public SynchronizationPage(ReadingViewModel _viewModel)
	{
		InitializeComponent();
        viewModel = _viewModel;
        BindingContext = _viewModel;
	}
}