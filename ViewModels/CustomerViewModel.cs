namespace SampleMauiMvvmApp.ViewModels
{
    public partial class CustomerViewModel : BaseViewModel
    {
        // If properties of Customer model do not implement [ObservableProperty]
        // Then ObservableCollection<Customer> will not detect any changes of Customer's properties and UI will not be updated
        // Therefore, CustomerWrapper with ObservableObject and ObservableProperty is needed to facilitate the UI update
        //public ObservableCollection<Customer> Customers { get; set; } = new();


        //This collection stores customer objects from  Sqlite straight. 
        public static List<Customer> CustomersListForSearch { get; private set; } = new List<Customer>();

        //This collection stores customer objects from Sql Server , retrieved through Sqlite.
        public ObservableCollection<Customer> SqlCustomers { get; set; } = new();
        public BaseService baseService;
        readonly CustomerService _customerService;
        IGeolocation geolocation;

        public CustomerViewModel(CustomerService customerService, IGeolocation geolocation)
        {
            Title = "Customer Page";
            _customerService = customerService;
            this.geolocation = geolocation;
        }

        [ObservableProperty]
        public bool isRefreshing;

        [RelayCommand]
        public async Task GetCustomersAsync()
        {
            IsBusy = true;

            try
            {
                if (SqlCustomers != null && SqlCustomers.Count > 0)
                {
                    //CustomersListForSearch.Clear();
                    //CustomersListForSearch.AddRange(SqlCustomers);
                }
                else
                {
                    var customers = await _customerService.GetAllCustomers();

                    if (customers == null)
                    {
                        await Shell.Current.DisplayAlert("Error", _customerService.StatusMessage, "OK");
                    }
                    else
                    {
                        foreach(var customer in customers)
                        {
                            SqlCustomers.Add(customer);
                        }

                        CustomersListForSearch.Clear();
                        CustomersListForSearch.AddRange(customers);
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsRefreshing = false;
                IsBusy = false;
            }
        }




        [RelayCommand]
        public async Task GoToDetailsAsync(string customerId)
        {
            if (customerId == null) return;

            var customer = await _customerService.GetCustomerDetails(customerId);
            if (customer == null)
            {
                await Shell.Current.DisplayAlert("Error", "Failed getting customer details", "OK");
                return;
            }

            await Shell.Current.GoToAsync($"{nameof(CustomerDetailPage)}", true,
                new Dictionary<string, object>()
                {
                    { nameof(Customer), new CustomerWrapper(customer) }
                });
        }

        //[RelayCommand]
        //async Task GetClosestCustomer()
        //{
        //    if (IsBusy || Customers.Count == 0)
        //        return;

        //    try
        //    {
        //        IsBusy = true;
        //        // Get cached location, else get real location.
        //        var location = await geolocation.GetLastKnownLocationAsync();
        //        if (location == null)
        //        {
        //            location = await geolocation.GetLocationAsync(new GeolocationRequest
        //            {
        //                DesiredAccuracy = GeolocationAccuracy.Medium,
        //                Timeout = TimeSpan.FromSeconds(20)
        //            });
        //        }

        //        // Find closest monkey to us
        //        var first = Customers.OrderBy(m => location.CalculateDistance(
        //            new Location(m.Geo_latitute, m.Geo_longitude), DistanceUnits.Miles))
        //            .FirstOrDefault();
        //        IsBusy = false;
        //        await Shell.Current.DisplayAlert("Closest Resident", $"Resident Name:{first.Custname} , Location:{ first.Address1}", "OK");

        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Unable to query location: {ex.Message}");
        //        await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        //    }
        //}
    }
}
