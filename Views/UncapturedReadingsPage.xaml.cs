namespace SampleMauiMvvmApp.Views;

public partial class UncapturedReadingsPage : ContentPage
{
    ReadingViewModel _viewModel;
    public UncapturedReadingsPage(ReadingViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel;
        this.BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.GetUncapturedReadingsCommand.Execute(null);
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}