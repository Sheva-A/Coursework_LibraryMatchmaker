using LibraryMatchmaker.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryMatchmaker.Pages.Reviews;

// Відгук. Оцінити можна лише повернену книгу, це перевіряє тригер у БД
public class EditModel(ReviewRepository reviews, ReaderRepository readers, BookRepository books)
    : EditPageModel<Review>(reviews)
{
    public SelectList Readers { get; private set; } = default!;
    public SelectList Books { get; private set; } = default!;

    // Кнопка «Оцінити» з історії читача передає читача і книгу
    protected override void FillNew()
    {
        Item.ReaderId = QueryInt("readerId") ?? 0;
        Item.BookId = QueryInt("bookId") ?? 0;
    }

    protected override void LoadLists()
    {
        Readers = new SelectList(readers.GetAll(), nameof(Reader.ReaderId), nameof(Reader.FullName));
        Books = new SelectList(books.GetAll(), nameof(Book.BookId), nameof(Book.Title));
    }
}
