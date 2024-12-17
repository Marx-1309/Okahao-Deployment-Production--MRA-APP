
namespace SampleMauiMvvmApp.ModelWrappers
{
    public partial class ReadingWrapper : ObservableObject
    {
        public ReadingWrapper(Reading readingModel)
        {
            if (readingModel != null)
            {
                Id = readingModel.Id;
                WaterReadingExportDataID = (int)readingModel.WaterReadingExportDataID;
                WaterReadingExportId = (int)readingModel.WaterReadingExportID;
                Customer_number = readingModel.CUSTOMER_NUMBER;
                Customer_name = readingModel.CUSTOMER_NAME;
                Area = readingModel.AREA;
                Phone1 = (long)readingModel.PHONE1;
                Erf_number = readingModel.ERF_NUMBER;
                Meter_number = readingModel.METER_NUMBER;
                Current_reading = (long)readingModel.CURRENT_READING;
                Previous_reading = (decimal)readingModel.PREVIOUS_READING;
                MonthID = (int)readingModel.MonthID;
                CurrentMonth = readingModel.CurrentMonth;
                Year = (int)readingModel.Year;
                Customer_zoning = readingModel.CUSTOMER_ZONING;
                RouteNumber = readingModel.RouteNumber;
                MeterReader = readingModel.METER_READER;
                IsFlagged = (bool)readingModel.IsFlagged;
                //ReadingDate = (DateTime)readingModel.READING_DATE;
                Comment = readingModel.Comment;
                ReadingNotTaken = (bool)readingModel.ReadingNotTaken;
                //ReadingTaken = (bool)readingModel.ReadingTaken;
                //ReadingSync = (bool)readingModel.ReadingSync;


            }
        }

        public ReadingWrapper(List<Reading> month)
        {
            this.month = month;
        }
        public int Id;
        public int WaterReadingExportDataID { get; set; }

        [ObservableProperty]
        int waterReadingExportId;
        [ObservableProperty]
        string customer_number;
        [ObservableProperty]
        string customer_name;
        [ObservableProperty]
        string area;
        [ObservableProperty]
        long phone1;
        [ObservableProperty]
        string erf_number;
        [ObservableProperty]
        string meter_number;
        [ObservableProperty]
        string c_reading;
        [ObservableProperty]
        long? current_reading;
        [ObservableProperty]
        decimal previous_reading;
        [ObservableProperty]
        int percentageChange;
        [ObservableProperty]
        int monthID;
        [ObservableProperty]
        string? currentMonth;
        [ObservableProperty]
        int year;
        [ObservableProperty]
        string customer_zoning;
        [ObservableProperty]
        string readingDate;
        [ObservableProperty]
        int customerId;
        [ObservableProperty]
        bool isNew;
        [ObservableProperty]
        int readingsID;
        [ObservableProperty]
        string reading_number;
        [ObservableProperty]
        string meterReader;
        [ObservableProperty]
        string comment;
        [ObservableProperty]
        int waterReadingTypeID;
        [ObservableProperty]
        string routeNumber;
        [ObservableProperty]
        public bool readingTaken;
        [ObservableProperty]
        public bool isFlagged;
        [ObservableProperty]
        public bool readingNotTaken;
        [ObservableProperty]
        bool readingSync;
        [ObservableProperty]
        bool areaUpdated;
        public List<Reading> month;
    }
}
