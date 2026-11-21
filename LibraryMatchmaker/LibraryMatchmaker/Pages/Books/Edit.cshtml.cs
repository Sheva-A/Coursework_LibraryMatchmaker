using LibraryMatchmaker.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryMatchmaker.Pages.Books;

public class EditModel(BookRepository books, AuthorRepository authors, PublisherRepository publishers,
    GenreRepository genres) : EditPageModel<Book>(books)
{
    public SelectList Authors { get; private set; } = default!;
    public SelectList Publishers { get; private set; } = default!;
    public SelectList Genres { get; private set; } = default!;

    protected override void LoadLists()
    {
        Authors = new SelectList(authors.GetAll(), nameof(Author.AuthorId), nameof(Author.FullName));
        Publishers = new SelectList(publishers.GetAll(), nameof(Publisher.PublisherId), nameof(Publisher.Name));
        Genres = new SelectList(genres.GetAll(), nameof(Genre.GenreId), nameof(Genre.Name));
    }
}
