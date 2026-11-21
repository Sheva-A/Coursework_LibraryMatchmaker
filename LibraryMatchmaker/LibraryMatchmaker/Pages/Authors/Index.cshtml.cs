using LibraryMatchmaker.Data;

namespace LibraryMatchmaker.Pages.Authors;

public class IndexModel(AuthorRepository authors) : ListPageModel<Author>(authors)
{
    public List<Author> Authors { get; private set; } = [];

    public void OnGet() => Authors = authors.GetAll();
}
