
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
                var loginModel = new LoginModel(username, password);
                var response = await authenticationService.Login(loginModel);

                await DisplayLoginMessage(authenticationService.StatusMessage);
                //await Shell.Current.DisplayAlert("Success", "Login was successful", "OK");

                if (!string.IsNullOrEmpty(response.Token)) 
                {
                    MauiProgram.CreateMauiApp();

                    await SecureStorage.SetAsync("Token",response.Token);


                    //Build a menu on the fly...based on the role
                    var jsonToken = new JwtSecurityTokenHandler().ReadToken(response.Token) as
                        JwtSecurityToken;

                    string userId = jsonToken.Claims.First(claim => claim.Type == "sub")?.Value;
                    string email = jsonToken.Claims.First(claim => claim.Type == "email")?.Value;
                    string userSite = jsonToken.Claims.First(claim => claim.Type == "UserSite")?.Value.Trim();

                    Preferences.Default.Set("userId", userId);
                    Preferences.Default.Set("username", email);
                    Preferences.Default.Set("userSite", userSite);

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
