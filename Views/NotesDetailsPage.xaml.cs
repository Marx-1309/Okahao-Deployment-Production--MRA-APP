using SampleMauiMvvmApp.ViewModels;

namespace SampleMauiMvvmApp.Views;

[QueryProperty("NoteDetails", "NoteDetails")]
public partial class NotesDetailsPage : ContentPage
{
	public NotesDetailsPage(NotesViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;
	}
}