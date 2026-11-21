using LibraryMatchmaker.Data;

namespace LibraryMatchmaker.Pages.Genres;

public class EditModel(GenreRepository genres) : EditPageModel<Genre>(genres);
