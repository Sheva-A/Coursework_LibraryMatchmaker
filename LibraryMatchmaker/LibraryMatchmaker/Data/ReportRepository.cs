using System.Data;
using Dapper;

namespace LibraryMatchmaker.Data;

// Звіти для сторінки «Статистика»
public class ReportRepository(IDbConnection db)
{
    // Запит 5. Найпопулярніші книги: кількість видач і середня оцінка
    public List<PopularBook> GetPopular() => db.Query<PopularBook>(
        """
        SELECT TOP (5)
            b.book_id,
            b.title,
            a.first_name + N' ' + a.last_name AS author,
            COUNT(l.loan_id) AS loans_count,
            (SELECT CAST(AVG(CAST(rv.rating AS DECIMAL(4,2))) AS DECIMAL(3,1))
             FROM Reviews rv
             WHERE rv.book_id = b.book_id) AS avg_rating
        FROM Books b
        JOIN Authors a ON a.author_id = b.author_id
        LEFT JOIN Loans l ON l.book_id = b.book_id
        GROUP BY b.book_id, b.title, a.first_name, a.last_name
        ORDER BY loans_count DESC, avg_rating DESC, b.title
        """).ToList();

    // Запит 6. Статистика за жанрами: книги, видачі, середня оцінка і кількість читачів,
    // які обрали жанр улюбленим
    public List<GenreStats> GetGenreStats() => db.Query<GenreStats>(
        """
        SELECT
            g.genre_id,
            g.name AS genre,
            (SELECT COUNT(*) FROM Books b WHERE b.genre_id = g.genre_id) AS books_count,
            (SELECT COUNT(*) FROM Loans l
             JOIN Books b ON b.book_id = l.book_id
             WHERE b.genre_id = g.genre_id) AS loans_count,
            (SELECT CAST(AVG(CAST(rv.rating AS DECIMAL(4,2))) AS DECIMAL(3,1))
             FROM Reviews rv
             JOIN Books b ON b.book_id = rv.book_id
             WHERE b.genre_id = g.genre_id) AS avg_rating,
            (SELECT COUNT(*) FROM Reader_Genres rg WHERE rg.genre_id = g.genre_id) AS fans_count
        FROM Genres g
        ORDER BY loans_count DESC, genre
        """).ToList();
}
