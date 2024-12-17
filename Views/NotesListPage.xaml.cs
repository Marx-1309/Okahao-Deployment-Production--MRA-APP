

namespace SampleMauiMvvmApp.Views;

public partial class NotesListPage : ContentPage
{
	public NotesViewModel listViewModel;
    public static List<Notes> UnregReadingsListForSearch { get; private set; } = new List<Notes>();
    public ObservableCollection<Notes> Readings { get; set; } = new ObservableCollection<Notes>();

    private readonly NotesService _notesService;

    public NotesListPage(NotesViewModel _listViewModel)
	{
		InitializeComponent();
		listViewModel = _listViewModel;
		this.BindingContext = listViewModel;
	} 

    protected override void OnAppearing()
    {
        base.OnAppearing();
        listViewModel.GetNotesListCommand.Execute(null);
    }

    [RelayCommand]
    public async Task GetReadingsList()
    {
        Readings.Clear();
        var unregisteredReadingList = await _notesService.GetNotesList();
        if (unregisteredReadingList?.Count > 0)
        {
            unregisteredReadingList = unregisteredReadingList.OrderBy(f => f.NoteTitle).ToList();
            foreach (var reading in unregisteredReadingList)
            {
                Readings.Add(reading);
            }
            UnregReadingsListForSearch.Clear();
            UnregReadingsListForSearch.AddRange(unregisteredReadingList);
        }
    }


    [RelayCommand]
    public async Task AddUpdateUnregReading()
    {
        await AppShell.Current.GoToAsync(nameof(NotesDetailsPage));
    }

    [RelayCommand]
    public async Task EditUnregReading(Notes readingModel)
    {
        var navParam = new Dictionary<string, object>();
        navParam.Add("ReadingDetail", readingModel);
        await AppShell.Current.GoToAsync(nameof(NotesDetailsPage), navParam);
    }

    [RelayCommand]
    public async Task DeleteNote(Notes note)
    {
        var delResponse = await _notesService.DeleteNote(note);
        if (delResponse > 0)
        {
            await GetReadingsList();
        }
    }


    [RelayCommand]
    public async Task DisplayAction(Notes noteModel)
    {
        var response = await AppShell.Current.DisplayActionSheet("Select Option", "OK", null, "Edit", "Delete");
        if (response == "Edit")
        {
            var navParam = new Dictionary<string, object>();
            navParam.Add("ReadingDetail", noteModel);
            await AppShell.Current.GoToAsync(nameof(NotesDetailsPage), navParam);
        }
        else if (response == "Delete")
        {
            var delResponse = await _notesService.DeleteNote(noteModel);
            if (delResponse > 0)
            {
                await GetReadingsList();
            }
        }
    }
}