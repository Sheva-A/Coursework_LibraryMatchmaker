using LibraryMatchmaker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryMatchmaker.Pages.Books;

// Каталог з пошуком (запит 1)
public class IndexModel(BookRepository books, GenreRepository genres) : ListPageModel<Book>(books)
{
    // Параметри пошуку з адреси сторінки (?Search=...&GenreId=...&OnlyAvailable=true)
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? GenreId { get; set; }
    [BindProperty(SupportsGet = true)] public bool OnlyAvailable { get; set; }

    public List<BookListItem> Books { get; private set; } = [];
    public SelectList Genres { get; private set; } = default!;

    public void OnGet()
    {
        Books = books.Search(Search, GenreId, OnlyAvailable);
        Genres = new SelectList(genres.GetAll(), nameof(Genre.GenreId), nameof(Genre.Name));
    }
}
