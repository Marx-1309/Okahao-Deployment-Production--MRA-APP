
namespace SampleMauiMvvmApp.Push_Notifications
{
    public class LocalNotifications
    {
        public LocalNotifications()
        {
            LocalNotificationCenter.Current.NotificationActionTapped += Current_NotificationActionTapped;
        }

        private async void Current_NotificationActionTapped(Plugin.LocalNotification.EventArgs.NotificationActionEventArgs e)
        {
            if (e.IsDismissed)
            {
                return;
            }
            else if (e.IsTapped)
            {
                await Shell.Current.DisplayAlert("Good Morning", "Hello", "Ok");
            }
        }

        private void onNewReadingExportFound()
        {
            string Month = DateTime.Now.ToLongDateString();

            var Mnth = Month.Split(',')[1].Split().ToString();

            string currentMonth = string.Empty;

            //Notification request params 
            
        }
    }

    public class ServerNotifications
    {
    }
}
