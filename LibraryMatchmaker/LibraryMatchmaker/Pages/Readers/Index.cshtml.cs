using LibraryMatchmaker.Data;

namespace LibraryMatchmaker.Pages.Readers;

public class IndexModel(ReaderRepository readers) : ListPageModel<Reader>(readers)
{
    public List<Reader> Readers { get; private set; } = [];

    public void OnGet() => Readers = readers.GetAll();
}
