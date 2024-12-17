
using SampleMauiMvvmApp.Helpers;

namespace SampleMauiMvvmApp.ViewModels
{
    [QueryProperty(nameof(IsFirstTime), nameof(IsFirstTime))]
    public partial class LoginViewModel : BaseViewModel
    {
        AppShell _appshell;
        public LoginViewModel(AuthenticationService _authenticationService,AppShell appShell )
        {
            this.authenticationService = _authenticationService; 
            this._appshell = appShell;
        }
        [ObservableProperty]
        private bool _isFirstTime;

        [ObservableProperty]
        string username;

        [ObservableProperty]
        string password;

        private AuthenticationService authenticationService;

        [RelayCommand]

        async Task Login()
        {
            if(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayLoginMessage("Invalid Login Attempt");
            }
            else
            {
                IsBusy = true;
                await Task.Delay(100);
                // Call API to attempt a login
                var loginModel = new LoginModel(username, password);
                var response = await authenticationService.Login(loginModel);

                //Display welcome message
                await DisplayLoginMessage(authenticationService.StatusMessage);
                //await Shell.Current.DisplayAlert("Success", "Login was successful", "OK");

                if (!string.IsNullOrEmpty(response.Token)) 
                {
                    MauiProgram.CreateMauiApp();
                    //Store token in secure 
                    await SecureStorage.SetAsync("Token",response.Token);


                    //Build a menu on the fly...based on the role
                    var jsonToken = new JwtSecurityTokenHandler().ReadToken(response.Token) as
                        JwtSecurityToken;

                    string userId = jsonToken.Claims.First(claim => claim.Type == "sub")?.Value;
                    string email = jsonToken.Claims.First(claim => claim.Type == "email")?.Value;
                    string userSite = jsonToken.Claims.First(claim => claim.Type == "UserSite")?.Value.Trim();
                    // Set a string value:
                    Preferences.Default.Set("userId", userId);
                    Preferences.Default.Set("username", email);
                    Preferences.Default.Set("userSite", userSite);

                    //You can use this to access details of the logged_in user//commented out 

                    //Navigate to the app's main page
                    IsBusy = false;
                    await Shell.Current.GoToAsync($"//{nameof(MonthCustomerTabPage)}");;
                    
                }
                IsBusy = false;
                

            }
        }



        async Task DisplayLoginMessage(string message) 
        {
            await Shell.Current.DisplayAlert("Attempt Result", message, "OK");
            Password = string.Empty;
        }

    }
}
