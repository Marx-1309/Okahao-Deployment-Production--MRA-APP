
using System.Net.NetworkInformation;

namespace SampleMauiMvvmApp.Services
{
    public interface IReadingExportService
    {
        Task<List<ReadingExport>> CheckForNewExportInSql();
        Task CheckNewExports();
        Task DeleteOldReadings();
        Task GetLatestExportItemIntoSqlite();
        Task ScanForNewExports();
    }

    public class ReadingExportService 
    {
        HttpClient _httpClient;
        public string StatusMessage;
        public DbContext dbContext;
        ReadingService readingService;
        CustomerService customerService;
        IConnectivity connectivity;


        public ReadingExportService(DbContext _dbContext, ReadingService _readingService, CustomerService _customerService, IConnectivity _connectivity)
        {
            this._httpClient = new HttpClient();
            this.dbContext = _dbContext;
            this.readingService = _readingService;
            this.customerService = _customerService;
            this.connectivity = _connectivity;
        }

        public ReadingExportService()
        {
        }

        #region Get ReadingExport

        List<ReadingExport> readingExports;

        #region Check for Newly Added Exports
        public async Task CheckNewExports()
        {
            try
            {
                if (connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await Shell.Current.DisplayAlert("No connectivity!",
                        $"Please check internet and try again.", "OK");
                    return;
                }

                var responseSql = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.ReadingExport);
                var exportsList = await dbContext.Database.Table<ReadingExport>().ToListAsync();


                var existingIds = exportsList
                           .Select(r => r.WaterReadingExportID)
                           .ToList();
                if (responseSql.IsSuccessStatusCode)
                {
                    // Read and deserialize the response to a List<ReadingExport>
                    var newReadingExports = await responseSql.Content.ReadFromJsonAsync<List<ReadingExport>>();

                    // Filter the new ReadingExport items to get only the ones that do not exist in the SQLite database
                    var newItemsToInsert = newReadingExports
                        .Where(r => !existingIds.Contains(r.WaterReadingExportID))
                        .ToList();


                    if (newItemsToInsert.Any())
                    {
                        latestExport.Clear();
                        latestExport.AddRange(newItemsToInsert);
                        // Insert the new items into the SQLite database
                        //var response2 = await dbContext.Database.InsertAllAsync(newItemsToInsert);

                        foreach (var item in latestExport)
                        {
                            ReadingExport readingExport = new()
                            {
                                WaterReadingExportID = item.WaterReadingExportID,
                                MonthID = item.MonthID,
                                Year = item.Year,

                            };

                            await dbContext.Database.InsertAsync(readingExport);
                        }
                    }

                    //Download the customers that do not exist in sqlite database
                    var customerInSqlite = await dbContext.Database.Table<Customer>().ToListAsync();
                    var customerFromSqlServer = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.GetCustomer);

                    if (customerFromSqlServer.IsSuccessStatusCode)
                    {
                        // Read the response content as a string
                        var responseContent = await customerFromSqlServer.Content.ReadAsStringAsync();
                        // Deserialize the response content into the List<Customer>
                        var sqlServerCustomerList = JsonConvert.DeserializeObject<List<Customer>>(responseContent);
                        var sqliteReadingsList = await dbContext.Database.Table<Reading>().ToListAsync();


                        //  Identify the IDs of new customers that match those in sqlServerReadingsList
                        var matchingCustomerIDs = sqlServerCustomerList
                            .Where(customer => sqliteReadingsList.Any(reading => reading.CUSTOMER_NUMBER == customer.CUSTNMBR))
                            .Select(customer => customer.CUSTNMBR)
                            .ToList();

                        //  Filter and insert only new customers from the API whose IDs match
                        var newCustomersToInsert = sqlServerCustomerList
                            .Where(customer => matchingCustomerIDs.Contains(customer.CUSTNMBR))
                            .ToList();

                        //  Fetch the list of customers from the SQLite database
                        var sqliteCustomerList = await dbContext.Database.Table<Customer>().Where(c => c.CUSTNMBR != null).ToListAsync();

                        //  Filter out new customers that already exist in SQLite
                        var newCustomersNotInSQLite = newCustomersToInsert
                            .Where(customer => !sqliteCustomerList.Any(sqliteCustomer => sqliteCustomer.CUSTNMBR == customer.CUSTNMBR))
                            .ToList();

                        if (newCustomersNotInSQLite.Count > 0)
                        {
                            //  Add new customers to the SQLite database
                            await dbContext.Database.InsertAllAsync(newCustomersNotInSQLite);
                        }

                        // Update the CustomerList with the combined list
                        //CustomerList = sqliteCustomerList.Concat(newCustomersNotInSQLite).ToList();
                    }




                }
                return;


            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }
        #endregion



        // Initialize the lists
        List<ReadingExport> LatestExportList { get; set; } = new();
        List<Customer> LatestCustomerList { get; set; } = new();
        List<Reading> LatestReadingList { get; set; } = new();
        public async Task ScanForNewItems()
        {
            if (readingExports == null)
            {
                try
                {

                    if (connectivity.NetworkAccess != NetworkAccess.Internet)
                    {
                        await Shell.Current.DisplayAlert("No connectivity!",
                            $"Please check internet and try again.", "OK");
                        return;
                    }
                    //Get lists from APi
                    var responseSql1 = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.ReadingExport);
                    var responseSql2 = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.GetCustomer);
                    var responseSql3 = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.GetReading);

                    //Get Lists
                    var exportsList = await dbContext.Database.Table<ReadingExport>().ToListAsync();
                    var customerList = await dbContext.Database.Table<Customer>().ToListAsync();
                    var readingList = await dbContext.Database.Table<Reading>().ToListAsync();

                    //Get Existing Id's from Sqlite 
                    //Exports
                    var existingExportIds = exportsList
                           .Select(r => r.WaterReadingExportID)
                           .ToList();

                    //Customers
                    var existingCustomerIds = customerList
                           .Select(r => r.CUSTNMBR)
                           .ToList();

                    //Readings
                    var existingReadingIds = readingList
                           .Select(r => r.WaterReadingExportDataID)
                           .ToList();

                    var existingCustomerReadingIds = readingList
                           .Select(r => r.CUSTOMER_NUMBER)
                           .ToList();

                    if (responseSql1.IsSuccessStatusCode && responseSql2.IsSuccessStatusCode && responseSql3.IsSuccessStatusCode)
                    {
                        // Read the response content as a string
                        var responseContent1 = await responseSql1.Content.ReadAsStringAsync();
                        var responseContent2 = await responseSql2.Content.ReadAsStringAsync();
                        var responseContent3 = await responseSql3.Content.ReadAsStringAsync();

                        // Deserialize the response content into the List<Customer>
                        var newApiReadingExports = JsonConvert.DeserializeObject<List<ReadingExport>>(responseContent1);
                        var newApiCustomers = JsonConvert.DeserializeObject<List<Customer>>(responseContent2);
                        var newApiReadings = JsonConvert.DeserializeObject<List<Reading>>(responseContent3);


                        //ensuring that r.WaterReadingExportID is greater than the WaterReadingExportID of currentExportItem.
                        //This means that only items with an WaterReadingExportID greater than the current one will be included in the newExportToInsert list.

                        ReadingExport currentExportItem = await dbContext.Database.Table<ReadingExport>().FirstOrDefaultAsync();

                        // Filter the items to get only the ones that do not exist in the SQLite database
                        var newExportToInsert = newApiReadingExports
                            .Where(r => !existingExportIds.Contains(r.WaterReadingExportID) && r.WaterReadingExportID > currentExportItem?.WaterReadingExportID)
                            .ToList();


                        var newCustomers = newApiCustomers
                            .Where(r => !existingCustomerIds.Contains(r.CUSTNMBR))
                            .ToList();
                        //Firstly i filtered out the new customers that do not exist in the customer's table 
                        //Here filter out the new customers from the APi that do have existing readings in the readings table and save them
                        var newCustomersToInsert = newCustomers.Where(r => existingCustomerReadingIds.Contains(r.CUSTNMBR)).ToList();
                        //Here check if the newCustomerToInsert have an existing reading in Readings from the API (use customerNumber)


                        var newReadingToInsert = newApiReadings
                            .Where(r => !existingReadingIds.Contains(r.WaterReadingExportDataID))
                            .ToList();

                        var newReadingToUpdate = newApiReadings
                            .Where(r => existingReadingIds.Contains(r.WaterReadingExportDataID))
                            .ToList();

                        //Insert Non-Existing Exports
                        if (newExportToInsert.Any())
                        {
                            await Shell.Current.DisplayAlert("New Reading Exports Found!", $"We Are Updating The App!", "OK");
                            await Shell.Current.GoToAsync($"{nameof(SynchronizationPage)}");


                            List<ReadingExport?> exportsItemsToDelete = await dbContext.Database.Table<ReadingExport>().ToListAsync();

                            if (exportsItemsToDelete.Any())
                            {
                                await dbContext.Database.Table<ReadingExport>().DeleteAsync(r => r.WaterReadingExportID > 0);
                            }

                            List<Notes> notesToDelete = await dbContext.Database.Table<Notes>().ToListAsync();

                            if (notesToDelete.Any())
                            {
                                await dbContext.Database.Table<Notes>().DeleteAsync(r => r.NoteID > 0);
                            }


                            LatestExportList.Clear();
                            LatestExportList.AddRange(newExportToInsert);
                            // Insert the new items into the SQLite database
                            //var response2 = await dbContext.Database.InsertAllAsync(newItemsToInsert);

                            foreach (var item in LatestExportList)
                            {
                                ReadingExport readingExport = new()
                                {
                                    WaterReadingExportID = item.WaterReadingExportID,
                                    MonthID = item.MonthID,
                                    Year = item.Year,

                                };

                                await dbContext.Database.InsertAsync(readingExport);
                            }

                            //Insert Non-Existing Customers
                            if (newCustomersToInsert.Any())
                            {
                                LatestCustomerList.Clear();
                                LatestCustomerList.AddRange(newCustomersToInsert);
                                // Insert the new items into the SQLite database
                                //var response2 = await dbContext.Database.InsertAllAsync(newItemsToInsert);


                                List<Customer> CustomerList = new();
                                foreach (var item in newCustomersToInsert)
                                {
                                    Customer customer = new()
                                    {
                                        CUSTNMBR = item.CUSTNMBR,
                                        CUSTNAME = item.CUSTNAME,
                                        CUSTCLAS = item.CUSTCLAS,
                                        PHONE1 = item.PHONE1,
                                        STATE = item.STATE,
                                        ZIP = item.ZIP,
                                    };
                                    CustomerList.Clear();
                                    CustomerList.Add(customer);
                                }
                                await dbContext.Database.InsertAllAsync(CustomerList);
                            }

                            //Insert Non-Existing Readings
                            if (newReadingToInsert.Any())
                            {
                                // Clear the existing LatestReadingList and add the new readings
                                LatestReadingList.Clear();
                                LatestReadingList.AddRange(newReadingToInsert);

                                // Create a list to store Reading objects
                                List<Reading> ReadingList = new List<Reading>();

                                // Loop through newReadingToInsert and create Reading objects
                                foreach (var item in newReadingToInsert)
                                {
                                    Reading reading = new Reading
                                    {
                                        WaterReadingExportDataID = item.WaterReadingExportDataID,
                                        CURRENT_READING = item.CURRENT_READING,
                                        PREVIOUS_READING = item.PREVIOUS_READING,
                                        Comment = item.Comment, // Set Comment from item if available
                                        ReadingDate = item.ReadingDate, // Set READING_DATE from item if available
                                        MonthID = item.MonthID,
                                        Year = item.Year
                                    };

                                    ReadingList.Add(reading); // Add each reading to the list
                                }

                                // Insert the list of readings into the SQLite database
                                await dbContext.Database.InsertAllAsync(ReadingList);
                            }


                            //Update Existing Readings
                            // Update Existing Readings
                            if (newReadingToUpdate.Any())
                            {
                                List<Reading> ReadingList = new List<Reading>();

                                //Get the latest export items to assign  to readings during update
                                var lastExportItem = await dbContext.Database.Table<ReadingExport>()
                                          .OrderByDescending(r => r.WaterReadingExportID)
                                          .FirstOrDefaultAsync();

                                foreach (var item in newReadingToUpdate)
                                {
                                    // Retrieve the record to update
                                    Reading recordToUpdate = await dbContext.Database.Table<Reading>()
                                        .Where(r => r.WaterReadingExportDataID == item.WaterReadingExportDataID)
                                        .FirstOrDefaultAsync();

                                    if (recordToUpdate != null)
                                    {
                                        // Update the properties of the record
                                        recordToUpdate.WaterReadingExportID = lastExportItem.WaterReadingExportID;
                                        recordToUpdate.CUSTOMER_NUMBER = item.CUSTOMER_NUMBER;
                                        recordToUpdate.CUSTOMER_NAME = item.CUSTOMER_NAME;
                                        recordToUpdate.AREA = item.AREA?.Trim();
                                        recordToUpdate.PHONE1 = item.PHONE1;
                                        recordToUpdate.CUSTOMER_ZONING = item.CUSTOMER_ZONING;
                                        recordToUpdate.ERF_NUMBER = item.ERF_NUMBER;
                                        recordToUpdate.RouteNumber = item.RouteNumber;
                                        recordToUpdate.METER_NUMBER = item.METER_NUMBER;
                                        recordToUpdate.CURRENT_READING = item.CURRENT_READING;
                                        recordToUpdate.PREVIOUS_READING = item.PREVIOUS_READING;
                                        recordToUpdate.ReadingSync = false;
                                        recordToUpdate.ReadingTaken = false;
                                        recordToUpdate.Comment = string.Empty;
                                        //recordToUpdate.READING_DATE = string.Empty;
                                        recordToUpdate.MonthID = lastExportItem.MonthID;
                                        recordToUpdate.Year = lastExportItem.Year;

                                        ReadingList.Add(recordToUpdate);
                                    }
                                }

                                // Update the records in the database
                                await dbContext.Database.UpdateAllAsync(ReadingList);
                                await Shell.Current.DisplayAlert("Done!", $"finished downloading new data!", "OK");
                                Page pg = await Shell.Current.Navigation.PopAsync();
                                 Shell.Current.Navigation.RemovePage(pg);
                                await Shell.Current.GoToAsync($"{nameof(UncapturedReadingsPage)}");
                            }
                        }
                        else
                        {
                            //await Shell.Current.DisplayAlert("No New Exports Found!", $"You Can Proceed Using The App! ", "OK");
                            string tstMsg = "No New Exports Found! You Can Proceed Using The App! ";
                            Toast.Make(tstMsg, CommunityToolkit.Maui.Core.ToastDuration.Long, 10).Show();
                        }
                    }

                }
                catch (Exception ex)
                {
                    // Handle any other exception that might occur during the API call
                    StatusMessage = $"Error." + ex.Message;
                }
            }

            // Return the ReadingExport list, even if it's null (client code should handle this)
            return;
        }


        List<ReadingExport> latestExport { get; set; } = new List<ReadingExport>(); // Initialize the list

        public async Task<List<ReadingExport>> CheckForNewExportInSql()
        {
            if (latestExport.Count == 0)
            {
                try
                {

                    //if (connectivity.NetworkAccess != NetworkAccess.Internet)
                    //{
                    //    await Shell.Current.DisplayAlert("No connectivity!",
                    //        $"Please check internet and try again.", "OK");
                    //    return null;
                    //}
                    //Get lists from APi
                    var responseSql1 = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.ReadingExport);


                    if (responseSql1.IsSuccessStatusCode)
                    {
                        // Read the response content as a string
                        var responseContent1 = await responseSql1.Content.ReadAsStringAsync();


                        // Deserialize the response content into the List<Customer>
                        var newApiReadingExports = JsonConvert.DeserializeObject<List<ReadingExport>>(responseContent1);


                        // Filter the items to get only the ones that do not exist in the SQLite database
                        var newExportToInsert = newApiReadingExports.ToList();


                        //Insert Non-Existing Exports
                        if (newExportToInsert.Any())
                        {
                            LatestExportList.Clear();
                            LatestExportList.AddRange(newExportToInsert);
                            // Insert the new items into the SQLite database
                            //var response2 = await dbContext.Database.InsertAllAsync(newItemsToInsert);

                            foreach (var item in LatestExportList)
                            {
                                ReadingExport readingExport = new()
                                {
                                    WaterReadingExportID = item.WaterReadingExportID,
                                    MonthID = item.MonthID,
                                    Year = item.Year,

                                };

                                await dbContext.Database.InsertAsync(readingExport);
                            }
                        }
                        else { return null; };


                    }

                }
                catch (Exception ex)
                {
                    // Handle any other exception that might occur during the API call
                    StatusMessage = $"Error." + ex.Message;
                }

            }

            // Return the ReadingExport list, even if it's null (client code should handle this)
            return LatestExportList;
        }



        public async Task FlushAndSeed()
        {
            
            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("Failed to recycle readings!",
                    $"Please ensure connectivity and try again.", "OK");
                return;
            }
            var i = await PendingNotSyncedReadings();
            if (i >0)
            {
                await Shell.Current.DisplayAlert($"{i} reading(s) not uploaded!",
                    $"Please sync the pending readings,and try again!", "OK");
                return;
            }

            await Shell.Current.GoToAsync($"{nameof(SynchronizationPage)}");
            try
            {
                //await customerService.GetListOfCustomerFromSql();
                #region deleting existing db data
                List< ReadingExport > result1 = await dbContext.Database.Table<ReadingExport>().Where(i => i.WaterReadingExportID > 0).ToListAsync();
                List<Reading> result2 = await dbContext.Database.Table<Reading>().Where(i => i.Id > 0).ToListAsync();
                List<Customer> result3 = await dbContext.Database.Table<Customer>().Where(i => i.CUSTNMBR != null).ToListAsync();
                List<Month> result4 = await dbContext.Database.Table<Month>().Where(i => i.MonthID >0).ToListAsync();
                List<ReadingMedia> result5 = await dbContext.Database.Table<ReadingMedia>().Where(i=>i.Id>0).ToListAsync();

                if (result1.Count>0 || result1.Any())
                {
                    await dbContext.Database.Table<ReadingExport>().DeleteAsync(r => r.WaterReadingExportID>0);
                }

                if (result2.Count > 0 || result2.Any())
                {
                    await dbContext.Database.Table<Reading>().DeleteAsync(r => r.Id > 0);
                }

                if (result3.Count > 0 || result3.Any())
                {
                    await dbContext.Database.Table<Customer>().DeleteAsync(r => r.CUSTNMBR != null);
                }

                if (result4.Count > 0 || result4.Any())
                {
                    await dbContext.Database.Table<Month>().DeleteAsync(r => r.MonthID >0);
                }

                if (result5.Count > 0 || result5.Any())
                {
                    await dbContext.Database.Table<ReadingMedia>().DeleteAsync(r => r.Id>0);
                }

                #endregion
                await ScanNewLocationsFromSql();
                await customerService.GetListOfCustomerFromSql();
                await CheckForNewExportInSql();

                #region Getting the latest export values(Id,Month & Year)
                var latestExportItem = await dbContext.Database.Table<ReadingExport>()
               .OrderByDescending(r => r.WaterReadingExportID)
               .FirstOrDefaultAsync();


                // If current month is January, adjust to December of previous year
                int currentExportId = latestExportItem.WaterReadingExportID;
                int currentMonthId = latestExportItem.MonthID;
                int currentYearId = latestExportItem.Year;
                if (currentMonthId == 0)
                {
                    currentMonthId = 12;
                    latestExportItem.Year -= 1;
                }
                #endregion



                List<Reading> GeneratedReadings = new();
                List<Customer> allCustomers = await dbContext.Database.Table<Customer>().ToListAsync();

                foreach (var customer in allCustomers)
                {
                    var existingReading = await dbContext.Database.Table<Reading>()
                        .Where(r => r.CUSTOMER_NUMBER == customer.CUSTNMBR)
                        .FirstOrDefaultAsync();

                    if (existingReading == null)
                    {
                        var readingFaker = new ReadingFaker();
                        var reading = readingFaker.Generate(1).FirstOrDefault();

                        reading.CUSTOMER_NUMBER = customer.CUSTNMBR;
                        reading.CUSTOMER_NAME = customer.CUSTNAME;
                        reading.ERF_NUMBER = customer.ZIP;
                        reading.PHONE1 = customer?.PHONE1;
                        reading.AREA = customer.STATE;
                        reading.CUSTOMER_ZONING = customer.CUSTCLAS;
                        reading.CURRENT_READING = 0;
                        reading.Comment = string.Empty;
                        reading.MonthID = currentMonthId;
                        //reading.Year = await _readingService.GetLatestExportItemYear() ?? reading.Year;
                        //reading.READING_DATE = DateTime.UtcNow.ToLongDateString();
                        reading.ReadingTaken = false;
                        reading.ReadingNotTaken = true;
                        reading.ReadingSync = false;
                        reading.WaterReadingExportID = currentExportId;
                        reading.METER_READER = string.Empty;


                        GeneratedReadings.Add(reading);
                    }


                }
                await dbContext.Database.InsertAllAsync(GeneratedReadings);

                await readingService.GetListOfPrevMonthReadingFromSql();
                await Shell.Current.DisplayAlert("Successful sync","Readings were restored successfully","OK");
                await GoBackAsync();
            }
            catch(Exception ex)
            {
                StatusMessage = ex.Message;
            }
            
        }
        #endregion

        public async Task<int> PendingNotSyncedReadings()
        {
            try
            {
                var r = await dbContext.Database.Table<Reading>()
                        .Where(((r=>r.ReadingSync == false
                        && r.ReadingTaken == true && r.CURRENT_READING >= 0 
                        && r.WaterReadingExportDataID > 0 || (r.AreaUpdated ==true && r.ReadingSync==false))))
                        .OrderBy(r => r.ReadingDate).ToListAsync();

                if (r.Count > 0)
                {
                    return r.Count;
                }
                else if(r.Count == 0)
                {
                    return 0;
                }
                else { return 0; }
            }
            catch(Exception ex)
            {
                StatusMessage = $"An error was encountered";
                return 0;
            }
        }

        #region GetLatestExportItemIntoSqlite
        public async Task GetLatestExportItemIntoSqlite()
        {
            var readingsCount = await dbContext.Database
                       .Table<ReadingExport>()
                       .Where(c => c.WaterReadingExportID >= 1)
                       .ToListAsync();

            if (readingsCount.Count > 0)
            {
                // Retrieve all the IDs of the existing ReadingExport items in the SQLite database
                var existingIds = readingsCount
                    .Select(r => r.WaterReadingExportID)
                    .ToList();

                var response = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.ReadingExport);

                if (response.IsSuccessStatusCode)
                {
                    // Read and deserialize the response to a List<ReadingExport>
                    var newReadingExports = await response.Content.ReadFromJsonAsync<List<ReadingExport>>();

                    // Filter the new ReadingExport items to get only the ones that do not exist in the SQLite database
                    var newItemsToInsert = newReadingExports
                        .Where(r => !existingIds.Contains(r.WaterReadingExportID))
                        .ToList();

                    if (newItemsToInsert.Any())
                    {
                        latestExport.Clear();
                        latestExport.AddRange(newItemsToInsert);
                        // Insert the new items into the SQLite database
                        //var response2 = await dbContext.Database.InsertAllAsync(newItemsToInsert);

                        foreach (var item in latestExport)
                        {
                            var ExportItem = new ReadingExport
                            {
                                WaterReadingExportID = item.WaterReadingExportID,
                                MonthID = item.MonthID,
                                Year = item.Year,
                                SALSTERR = item.SALSTERR,
                            };

                            await dbContext.Database.InsertAsync(ExportItem);

                        }
                    }
                }
            }
        }
        #endregion

        #region DeleteOldReadings
        public async Task DeleteOldReadings()
        {
            #region Getting the latest export values(Id,Month & Year)
            var latestExportItem = await dbContext.Database.Table<ReadingExport>()
                       .OrderByDescending(r => r.WaterReadingExportID)
                       .FirstOrDefaultAsync();

            var latestExportMonthItem = await dbContext.Database.Table<ReadingExport>()
                   .OrderByDescending(r => r.MonthID)
                   .FirstOrDefaultAsync();

            var latestExportYearItem = await dbContext.Database.Table<ReadingExport>()
                   .OrderByDescending(r => r.Year)
                   .FirstOrDefaultAsync();

            // If current month is January, adjust to December of previous year
            int currentExportId = latestExportItem.WaterReadingExportID;
            int currentMonthId = latestExportMonthItem.MonthID;
            int currentYearId = latestExportYearItem.Year;
            if (currentMonthId == 0)
            {
                currentMonthId = 12;
                latestExportYearItem.Year -= 1;
            }
            #endregion

            List<Reading> oldRecords = await dbContext.Database.Table<Reading>().Where(r => r.WaterReadingExportDataID != currentExportId).ToListAsync();

            foreach (var item in oldRecords)
            {
                await dbContext.Database.DeleteAsync(item);
            }
        }
        #endregion

        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("../..");
        }
        //End of function 

        public async Task<List<Reading>> ScanNewLocationsFromSql()
        {
            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("Failed to scan for new locations!",
                    $"Please ensure connectivity and try again.", "OK");
                return null;
            }
            try
            {
                //Get lists from APi
                var responseSql = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.GetLocation);

                //Get Lists if readings 
                var locationsList = await dbContext.Database.Table<BillingLocation>().ToListAsync();


                //Readings by Customer Number's
                var existingLocationNo = locationsList
                       .Select(r => r.Location)
                       .ToList();

                if (responseSql.IsSuccessStatusCode)
                {
                    // Read the response content as a string
                    var responseContent = await responseSql.Content.ReadAsStringAsync();

                    // Deserialize the response content into the List<Customer>
                    var newApiLocations = JsonConvert.DeserializeObject<List<BillingLocation>>(responseContent);


                    var newLocations = newApiLocations
                            .Where(r => !existingLocationNo.Contains(r.Location) /*&& r.WaterReadingExportID == currentExportId*/)
                            .ToList();
                    if (newLocations.Any())
                    {
                        foreach (var location in newLocations)
                        {
                            await dbContext.Database.InsertAsync(new BillingLocation
                            {
                                BillingLocationID = location.BillingLocationID,
                                Location = location.Location,
                                Township = location.Township
                            });

                            await dbContext.Database.InsertAsync(location);
                        }
                        string tstMsg1 = $"{newLocations.Count} found and successfully inserted";
                        await Toast.Make(tstMsg1, CommunityToolkit.Maui.Core.ToastDuration.Long, 10).Show();

                    }
                    else
                    {
                        string tstMsg2 = "No new locations found";
                        await Toast.Make(tstMsg2, CommunityToolkit.Maui.Core.ToastDuration.Long, 10).Show();
                    }

                    string tstMsg = "You Can Proceed Using The App! ";
                    await Toast.Make(tstMsg, CommunityToolkit.Maui.Core.ToastDuration.Long, 10).Show();
                    //await Shell.Current.GoToAsync(nameof(UncapturedReadingsPage));
                    return new List<Reading>();
                }
                return new List<Reading>();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                return new List<Reading>();
            }
        }
    }


}