using LibraryMatchmaker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryMatchmaker.Pages.Readers;

// Картка читача: улюблені жанри, історія видач (запит 3) і підбір книг (запит 7)
public class DetailsModel(ReaderRepository readers, GenreRepository genres, LoanRepository loans) : PageModel
{
    public Reader Reader { get; private set; } = default!;
    public List<string> FavoriteGenres { get; private set; } = [];
    public List<LoanHistoryItem> History { get; private set; } = [];
    public List<Recommendation> Recommendations { get; private set; } = [];

    public IActionResult OnGet(int id)
    {
        var reader = readers.GetById(id);
        if (reader == null) return NotFound();
        Reader = reader;
        FavoriteGenres = genres.GetAll().Where(g => reader.GenreIds.Contains(g.GenreId)).Select(g => g.Name).ToList();
        History = loans.GetByReader(id);
        Recommendations = readers.Recommend(id);
        return Page();
    }
}
