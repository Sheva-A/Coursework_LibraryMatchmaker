using LibraryMatchmaker.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryMatchmaker.Pages.Loans;

// Видача книги. Якщо вільних примірників немає, тригер у БД не дасть зберегти
public class EditModel(LoanRepository loans, ReaderRepository readers, BookRepository books)
    : EditPageModel<Loan>(loans)
{
    public SelectList Readers { get; private set; } = default!;
    public SelectList Books { get; private set; } = default!;

    // Зі сторінки читача кнопка «Видати книгу» передає його id
    protected override void FillNew() => Item.ReaderId = QueryInt("readerId") ?? 0;

    protected override void LoadLists()
    {
        Readers = new SelectList(readers.GetAll(), nameof(Reader.ReaderId), nameof(Reader.FullName));
        Books = new SelectList(books.GetAll(), nameof(Book.BookId), nameof(Book.TitleWithCopies));
    }
}
