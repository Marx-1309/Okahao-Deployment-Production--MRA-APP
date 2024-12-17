using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleMauiMvvmApp.SearchHandlers
{
    public class LocationSearchHandler : SearchHandler
    {
        public LocationSearchHandler()
        {
        }


        public ReadingService readingService;
        public DbContext dbContext;


        public IList<LocationReadings> Locations { get; set; }
        public string NavigationRoute { get; set; }
        public Type NavigationType { get; set; }
        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);

            if (string.IsNullOrWhiteSpace(newValue))
            {
                ItemsSource = null;
            }
            else
            {
                //ItemsSource = customerWrappers.Where(customer => customer.Custname.ToLower().Contains(newValue.ToLower()));
                ItemsSource = Locations.Where(location => location.AREANAME.ToLower().Contains(newValue.ToLower()));
            }
        }

        //protected override async void OnItemSelected(object item)
        //{
        //    if (item is LocationReadings reading)
        //    {
        //        Customer customer = new Customer()
        //        {
        //            CUSTCLAS = reading.AREANAME,
        //        };


        //        if (customer == null)
        //        {
        //            await Shell.Current.DisplayAlert("Error", "Failed getting customer details", "OK");
        //            return;
        //        }



        //        if (customer == null)
        //        {
        //            await Shell.Current.DisplayAlert("Error", "Failed getting customer details", "OK");
        //            return;
        //        }

        //        await Shell.Current.GoToAsync($"{nameof(CustomerDetailPage)}", true,
        //            new Dictionary<string, object>()
        //            {
        //        { nameof(Customer), new CustomerWrapper(customer) }
        //            });
        //    }
        //}


    }
}
