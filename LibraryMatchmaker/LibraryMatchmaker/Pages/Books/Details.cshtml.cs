using LibraryMatchmaker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryMatchmaker.Pages.Books;

// Картка книги з відгуками читачів (запит 2)
public class DetailsModel(BookRepository books, ReviewRepository reviews) : PageModel
{
    public BookListItem Book { get; private set; } = default!;
    public List<BookReview> Reviews { get; private set; } = [];

    public IActionResult OnGet(int id)
    {
        var book = books.GetCard(id);
        if (book == null) return NotFound();
        Book = book;
        Reviews = reviews.GetByBook(id);
        return Page();
    }
}
