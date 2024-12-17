
namespace SampleMauiMvvmApp.Services
{
    public interface IMonthService
    {
        Task<int?> GetLatestExportItemMonthId();
        Task<string> GetLatestExportItemMonthName();
        Task<List<Month>> GetListOfMonthsFromSql();
        Task<List<Month>> GetListOfMonthsFromSqlite();
        Task<string> GetMonthNameById();
        Task<List<Month>> GetMonths();
        Task<List<Reading>> GetReadingsByMonthIdAsync(int MonthId);
    }

    public partial class MonthService : BaseService, IMonthService
    {
        HttpClient httpClient;
        public MonthService(DbContext dbContext) : base(dbContext)
        {
            this.httpClient = new HttpClient();
        }
        List<Month> MonthList;
        public async Task<List<Month>> GetMonths()
        {
            if (MonthList?.Count > 0)
                return MonthList;
            var response = await httpClient.GetAsync(Constants.GetMonth);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    MonthList = await response.Content.ReadFromJsonAsync<List<Month>>();
                }
                catch (Exception ex)
                {
                    // Handle deserialization error
                    Console.WriteLine("Deserialization error: " + ex.Message);
                }
            }
            return MonthList;
        }

        public async Task<string> GetMonthNameById()
        {
            var latestExportMonthItem = await dbContext.Database.Table<ReadingExport>()
                   .OrderByDescending(r => r.WaterReadingExportID)
                   .FirstOrDefaultAsync();

            int currentMonthId = latestExportMonthItem.MonthID;

            var Month = await dbContext.Database.Table<Month>().FirstOrDefaultAsync(m => m.MonthID == currentMonthId);

            if (Month != null)
            {
                return Month.MonthName;
            }
            else
            {
                return null; // Month not found
            }
        }

        public async Task<string> GetCurrentMonthNameById(int Id)
        {
            if (Id > 0)
            {
                try
                {
                    var month = await dbContext.Database.Table<Month>().Where(m=>m.MonthID == Id).FirstOrDefaultAsync();
                    string monthName = month.MonthName;
                    return monthName;
                }
                catch (Exception ex)
                {
                    return StatusMessage = ex.Message;
                }
            }
            return "";
        }

        List<Reading> ReadingList;
        public async Task<List<Reading>> GetReadingsByMonthIdAsync(int MonthId)
        {
            if (ReadingList?.Count > 0)
            {
                ReadingList.Clear();
            }
            try
            {
                var listOfReadings = await dbContext.Database.Table<Reading>().Where(x => x.MonthID == MonthId).ToListAsync();
                foreach (var item in listOfReadings)
                {
                    ReadingList?.Add(item);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
            }
            return ReadingList;
        }

        List<Month> listMonths;

        public async Task<List<Month>> GetListOfMonthsFromSql()
        {
            if (listMonths == null)
            {
                try
                {
                    var monthsCount = await dbContext.Database.Table<Month>().Where(m => m.MonthID >= 1).ToListAsync();

                    if (monthsCount.Count > 0)
                    {
                        var existingIds = monthsCount.Select(m => m.MonthID).ToList();

                        var response = await httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.GetMonth);

                        if (response.IsSuccessStatusCode)
                        {
                            var newMonths = await response.Content.ReadFromJsonAsync<List<Month>>();

                            // Delete records from SQLite that don't exist in the API response
                            var idsToDelete = existingIds.Except(newMonths.Select(m => m.MonthID)).ToList();
                            await dbContext.Database.Table<Month>().DeleteAsync(m => idsToDelete.Contains(m.MonthID));

                            // Filter the new Month items to get only the ones that do not exist in the SQLite database
                            var newItemsToInsert = newMonths.Where(m => !existingIds.Contains(m.MonthID)).ToList();

                            if (newItemsToInsert.Any())
                            {
                                // Insert the new items into the SQLite database
                                var response2 = await dbContext.Database.InsertAllAsync(newItemsToInsert);

                                // Update the listMonths list to include both existing items and new items
                                foreach (var item in newItemsToInsert)
                                {
                                    listMonths?.Add(item);
                                }
                            }
                        }
                        else
                        {
                            // Handle unsuccessful response, maybe throw an exception or log an error
                            StatusMessage = $"Failed :." + response.StatusCode;
                        }
                    }
                    else
                    {
                        var response = await httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.GetMonth);

                        if (response.IsSuccessStatusCode)
                        {
                            listMonths = await response.Content.ReadFromJsonAsync<List<Month>>();
                            foreach (var item in listMonths)
                            {
                                item.MonthID = item.MonthID;
                                item.MonthName = item.MonthName.Trim();

                                var response2 = await dbContext.Database.InsertAsync(item);
                            }

                        }
                        else
                        {
                            // Handle unsuccessful response, maybe throw an exception or log an error
                            StatusMessage = $"Failed :." + response.StatusCode;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any other exception that might occur during the API call
                    StatusMessage = $"Error." + ex.Message;
                }
            }

            // Return the Month list, even if it's null (client code should handle this)
            return listMonths;
        }

        public async Task<List<Month>> GetListOfMonthsFromSqlite()
        {
            try
            {
                var listOfMonths = await dbContext.Database.Table<Month>().ToListAsync();

                // Check if the count is 0 before calling GetListOfMonthsFromSql()
                if (listOfMonths.Count == 0)
                {
                    await GetListOfMonthsFromSql();
                    listOfMonths = await dbContext.Database.Table<Month>().ToListAsync();
                }

                // Check again if the count is greater than 0 after calling GetListOfMonthsFromSql()
                if (listOfMonths.Count > 0)
                {
                    return listOfMonths;
                }
                else
                {
                    // Handle the case when both attempts to retrieve months were unsuccessful
                    StatusMessage = $"Failed to retrieve months from the database.";
                }
            }
            catch (Exception ex)
            {
                // Handle any other exception that might occur during the database call
                StatusMessage = $"Error: {ex.Message}";
            }

            return null;
        }


        public async Task<string?> GetLatestExportItemMonthName()
        {
            try
            {
                var lastItem = await dbContext.Database.Table<Month>()
                    .OrderByDescending(r => r.MonthName)
                    .FirstOrDefaultAsync();

                // If the lastItem is not null, return its ID; otherwise, return null.
                return lastItem?.MonthName;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the database operation
                StatusMessage = $"Error: {ex.Message}";
                return null;
            }
        }

        public async Task<int?> GetLatestExportItemMonthId()
        {
            try
            {
                var lastItem = await dbContext.Database.Table<ReadingExport>()
                    .OrderByDescending(r => r.WaterReadingExportID)
                    .FirstOrDefaultAsync();

                // If the lastItem is not null, return its ID; otherwise, return null.
                return lastItem?.MonthID;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the database operation
                StatusMessage = $"Error: {ex.Message}";
                return null;
            }
        }

        public async Task<bool> IsMonthPopulated(Month month)
        {
            try
            {
                int isWithReadings = await  dbContext.Database.Table<Reading>().Where(r=>r.MonthID == month.MonthID).CountAsync();
                // If the lastItem is not null, return its ID; otherwise, return null.
                if(isWithReadings>0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the database operation
                StatusMessage = $"Error: {ex.Message}";
                return false;
            }
        }
    }
}
