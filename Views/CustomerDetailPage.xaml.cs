//using Plugin.Media;
//using Plugin.Media.Abstractions;
using SampleMauiMvvmApp.Fakers;
using SampleMauiMvvmApp.Models;
using SampleMauiMvvmApp.ModelWrappers;
using SampleMauiMvvmApp.ViewModels;
using System.IO.Compression;

namespace SampleMauiMvvmApp.Views;

//[QueryProperty("CustomerId", "id")]
public partial class CustomerDetailPage : ContentPage
{
    //public Customer CustomerId { get; set; }
    private CustomerDetailViewModel _viewModel;
    public CustomerDetailPage(CustomerDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        ReadingFaker readingFaker = new();
        _viewModel.VmReading = new ReadingWrapper(readingFaker.Generate());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.CustDisplayDetailsCommand.Execute(null);
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

    //private async void OnTakePhotoClicked(object sender, EventArgs e)
    //{
    //    var options = new StoreCameraMediaOptions { CompressionQuality = selectedCompressionQuality };
    //    var result = await CrossMedia.Current.TakePhotoAsync(options);

    //    if (result is null) return;



    //    var fileInfo = new FileInfo(result?.Path);
    //    var fileLength = fileInfo.Length;



    //    // Convert the image to Base64 string
    //    byte[] imageData = File.ReadAllBytes(result?.Path);
    //    string base64Image = Convert.ToBase64String(imageData);
    //}


}