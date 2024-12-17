namespace SampleMauiMvvmApp.Views;

public partial class ReflushPage : ContentPage
{
	public ReadingViewModel viewModel;
	public ReflushPage(ReadingViewModel _ViewModel)
	{
		InitializeComponent();
        viewModel = _ViewModel;
		this.BindingContext = viewModel;
	}
}