using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views;

public partial class MonthPage : ContentPage
{
    MonthViewModel _viewModel;

    public MonthPage(MonthViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel;
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.GetMonthsCommand.Execute(null);

    }

}