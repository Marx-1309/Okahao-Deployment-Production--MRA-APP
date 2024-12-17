namespace SampleMauiMvvmApp;

public partial class AppShell : Shell
{
    public AppShell()
	{
        InitializeComponent();
        //CheckIfValidToken();
        //Routes
        Routing.RegisterRoute(nameof(CustomerDetailPage), typeof(CustomerDetailPage));
        Routing.RegisterRoute(nameof(MonthPage), typeof(MonthPage));
        Routing.RegisterRoute(nameof(ListOfReadingByMonthPage), typeof(ListOfReadingByMonthPage));
        Routing.RegisterRoute(nameof(MonthCustomerTabPage), typeof(MonthCustomerTabPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(LogoutPage), typeof(LogoutPage));
        Routing.RegisterRoute(nameof(OnboardingPage), typeof(OnboardingPage));
        Routing.RegisterRoute(nameof(CapturedReadingsPage), typeof(CapturedReadingsPage));
        Routing.RegisterRoute(nameof(UncapturedReadingsPage), typeof(UncapturedReadingsPage));
        Routing.RegisterRoute(nameof(ReflushPage), typeof(ReflushPage));
        Routing.RegisterRoute(nameof(SynchronizationPage), typeof(SynchronizationPage));
        Routing.RegisterRoute(nameof(OnboardingPage), typeof(OnboardingPage));
        Routing.RegisterRoute(nameof(LocationPage), typeof(LocationPage));
        Routing.RegisterRoute(nameof(UncapturedReadingsByAreaPage), typeof(UncapturedReadingsByAreaPage));
        Routing.RegisterRoute(nameof(NotesListPage), typeof(NotesListPage));
        Routing.RegisterRoute(nameof(NotesDetailsPage), typeof(NotesDetailsPage));
        Routing.RegisterRoute(nameof(SyncNewCustomersPage), typeof(SyncNewCustomersPage));
        Routing.RegisterRoute(nameof(ExceptionReadingListPage), typeof(ExceptionReadingListPage));
        Routing.RegisterRoute(nameof(MenuPage), typeof(MenuPage));
        Routing.RegisterRoute(nameof(AppShell), typeof(AppShell));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CheckIfValidToken();
    }


    //private async Task DisplayLoggedInUserInfo()
    //{
    //    await Task.Delay(1000);
    //    IsBusy = true;
    //    await CheckIfValidToken();
    //            IsBusy = false;
    //}

    private async Task GoToLoginPage()
    {
        await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
    }

    private async Task GoToMainPage()
    {
        await Shell.Current.GoToAsync($"//{nameof(MonthCustomerTabPage)}"); ;
    }

    private async Task GoToSyncPage()
    {
        await Shell.Current.GoToAsync($"{nameof(SynchronizationPage)}", true,
                 new Dictionary<string, object>()
                 {
                    { "Refresh","Refresh"}
                 }); 
    }

    public async Task CheckIfValidToken()
    {
        await Task.Delay(50);
        IsBusy = true; 
        //Retrieve Token from internal Secure Storage
        var token = await SecureStorage.GetAsync("Token");
      

        if (string.IsNullOrEmpty(token))
        {
            IsBusy = false;
            await GoToLoginPage();
        }
        else
        {
            //lblUsername.Text = Preferences.Default.Get("username", "Not logged in");
            //lblRole.Text = "Meter Reader" ?? "Meter Reader";

            var jsonToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;

            if (jsonToken.ValidTo < DateTime.UtcNow)
            {
                SecureStorage.Remove("Token");
                Preferences.Default.Clear();
                await GoToLoginPage();
            }
            else
            {
                string userId = jsonToken.Claims.First(claim => claim.Type == "sub").Value;
                string email = jsonToken.Claims.First(claim => claim.Type == "email").Value;

                string userSite = Preferences.Default.Get("userSite", "");

                lblUsername.Text = email;
                lblUserSite.Text = userSite;
                IsBusy = false;
                //await GoToMainPage();
            }
        }
    }

    private void BtnFlush_Clicked(object sender, EventArgs e)
    {
        GoToSyncPage();
    }
}
