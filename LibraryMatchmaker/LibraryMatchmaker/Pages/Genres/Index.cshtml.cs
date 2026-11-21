using LibraryMatchmaker.Data;

namespace LibraryMatchmaker.Pages.Genres;

public class IndexModel(GenreRepository genres) : ListPageModel<Genre>(genres)
{
    public List<Genre> Genres { get; private set; } = [];

    public void OnGet() => Genres = genres.GetAll();
}
