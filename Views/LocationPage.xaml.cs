namespace SampleMauiMvvmApp.Views;
public partial class LocationPage : ContentPage
{
    ReadingViewModel _viewModel;

    public LocationPage(ReadingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.GetLocationsCommand.Execute(null);

    }

}