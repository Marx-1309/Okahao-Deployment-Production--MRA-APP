using CommunityToolkit.Maui.Converters;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SampleMauiMvvmApp.ViewModels
{
    [QueryProperty("Customer", "Customer")]
    [QueryProperty("Reading", "Reading")]
    public partial class CustomerDetailViewModel : BaseViewModel
    {
        DbContext dbContext;
        ReadingService readingService;
        MonthService monthService;
        CustomerService customerService;
        [ObservableProperty]
        CustomerWrapper customer;
        [ObservableProperty]
        ReadingWrapper reading;
        [ObservableProperty]
        string erfNumber;
        [ObservableProperty]
        string custStateErf;
        [ObservableProperty]
        long custphone1;
        [ObservableProperty]
        decimal custPrevReading;
        [ObservableProperty]
        decimal custCurrentReading;
        [ObservableProperty]
        string totalUsage;
        [ObservableProperty]
        string meterNumber;
        [ObservableProperty]
        string routeNumber;
        [ObservableProperty]
        ReadingWrapper vmReading;
        //[ObservableProperty]
        //string? currentMonth;
        [ObservableProperty]
        public static bool isExist;
        [ObservableProperty]
        bool isUpdate;
        [ObservableProperty]
        bool isCurrentReading;

        private int selectedCompressionQuality = 25;
        IGeolocation geolocation;
        public CustomerDetailViewModel(DbContext _dbContext, ReadingService readingService,
            CustomerService _customerService, MonthService _monthService, IGeolocation geolocation)
        {
            Title = "Customer Detail Page";
            this.dbContext = _dbContext;
            this.readingService = readingService;
            this.customerService = _customerService;
            this.monthService = _monthService;
            this.geolocation = geolocation;

            //WeakReferenceMessenger.Default.Register<ReadingCreateMessage>(this, (obj, handler) =>
            //{
            //    MainThread.BeginInvokeOnMainThread(() =>
            //    {
            //        var newReading = new ReadingWrapper(handler.Value)
            //        {
            //            IsNew = true
            //        };

            //        if (Customer.Readings == null) Customer.Readings = new();
            //        Customer.Readings.Insert(0, newReading);

            //    });
            //});
        }


        [RelayCommand]
        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("../..");
        }

        [RelayCommand]
        async Task CustDisplayDetailsAsync()
        {
            
            var reading = await readingService.GetLastReadingByIdAsync(Customer.Custnmbr);
            if (reading != null)
            {
                
                CustPrevReading = (decimal)reading.PREVIOUS_READING;
                CustCurrentReading = (decimal)reading.CURRENT_READING;
                MeterNumber = reading.METER_NUMBER;
                RouteNumber = reading.RouteNumber;
                Custphone1 = (long)reading.PHONE1;
                erfNumber = reading.ERF_NUMBER;
                TotalUsage = $"{((decimal?)reading.CURRENT_READING >= (decimal?)reading.PREVIOUS_READING ? (decimal?)reading.CURRENT_READING - (decimal?)reading.PREVIOUS_READING : 0)}";
                bool isCurrentReading = IsCurrentReadingCaptured(reading.CURRENT_READING);
                
                //CurrentMonth =  monthService?.GetCurrentMonthNameById(reading.MonthID).GetAwaiter().GetResult();
                if (isCurrentReading)
                {
                    IsCurrentReading = true;
                }
                else
                {
                    IsCurrentReading = false;
                }


                //CustStateErf = $"{reading.AREA.Trim()} - (ERF {reading.ERF_NUMBER.Replace("ERF","").Trim()})" ?? "NO ERF";
                bool result = IsUpdateMode(CustCurrentReading);
                if (result)
                {
                    IsUpdate = true;
                }
                else
                {
                    IsUpdate = false;
                }

                if (string.IsNullOrEmpty(reading.AREA) || !Regex.IsMatch(reading.ERF_NUMBER, @"\d") || reading.AREA is null)
                {
                    
                    CustStateErf = $"{reading?.AREA?.Trim()} - NO ERF";
                }
                else
                {
                    CustStateErf = $"{reading.AREA.Trim()} - (ERF {reading.ERF_NUMBER.Replace("ERF", "").Trim()})";
                }
                
                Title = $"{reading.CUSTOMER_NAME.Trim()}";
                
            }

            bool isExist = await readingService.IsReadingExistForMonthId(Customer.Custnmbr);
            IsExist = isExist;
            return;
        }


        [RelayCommand]
        public async Task CreateReadingAsync()
        {
            try
            {
                IsValid();
                var CurrentMonthReading = await readingService.GetCurrentMonthReadingByCustIdAsync(Customer.Custnmbr);
                var customerInfo = await customerService.GetCustomerDetails(Customer.Custnmbr);
                //var loggedInUser = await dbContext.Database.Table<LoginHistory>()?.OrderByDescending(r => r.LoginId).FirstAsync();

                if (!VmReading.C_reading.IsNullOrEmpty())
                {
                    if (int.TryParse(VmReading.C_reading, out int intValue))
                    {
                        CurrentMonthReading.CURRENT_READING = intValue;
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert($"Error",
                                                        $"Something went wrong while converting reading to int", "OK");
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert($"Null Or Empty",
                                                        $"Please enter a valid reading!", "OK");
                    return;
                }
                
                if (CurrentMonthReading.WaterReadingExportID <= 0)
                {
                    await Shell.Current.DisplayAlert($"No Reading Export Found",
                                                         $"Confirm Database and try again!", "OK");
                    await ClearForm();
                    return;
                }

                if (CurrentMonthReading.CURRENT_READING < CustPrevReading && CurrentMonthReading.CURRENT_READING >= 0)
                {

                    await Shell.Current.DisplayAlert($"Current Reading lesser than Previous of:{CustPrevReading}",
                                                          $"Please check current reading and try again!", "OK");

                    await Shell.Current.DisplayAlert($"Invalid Input!",
                                                          $"Please enter valid reading!", "OK");
                    await ClearForm();
                    return;
                }


                if (CurrentMonthReading.CURRENT_READING == 0)
                {
                   var myAction = await Shell.Current.DisplayAlert($"Zero(0) readings entered",
                                                          $"Are you sure you want to enter this reading?", "Cancel","Yes");

                    if (myAction)
                    {
                        await ClearForm();
                        return;
                    }
                }

                CurrentMonthReading.Comment = VmReading.Comment;
                //CurrentMonthReading.READING_DATE = DateTime.Now.ToString();
                //CurrentMonthReading.Meter_Reader = loggedInUser.Username;
                CurrentMonthReading.ReadingTaken = true;
                CurrentMonthReading.ReadingNotTaken = false;
                CurrentMonthReading.ReadingSync = false;
                CurrentMonthReading.WaterReadingExportID = (int)await readingService.GetLatestExportItemId();

                Reading newReading = new Models.Reading();
                //GetLocation();
                if (string.IsNullOrEmpty(CurrentMonthReading.AREA) ||
                                string.IsNullOrWhiteSpace(CurrentMonthReading.AREA.Trim()) ||
                                CurrentMonthReading.AREA.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(CurrentMonthReading.METER_NUMBER))
                    {
                        CurrentMonthReading.METER_NUMBER = CurrentMonthReading.METER_NUMBER;
                    }
                    else
                    {
                        var MeterNo = await UpdateCustomerMeterNo();
                        CurrentMonthReading.METER_NUMBER = MeterNo;
                        meterNumber = MeterNo.ToString();
                    }
                    CurrentMonthReading.AREA = await AddNewCustomerLocation(Customer.Custnmbr);
                    await readingService.InsertReading(CurrentMonthReading);
                    await GoBackAsync();
                }
                else
                {
                     newReading = await readingService.InsertReading(Models.Reading.GenerateNewFromWrapper(new ReadingWrapper(CurrentMonthReading)));
                }
                
                
                IsExist = true;


                if (newReading != null)
                {
                    var latestMonthName = await monthService.GetMonthNameById();
                    if(IsUpdate) 
                    {
                        await Shell.Current.DisplayAlert($"Success!", $"A reading for {CurrentMonthReading.CUSTOMER_NAME.Substring(0,15).Trim()}... Updated!", "OK");
                        CustCurrentReading = custCurrentReading;
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert($"Success!", $"A reading for {CurrentMonthReading.CUSTOMER_NAME.Substring(0,15).Trim() ?? $"customer"} Created!", "OK");
                    }

                    // Propagate the new reading to the main reading page.
                    WeakReferenceMessenger.Default.Send(new ReadingCreateMessage(newReading));
                    await Task.Delay(1000);
                    await GoBackAsync();
                }

                else
                {
                    await Shell.Current.DisplayAlert($"Error!",
                                   $"Something Wrong,Try Again!", "OK");
                    await ClearForm();
                    await Task.Delay(500);
                    await GoBackAsync();
                }
            }
            catch
            {
                return;
            }
        }

        [RelayCommand]
        public async Task OnTakePhotoClicked(CancellationToken xToken)
        {
            var options = new StoreCameraMediaOptions { CompressionQuality = selectedCompressionQuality };
            var result = await CrossMedia.Current.TakePhotoAsync(options);
            if (result is null) return;

            var fileInfo = new FileInfo(result?.Path);
            var fileLength = fileInfo.Length;

            // Convert the image to Base64 string
            byte[] imageData = File.ReadAllBytes(result?.Path);
            string base64Image = Convert.ToBase64String(imageData);

            // Save the data to the database
            var latestExportItem = await dbContext.Database.Table<ReadingExport>()
                       .OrderByDescending(r => r.WaterReadingExportID)
                       .FirstOrDefaultAsync();

            int currentExportId = latestExportItem.WaterReadingExportID;

            Reading reading = await dbContext.Database.Table<Reading>()
                .Where(r => r.CUSTOMER_NUMBER == Customer.Custnmbr && r.WaterReadingExportID == currentExportId)
                .FirstOrDefaultAsync();

            List<ReadingMedia> existingImage = await dbContext.Database.Table<ReadingMedia>().Where(r => r.WaterReadingExportDataId == reading.WaterReadingExportDataID).ToListAsync();
            List<ReadingMedia> existingImages = await dbContext.Database.Table<ReadingMedia>().ToListAsync();

            ReadingMedia capturedImage = new()
            {
                WaterReadingExportDataId = reading.WaterReadingExportDataID,
                WaterReadingExportId = (int)reading.WaterReadingExportID,
                Title = result.OriginalFilename,
                MeterImage = base64Image,
                DateTaken = DateTime.UtcNow.ToLongDateString(),
            };

            if (existingImage.Any())
            {
                foreach (var img in existingImage)
                {
                    await dbContext.Database.DeleteAsync(img);
                }
            }
            int isSaved = await dbContext.Database.InsertAsync(capturedImage);
            if(isSaved == 1)
            {
                 await Toast.Make("image saved", CommunityToolkit.Maui.Core.ToastDuration.Short, 10).Show();
            }
        }


        #region Get Current Location
        public async void GetLocation()

        {

            var location = await geolocation.GetLastKnownLocationAsync();
            if (location == null)

            {
                location = await geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                    ,RequestFullAccuracy = true,

                });
                await Shell.Current.DisplayAlert($"Current Location!", $"Longitude is {location.Longitude} , Latitude {location.Latitude}", "OK");
            }
            return;
        } 

    #endregion

        public bool IsValid()
        {
            try
            {

                Guard.Against.OutOfRange<Decimal>((decimal)VmReading.Current_reading, nameof(VmReading.Current_reading), 0, Decimal.MaxValue);
                Guard.Against.OutOfRange<Decimal>((decimal)VmReading.Current_reading, nameof(VmReading.Current_reading), 100000, Decimal.MinValue);
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                return false;
            }

            return true;
        }

        public bool IsCurrentReadingCaptured(decimal? Currreading)
        {
            if ((decimal?)Currreading > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsUpdateMode(decimal existingReading)
        {
            if (existingReading > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [RelayCommand]
        async Task ClearForm()
        {
            await Task.Yield();
            VmReading.C_reading = string.Empty;
        }

        #region CustomerLocations
        //string items1 = "ONESI";
        //string items2 = "OKALONGO";
        string items3 = "OGONGO";
        string items4 = "UNCLASSIFIED";
        //string items1 = "EXTENSION 1";
        //string items2 = "EXTENSION 2";
        //string items3 = "EXTENSION 3";
        //string items4 = "EXTENSION 4";
        //string items5 = "EXTENSION 5";
        //string items6 = "EXTENSION 6";
        //string items7 = "EXTENSION 7";
        //string items8 = "EXTENSION 8";
        //string items9 = "EXTENSION 9";
        //string items10 = "EXTENSION 10";
        //string items11 = "EXTENSION 11";
        //string items12 = "EXTENSION 12";
        //string items13 = "EXTENSION 13";
        //string items14 = "EXTENSION 14";
        //string items15 = "OKAHAO PROPER";
        //string items16 = "KASHENDA PROPER";
        //string items17 = "KASHENDA EXTENSION 1";
        //string items18 = "EHAO PROPER";
        //string items19 = "EHAO EXTENSION 1";
        //string items20 = "UNCLASSIFIED";


        #endregion

        public async Task<string> AddNewCustomerLocation(string customerNo)
        {
            var cstObj = await dbContext.Database.Table<Reading>()
                            .Where(r => r.CUSTOMER_NUMBER == customerNo)
                            .FirstOrDefaultAsync();

            bool hasLocation = false;

            // Check initial condition
            if (cstObj != null)
            {
                if (cstObj.AREA != null)
                {
                    cstObj.AREA = cstObj.AREA.Trim();
                }

                hasLocation = !(string.IsNullOrEmpty(cstObj.AREA) ||
                                string.IsNullOrWhiteSpace(cstObj.AREA) ||
                                cstObj.AREA.Equals("NULL", StringComparison.OrdinalIgnoreCase));
            }

            // Keep prompting until a valid location is entered
            while (!hasLocation)
            {
                var userLocation = await Shell.Current.DisplayActionSheet(
                    "Select Location",null,null,items3, items4
                    );

                if (!string.IsNullOrEmpty(userLocation) &&
                    !string.IsNullOrWhiteSpace(userLocation) &&
                    !userLocation.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                {
                    var newReading = await readingService.GetCurrentMonthReadingByCustIdAsync(cstObj.CUSTOMER_NUMBER);
                    newReading.AREA = userLocation.Trim();
                    
                    var custNewArea = await readingService.UpsertArea(newReading);
                    hasLocation = true;
                    return custNewArea;
                }

                
            }
            
            return "";
        }

        [RelayCommand]
        public async Task<string> UpdateCustomerMeterNo()
        {
            try
            {
                var cstObj = await dbContext.Database.Table<Reading>()
                              .Where(r => r.CUSTOMER_NUMBER == Customer.Custnmbr)
                              .FirstOrDefaultAsync();

                var userMenterNo = await Shell.Current.DisplayPromptAsync(
                    "Add Customer Meter",
                    "Please enter customer meter",
                    "Add",
                    "Cancel",
                    "Enter meter number here...",
                    keyboard: Keyboard.Text);
                if (!string.IsNullOrWhiteSpace(userMenterNo))
                {
                    cstObj.METER_NUMBER = userMenterNo;
                    var custNewMeter = await readingService.UpsertMeter(cstObj);

                    if (!string.IsNullOrEmpty(custNewMeter))
                    {
                        await Shell.Current.DisplayAlert("Success!", "Meter Updated!", "OK");
                        MeterNumber = custNewMeter;
                    }
                    return userMenterNo;
                }
                else
                {
                    cstObj.METER_NUMBER = cstObj?.METER_NUMBER;
                    var custNewArea = await readingService.UpsertMeter(cstObj);
                    return cstObj?.METER_NUMBER;
                }
            }
            catch(Exception ex)
            {

            }
            return "";
        }

        [RelayCommand]
        public async Task<string> UpdateCustomerLocation()
        {
            StatusMessage = "";
            try
            {
                var cstObj1 = await dbContext.Database.Table<Reading>()
                              .Where(r => r.CUSTOMER_NUMBER == Customer.Custnmbr)
                              .FirstOrDefaultAsync();

                if (cstObj1 != null)
                {
                    var userLocation = await Shell.Current.DisplayActionSheet(
                        "Select Location", null, null, items3,items4
                    );

                    if (!string.IsNullOrEmpty(userLocation))
                    {
                        cstObj1.AREA = userLocation;

                        var custNewArea = await readingService.UpsertArea(cstObj1);

                        if (!string.IsNullOrEmpty(custNewArea))
                        {
                            await Shell.Current.DisplayAlert("Success!", "Location Updated!", "OK");
                            CustStateErf =  custNewArea;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", "Location could not be updated", "OK");
            }
            return "";
        }

    }
}