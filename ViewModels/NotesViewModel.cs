
namespace SampleMauiMvvmApp.ViewModels
{
    [QueryProperty(nameof(NoteDetails), "NoteDetails")]
    public partial class NotesViewModel : ObservableObject
    {
        [ObservableProperty]
        Notes _noteDetails = new Notes();
        public static List<Notes> NotesListForSearch { get; private set; } = new List<Notes>();
        public ObservableCollection<Notes> Notes { get; set; } = new ObservableCollection<Notes>();

        private readonly NotesService _notesService;
        public NotesViewModel(NotesService notesService)
        {
            _notesService = notesService;
        }

        [RelayCommand]
        public async Task UpsertNote()
        {
            int response = -1;
            if (NoteDetails.NoteID > 0)
            {
                if (NoteDetails.NoteTitle == null)
                {
                    await Shell.Current.DisplayAlert("Empty Field", "Note title not provided.", "OK");
                    return;
                }
                response = await _notesService.UpdateNote(NoteDetails);

            }
            else
            {

                response = await _notesService.AddNote(new Models.Notes
                {
                    Date = DateTime.Now.ToString("dd MMM yyyy h:mm tt"),
                    NoteTitle = NoteDetails.NoteTitle,
                    NoteContent = NoteDetails.NoteContent,
                    Image = NoteDetails.Image
                });

                //bool isReadingExist = await _notesService.CheckExistingNoteListById(NoteDetail.NoteID);
                //if (!isReadingExist)
                //{
                //    if(NoteDetail.NoteTitle ==null || NoteDetail.NoteContent == null)
                //    {
                //        await Shell.Current.DisplayAlert("Empty Fields", "Meter or Erf No not provided.", "OK");
                //        await Shell.Current.DisplayAlert("Heads up!", "Record NOT saved!.", "OK");
                //        return;
                //    }

                //    if (NoteDetail.NoteTitle == null)
                //    {
                //        await Shell.Current.DisplayAlert("Empty Fields", "Current Reading not provided.", "OK");
                //        return;
                //    }
                //    response = await _notesService.AddNote(NoteDetail);
                //}
            }


            if (response > 0)
            {
                await Shell.Current.DisplayAlert("Note Info Saved", "Record Saved", "OK");
                await ClearForm();
                await Task.Delay(1000);
                await GoBackAsync();
            }
            else
            {
                await Shell.Current.DisplayAlert("Heads Up!", "Something went wrong while adding your note", "OK");
                await ClearForm();
            }
        }

        [RelayCommand]
        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("../");
        }



        [RelayCommand]
        async Task ClearForm()
        {
            await Task.Yield();
            NoteDetails.NoteTitle =string.Empty;
            NoteDetails.NoteContent =string.Empty;
        }

        [RelayCommand]
        public async Task GetNotesList()
        {
            Notes.Clear();
            var motesList = await _notesService.GetNotesList();
            if (motesList?.Count > 0)
            {
                motesList = motesList.OrderBy(f => f.Date).ToList();
                foreach (var note in motesList)
                {
                    await Task.Delay(50);
                    Notes.Add(note);
                }
                NotesListForSearch.Clear();
                NotesListForSearch.AddRange(motesList);
            }
        }


        [RelayCommand]
        public async Task AddUpdateNote()
        {
            await AppShell.Current.GoToAsync(nameof(NotesDetailsPage));
        }

        [RelayCommand]
        public async Task EditNote(Notes NoteDetail)
        {
            var navParam = new Dictionary<string, object>();
            navParam.Add("NoteDetails", NoteDetails);
            await AppShell.Current.GoToAsync(nameof(NotesDetailsPage), navParam);
        }

        [RelayCommand]
        public async Task DeleteNote(Notes note)
        {
            var delResponse = await _notesService.DeleteNote(note);
            if (delResponse > 0)
            {
                await GetNotesList();
            }
        }


        [RelayCommand]
        public async Task DisplayAction(Notes note)
        {
            var response = await AppShell.Current.DisplayActionSheet("Select Option", "OK", null, "Edit", "Delete");
            if (response == "Edit")
            {
                var navParam = new Dictionary<string, object>();
                navParam.Add("NoteDetails", note);
                await AppShell.Current.GoToAsync(nameof(NotesDetailsPage), navParam);
            }
            else if (response == "Delete")
            {
                var delResponse = await _notesService.DeleteNote(note);
                if (delResponse > 0)
                {
                    await GetNotesList();
                }
            }
        }

        //public async Task<List<UnregReadings>> CheckForExistingReadings(string meterNo)
        //{
        //    int ReadingValue = 0;

        //    if (!string.IsNullOrEmpty(meterNo))
        //    {
        //        if (int.TryParse(meterNo, out ReadingValue))
        //        {
        //            bool isExistingReading = await _unregReadingService.checkExistingReadingListById(ReadingValue);

        //            if (unregisteredReadingList.Count > 0)
        //            {
        //                await Shell.Current.DisplayAlert("Duplicate Records", "A reading with the same meter no was found!", "OK");
        //            }
        //            else
        //            {
        //                return unregisteredReadingList;
        //            }
        //        }
        //        else
        //        {
        //            // Handle the case where meterNo couldn't be parsed to an int
        //            await Shell.Current.DisplayAlert("Invalid Input", "Please enter a valid meter number.", "OK");
        //        }
        //    }

        //    return new List<UnregReadings>();
        //}

    }
}
