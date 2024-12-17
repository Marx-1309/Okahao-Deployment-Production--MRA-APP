using CommunityToolkit.Mvvm.ComponentModel;
using SampleMauiMvvmApp.Models;
using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views;

public partial class ListOfReadingByMonthPage : ContentPage
{
    //public Customer CustomerId { get; set; }
    public ListOfReadingByMonthPage(MonthViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;
	}

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

    }



}