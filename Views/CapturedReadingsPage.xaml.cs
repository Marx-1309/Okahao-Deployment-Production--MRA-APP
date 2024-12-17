using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views;

public partial class CapturedReadingsPage : ContentPage
{
    ReadingViewModel _viewModel;
    public CapturedReadingsPage(ReadingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        this.BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.GetCapturedReadingsCommand.Execute(null);
    }
}