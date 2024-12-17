namespace SampleMauiMvvmApp.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        public BaseViewModel()
        {

        }

        [ObservableProperty]
        string title;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        bool isBusy;

        public bool IsNotBusy => !IsBusy;
         
        [ObservableProperty]
        string statusMessage;


        string tstMsg = "You Can Proceed Using The App! ";

        public void DisplayToast(string tstMsg)
        {
            Toast.Make(tstMsg, CommunityToolkit.Maui.Core.ToastDuration.Long, 10).Show();
        }
        
    }
}
