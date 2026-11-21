using LibraryMatchmaker.Data;

namespace LibraryMatchmaker.Pages.Readers;

// Читач разом з улюбленими жанрами (таблиця Reader_Genres)
public class EditModel(ReaderRepository readers, GenreRepository genres) : EditPageModel<Reader>(readers)
{
    public List<Genre> Genres { get; private set; } = [];

    protected override void LoadLists() => Genres = genres.GetAll();

    protected override int Save()
    {
        int id = base.Save();
        readers.SaveGenres(id, Item.GenreIds);
        return id;
    }
}
