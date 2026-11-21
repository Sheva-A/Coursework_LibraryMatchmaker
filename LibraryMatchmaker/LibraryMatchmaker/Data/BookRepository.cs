using System.Data;
using Dapper;

namespace LibraryMatchmaker.Data;

public class BookRepository(IDbConnection db) : Repository<Book>(db, "Books", "book_id", "title")
{
    // copies_available не вказуємо: його рахує тригер з copies_total і виданих примірників
    protected override string InsertSql =>
        "INSERT INTO Books (title, author_id, publisher_id, genre_id, isbn, year_published, copies_total) " +
        "VALUES (@Title, @AuthorId, @PublisherId, @GenreId, @Isbn, @YearPublished, @CopiesTotal)";

    // copies_available рахуємо тут же: SQL Server перевіряє CHECK раніше, ніж спрацює тригер,
    // і без цього не можна було б зменшити кількість примірників
    protected override string UpdateSql =>
        "UPDATE Books SET title = @Title, author_id = @AuthorId, publisher_id = @PublisherId, " +
        "genre_id = @GenreId, isbn = @Isbn, year_published = @YearPublished, copies_total = @CopiesTotal, " +
        "copies_available = @CopiesTotal - (SELECT COUNT(*) FROM Loans l " +
        "WHERE l.book_id = @BookId AND l.return_date IS NULL) " +
        "WHERE book_id = @BookId";

    // Книга разом з назвами автора, жанру і видавництва
    private const string CatalogSql =
        """
        SELECT
            b.book_id,
            b.title,
            a.first_name + N' ' + a.last_name AS author,
            g.name AS genre,
            p.name AS publisher,
            b.isbn,
            b.year_published,
            b.copies_total,
            b.copies_available
        FROM Books b
        JOIN Authors a ON a.author_id = b.author_id
        JOIN Genres g ON g.genre_id = b.genre_id
        JOIN Publishers p ON p.publisher_id = b.publisher_id
        """;

    // Запит 1. Пошук у каталозі за назвою або автором, з фільтром за жанром і наявністю.
    // Символи [, % і _ з поля пошуку екрануємо, щоб LIKE шукав їх буквально
    public List<BookListItem> Search(string? search, int? genreId, bool onlyAvailable) =>
        Db.Query<BookListItem>(CatalogSql +
            """

            WHERE (@search IS NULL
                   OR b.title LIKE N'%' + @search + N'%'
                   OR a.first_name + N' ' + a.last_name LIKE N'%' + @search + N'%')
              AND (@genreId IS NULL OR b.genre_id = @genreId)
              AND (@onlyAvailable = 0 OR b.copies_available > 0)
            ORDER BY b.title
            """, new { search = EscapeLike(search), genreId, onlyAvailable }).ToList();

    private static string? EscapeLike(string? text) =>
        text?.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

    public BookListItem? GetCard(int id) =>
        Db.QuerySingleOrDefault<BookListItem>(CatalogSql + " WHERE b.book_id = @id", new { id });
}
