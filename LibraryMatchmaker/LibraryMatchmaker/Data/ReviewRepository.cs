using System.Data;
using Dapper;

namespace LibraryMatchmaker.Data;

public class ReviewRepository(IDbConnection db)
    : Repository<Review>(db, "Reviews", "review_id", "review_date DESC")
{
    protected override string InsertSql =>
        "INSERT INTO Reviews (reader_id, book_id, rating, comment, review_date) " +
        "VALUES (@ReaderId, @BookId, @Rating, @Comment, @ReviewDate)";

    protected override string UpdateSql =>
        "UPDATE Reviews SET reader_id = @ReaderId, book_id = @BookId, rating = @Rating, " +
        "comment = @Comment, review_date = @ReviewDate WHERE review_id = @ReviewId";

    // Усі відгуки з іменами читачів і назвами книг
    public List<ReviewListItem> GetList() => Db.Query<ReviewListItem>(
        """
        SELECT
            rv.review_id,
            r.first_name + N' ' + r.last_name AS reader,
            b.title,
            rv.rating,
            rv.comment,
            rv.review_date
        FROM Reviews rv
        JOIN Readers r ON r.reader_id = rv.reader_id
        JOIN Books b ON b.book_id = rv.book_id
        ORDER BY rv.review_date DESC
        """).ToList();

    // Запит 2. Відгуки про книгу, від найновіших
    public List<BookReview> GetByBook(int bookId) => Db.Query<BookReview>(
        """
        SELECT
            r.first_name + N' ' + r.last_name AS reader,
            rv.rating,
            rv.comment,
            rv.review_date
        FROM Reviews rv
        JOIN Readers r ON r.reader_id = rv.reader_id
        WHERE rv.book_id = @bookId
        ORDER BY rv.review_date DESC
        """, new { bookId }).ToList();
}
