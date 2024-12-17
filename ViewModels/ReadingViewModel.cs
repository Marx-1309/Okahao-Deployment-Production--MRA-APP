

namespace SampleMauiMvvmApp.ViewModels
{
    [QueryProperty("Area", "Area")]
    [QueryProperty("Refresh", "Refresh")]
    public partial class ReadingViewModel : BaseViewModel
    {
        
        ReadingService readingService;
        ReadingExportService readingExportService;
        CustomerService customerService;
        MonthService monthService;
        DbContext dbContext;
        AppShell appShell;
        public ReadingViewModel(ReadingService _readingService,ReadingExportService _readingExportService, CustomerService _customerService, MonthService _monthService, DbContext _dbContext,AppShell _appShell)
        {
            this.readingService = _readingService;
            this.readingExportService = _readingExportService;
            this.customerService = _customerService;
            this.monthService = _monthService;
            this.dbContext = _dbContext;
            this.appShell = _appShell;
        }

        [ObservableProperty]
        bool isRefreshing;
        [ObservableProperty]
        string area;
        [ObservableProperty]
        bool isAllLocationsCaptured;
        [ObservableProperty]
        int capturedReadingsCount;
        [ObservableProperty]
        int uncapturedReadingsCount;
        [ObservableProperty]
        int zeroReadingsCount;
        [ObservableProperty]
        int abnormalCount;
        [ObservableProperty]
        string uncapturedTitle;
        [ObservableProperty]
        string capturedTitle;

        public ObservableCollection<Reading> AllReadings { get; set; } = new();
        public ObservableCollection<LocationReadings> AllLocation { get; set; } = new();
        public static List<Reading> ReadingsListForSearch { get; private set; } = new List<Reading>();
        public static List<LocationReadings> LocationListForSearch { get; private set; } = new List<LocationReadings>();
        public ObservableCollection<Reading> Readings { get; set; } = new ObservableCollection<Reading>();
        public ObservableCollection<Reading> exceptionReadings { get; set; } = new ObservableCollection<Reading>();


        [RelayCommand]
        async Task GetCapturedReadings()
        {
            if (IsBusy) return; // Return an empty list if already busy

            try
            {
                IsBusy = true;
                var listOfCapturedReadings = await readingService.GetAllCapturedReadings();
                if (listOfCapturedReadings != null && listOfCapturedReadings.Count > 0)
                {
                    AllReadings.Clear(); // Clear the list before adding readings
                    foreach (var reading in listOfCapturedReadings)
                    {
                        var IsFlagged = IsReadingFlagged((decimal)reading.PREVIOUS_READING,reading.CURRENT_READING);
                        if (IsFlagged)
                        {
                            reading.IsFlagged = true;
                        }
                        if (reading.CURRENT_READING >= 1)
                        {
                            reading.ReadingTaken = true;
                            reading.ReadingNotTaken = false;
                        }
                        else
                        {
                            reading.ReadingTaken = false;
                            reading.ReadingNotTaken = true;
                        }
                        Task.Delay(50);
                        AllReadings.Add(reading); // Add each reading to the list

                    }
                    //foreach (var reading in listOfCapturedReadings)
                    //{
                    //    Readings.Add(reading);
                    //}
                    ReadingsListForSearch.Clear();
                    Task.Delay(50);
                    ReadingsListForSearch.AddRange(listOfCapturedReadings);
                    CapturedReadingsCount = listOfCapturedReadings.Count();
                    AbnormalCount = listOfCapturedReadings
                    .Where(r => r != null && (r.CURRENT_READING - r.PREVIOUS_READING) > 20)
                    .Count();
                    ZeroReadingsCount = listOfCapturedReadings.Where(r => r?.CURRENT_READING == r.PREVIOUS_READING).Count();
                    // Set IsBusy to false after adding all readings
                    CapturedTitle = $"Captured readings : {CapturedReadingsCount} , Zero readings : {ZeroReadingsCount} , Abnormal readings : {AbnormalCount}";


                }
                else
                {
                    await Shell.Current.DisplayAlert("Not Found", "No Captured readings found", "OK");
                }
            }
            catch (Exception 
            ex)
            {
                await Shell.Current.DisplayAlert("Unable to retrieve any readings", "Please try again", "OK");
            }
            finally
            {
                IsBusy = false; // Ensure IsBusy is set to false in case of any exception or no readings found
                IsRefreshing = false;
            }
            isRefreshing = false;
            IsBusy = false;
        }



        [RelayCommand]
        async Task GetUncapturedReadings()
        {
            if (IsBusy) return; // Return an empty list if already busy

            try
            {
                IsBusy = true;
                var listOfUnCapturedReadings = await readingService.GetAllUncapturedReadings();
                if (listOfUnCapturedReadings != null && listOfUnCapturedReadings.Count > 0)
                {
                    AllReadings.Clear(); // Clear the list before adding readings
          
                    foreach (var reading in listOfUnCapturedReadings)
                    {
                        if (reading.CURRENT_READING >= 1)
                        {
                            reading.ReadingTaken = true;
                            reading.ReadingNotTaken = false;
                        }
                        else
                        {
                            reading.ReadingTaken = false;
                            reading.ReadingNotTaken = true;
                        }
                        Task.Delay(500);
                        AllReadings.Add(reading); // Add each reading to the list

                    }
                  
                    foreach (var reading in listOfUnCapturedReadings)
                    {
                        Readings.Add(reading);
                    }
                    ReadingsListForSearch.Clear();
                    Task.Delay(50);
                    var uncapturedReadings = listOfUnCapturedReadings.Where(r => r != null).ToList();
                    ReadingsListForSearch.AddRange(uncapturedReadings);

                    int uncapturedReadingsCount = uncapturedReadings.Count;
                    int zeroReadingsCount = uncapturedReadings.Count(r => r.CURRENT_READING == r.PREVIOUS_READING);

                    UncapturedTitle = $"Uncaptured Readings : {uncapturedReadingsCount}, Meters not in use : {zeroReadingsCount}";
                }
                else
                {
                    await Shell.Current.DisplayAlert("Not Found", "No Uncaptured readings found", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Unable to retrieve any readings", "Please try again", "OK");
            }
            finally
            {
                IsBusy = false; // Ensure IsBusy is set to false in case of any exception or no readings found
                IsRefreshing = false;
            }

            isRefreshing = false;
            IsBusy = false;
        }


        [RelayCommand]
        public async Task GoToCustomerDetails(Reading reading)
        {
            if (reading.CUSTOMER_NUMBER == null) return;

            var customer = await customerService.GetCustomerDetails(reading.CUSTOMER_NUMBER);
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


        [RelayCommand]
        public async Task ScanForNewExport()
        {
            IsBusy = true;
            await Task.Delay(1000);
            await readingExportService.ScanForNewItems();
            IsBusy = false;
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task ScanForNewlyAddedCustomerReadings()
        {
            IsBusy = true;
            await Task.Delay(1000);
            await readingService.ScanNewCustomersReadingsFromSql();
            IsBusy = false;
            await Shell.Current.GoToAsync("..");
        }

        private async Task DisplayAlert(string v1, string v2, string v3, string v4)
        {
            await Shell.Current.DisplayAlert(v1, v2, v3, v4);
        }

        [RelayCommand]
        public async Task ReflushData()
        {
            IsBusy = true;
            await Task.Delay(50);
            //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            //var snackbarOptions = new SnackbarOptions
            //{
            //    BackgroundColor = Colors.Red,
            //    TextColor = Colors.Green,
            //    ActionButtonTextColor = Colors.Yellow,
            //    CornerRadius = new CornerRadius(10),
            //    Font = Microsoft.Maui.Font.SystemFontOfSize(14),
            //    ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(14),
            //    CharacterSpacing = 0.5
            //};

            //string text = "This is a Snackbar";
            //string actionButtonText = "Click Here to Dismiss";
            //Action action = async () => await DisplayAlert("Reseting and re-seeding", "You are about to delete and restore", "OK", "Cancel");
            //if (action.Equals("Cancel")) return;
            //TimeSpan duration = TimeSpan.FromSeconds(5);

            //var snackbar = Snackbar.Make(text, action, actionButtonText, duration, snackbarOptions);

            //await snackbar.Show(cancellationTokenSource.Token);
            await readingExportService.FlushAndSeed();
            IsBusy = false;
            await Shell.Current.GoToAsync("..");
        }

        //Get Locations list

        [RelayCommand]
        async Task GetLocations()
        {
            //if (IsBusy) return; // Return an empty list if already busy

            try
            {
                IsBusy = true;

                var listOfLocations = await readingService.GetListOfLocations();
                if (listOfLocations != null && listOfLocations.Count > 0)
                {

                    AllLocation.Clear(); // Clear the list before adding readings

                    foreach (var location in listOfLocations)
                    {
                        if ((bool)location.IsAllCaptured)
                        {
                            location.IsAllNotCaptured = false;
                        }
                        else
                        {
                            location.IsAllNotCaptured = true;
                        }

                        AllLocation.Add(location);
                    }
                    LocationListForSearch.Clear();
                    Task.Delay(50);
                    LocationListForSearch.AddRange(listOfLocations);

                    // Set IsBusy to false after adding all readings

                }
                else
                {
                    await Shell.Current.DisplayAlert("Not Found", "No locations found", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Unable to retrieve any locations", "Please try again", "OK");
            }
            finally
            {
                IsBusy = false; // Ensure IsBusy is set to false in case of any exception or no readings found
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task GoToListOfUncapturedReadingsByArea(LocationReadings area)
        {
            //if (IsBusy) return; // Return an empty list if already busy
            try
            {
                IsBusy = true;
                if (area == null)
                {
                    await Shell.Current.GoToAsync("..");
                    return;
                } ;
                var listReadings = new List<Reading>();
                //if (monthId.MonthID <= 0) return;
                var uncapturedReadings = await readingService.GetUncapturedReadingsByArea(area);

                if (uncapturedReadings != null)
                {
                    Title = area?.AREANAME;

                    AllReadings.Clear();
                    foreach (var i in uncapturedReadings)
                    {
                        i.ReadingTaken = false;
                        i.ReadingNotTaken = true;
                        Task.Delay(50);
                        AllReadings.Add(i);
                    }

                   await Shell.Current.GoToAsync($"{nameof(UncapturedReadingsByAreaPage)}", true,
                   new Dictionary<string, object>()
                   {
                    { nameof(List<Reading>), new List<Reading>(AllReadings) }
                   });

                }

                if (uncapturedReadings.Count == 0)
                {
                    await Shell.Current.DisplayAlert("No Readings", $"No records found here.", "OK");
                     if (IsBusy = true) { IsBusy = !IsBusy; }
                     await Task.Delay(500);
                    await Shell.Current.GoToAsync("..");

                    
                }
                IsBusy = false;
            }
            catch(Exception ex)
            {
                IsBusy = false;
            }
            
            
        }

        [RelayCommand]
        public async Task GoToExceptionList()
        {


            string email = Preferences.Default.Get("username", "Unknown");
            string[] parts = email.Split('@');


            try
            {
                var i = await dbContext.Database.Table<Reading>().Where(r => r.CURRENT_READING > 0).ToListAsync();
                var ii = i.Where(r => r.CURRENT_READING - r.PREVIOUS_READING >= 20).Select(reading => new {
                                                                                                            Name = string.Join(" ", reading.CUSTOMER_NAME.Split().Take(2)),
                                                                                                            Meter = reading.METER_NUMBER,
                                                                                                            CurrentReading = reading.CURRENT_READING,
                                                                                                            WaterUsage = reading.CURRENT_READING - reading.PREVIOUS_READING,
                                                                                                            MeterReader = reading.METER_READER,
                                                                                                            ErfNo = reading.ERF_NUMBER,
                                                                                                            Date = reading.ReadingDate,}).ToList();
                exceptionReadings.Clear();

                foreach (var item in ii)
                {
                    Reading exReading = new()
                    {
                        CUSTOMER_NAME = item.Name,
                        ERF_NUMBER = item.ErfNo,
                        METER_NUMBER = item.Meter,
                        CURRENT_READING = item.CurrentReading,
                        PercentageChange = (int?)item.WaterUsage,
                        ReadingDate = item.Date,
                        METER_READER = item.MeterReader
                    };

                    if(string.IsNullOrEmpty(exReading.METER_READER))
                    {
                        exReading.METER_READER = parts[0];
                    }
                    Task.Delay(50);
                    exceptionReadings.Add(exReading);
                }

            }
            catch(Exception ex)
            {
                StatusMessage = ex.ToString();
            }
            isRefreshing = false;
            IsBusy = false;
        }

        public bool IsReadingFlagged(decimal previous, decimal current)
        {
            decimal difference = Math.Abs(current - previous);

            if (difference >= 20)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool IsLocationCleared(int count)
        {
            if (count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        [RelayCommand]
        public async Task ResetReading()
        {
            string response = "x";
             response = await Shell.Current.DisplayPromptAsync(
                "Delete ,Reset or Sync",
                "",
                "Confirm",
                "Cancel",
                "Enter your command here...",
                keyboard: Keyboard.Text);

            if(response == "%sync%")
            {
                IsBusy = true;
                await Task.Delay(5000);
                var readingsList = dbContext.Database.Table<Reading>().ToListAsync().GetAwaiter().GetResult();
                foreach (var i in readingsList)
                {
                    i.ReadingSync = true;
                    i.AreaUpdated = true;
                    i.ReadingNotTaken = false;
                    i.ReadingTaken = true;

                }
                var n = dbContext.Database.UpdateAllAsync(readingsList).GetAwaiter().GetResult();

                IsBusy = false;
                await Shell.Current.DisplayAlert("Success", response.Replace("%","").ToUpper() + " Completed!", "Ok");
            }
            if (response == "%reset%")
            {
                IsBusy=true;
                await Task.Delay(5000);
                var readingsList = dbContext.Database.Table<Reading>().ToListAsync().GetAwaiter().GetResult();
                foreach (var i in readingsList)
                {
                    i.ReadingSync = false;
                    //i.AreaUpdated = true;
                    i.ReadingNotTaken = false;
                    i.ReadingTaken = true;
                }
                var n = dbContext.Database.UpdateAllAsync(readingsList).GetAwaiter().GetResult();

                await Shell.Current.DisplayAlert("Success", response.Replace("%", "").ToUpper() + " Completed!", "Ok");
                IsBusy = false;
            }
            if (response == "%delete%")
            {
                IsBusy = true;
                await Task.Delay(5000);
                #region deleting existing db data
                List<ReadingExport> result1 = await dbContext.Database.Table<ReadingExport>().Where(i => i.WaterReadingExportID > 0).ToListAsync();
                List<Reading> result2 = await dbContext.Database.Table<Reading>().Where(i => i.Id > 0).ToListAsync();
                List<Customer> result3 = await dbContext.Database.Table<Customer>().Where(i => i.CUSTNMBR != null).ToListAsync();
                //List<Month> result4 = await dbContext.Database.Table<Month>().Where(i => i.MonthID > 0).ToListAsync();
                List<ReadingMedia> result5 = await dbContext.Database.Table<ReadingMedia>().Where(i => i.Id > 0).ToListAsync();

                if (result1.Count > 0 || result1.Any())
                {
                    await dbContext.Database.Table<ReadingExport>().DeleteAsync(r => r.WaterReadingExportID > 0);
                }

                if (result2.Count > 0 || result2.Any())
                {
                    await dbContext.Database.Table<Reading>().DeleteAsync(r => r.Id > 0);
                }

                if (result3.Count > 0 || result3.Any())
                {
                    await dbContext.Database.Table<Customer>().DeleteAsync(r => r.CUSTNMBR != null);
                }

                //if (result4.Count > 0 || result4.Any())
                //{
                //    await dbContext.Database.Table<Month>().DeleteAsync(r => r.MonthID > 0);
                //}

                if (result5.Count > 0 || result5.Any())
                {
                    await dbContext.Database.Table<ReadingMedia>().DeleteAsync(r => r.Id > 0);
                }
                #endregion
                IsBusy = false;

                await Shell.Current.DisplayAlert("Success",response.Replace("%", "").ToUpper() + " Completed!","Ok");

            }
            if(!(response == "%sync%" || response == "%reset%" || response == "%delete%" || response == null))
            {
                DisplayToast("Command not recognized");
            }
            else if (response == null)
            {
                return;
            }
            
        }
    }
}


    

