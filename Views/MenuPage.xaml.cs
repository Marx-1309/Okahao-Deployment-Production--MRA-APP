using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views;

public partial class MenuPage : ContentPage
{
    public MenuPage()
    {
        InitializeComponent();
        BindingContext = new MenuViewModel();
    }

    //private async void ImageButton_Clicked(object sender, EventArgs e)
    //{
    //    await Shell.Current.DisplayAlert("Hello","From Menu Page","OK");
    //}
}