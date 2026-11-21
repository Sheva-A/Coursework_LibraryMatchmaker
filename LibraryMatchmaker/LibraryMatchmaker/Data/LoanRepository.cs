using System.Data;
using Dapper;

namespace LibraryMatchmaker.Data;

public class LoanRepository(IDbConnection db) : Repository<Loan>(db, "Loans", "loan_id", "loan_date DESC")
{
    protected override string InsertSql =>
        "INSERT INTO Loans (reader_id, book_id, loan_date, due_date, return_date) " +
        "VALUES (@ReaderId, @BookId, @LoanDate, @DueDate, @ReturnDate)";

    protected override string UpdateSql =>
        "UPDATE Loans SET reader_id = @ReaderId, book_id = @BookId, loan_date = @LoanDate, " +
        "due_date = @DueDate, return_date = @ReturnDate WHERE loan_id = @LoanId";

    // Статус видачі для списків
    private const string StatusSql =
        """
        CASE
            WHEN l.return_date IS NOT NULL THEN N'Повернено'
            WHEN l.due_date < CAST(GETDATE() AS DATE) THEN N'Прострочено'
            ELSE N'На руках'
        END AS status
        """;

    // Усі видачі з іменами читачів і назвами книг
    public List<LoanListItem> GetList() => Db.Query<LoanListItem>(
        $"""
        SELECT
            l.loan_id,
            r.first_name + N' ' + r.last_name AS reader,
            b.title,
            l.loan_date,
            l.due_date,
            l.return_date,
            {StatusSql}
        FROM Loans l
        JOIN Readers r ON r.reader_id = l.reader_id
        JOIN Books b ON b.book_id = l.book_id
        ORDER BY l.loan_date DESC, l.loan_id DESC
        """).ToList();

    // Запит 3. Історія видач читача зі статусом кожної видачі та його оцінкою
    public List<LoanHistoryItem> GetByReader(int readerId) => Db.Query<LoanHistoryItem>(
        $"""
        SELECT
            b.book_id,
            b.title,
            a.first_name + N' ' + a.last_name AS author,
            l.loan_date,
            l.due_date,
            l.return_date,
            {StatusSql},
            rv.rating
        FROM Loans l
        JOIN Books b ON b.book_id = l.book_id
        JOIN Authors a ON a.author_id = b.author_id
        LEFT JOIN Reviews rv ON rv.reader_id = l.reader_id AND rv.book_id = l.book_id
        WHERE l.reader_id = @readerId
        ORDER BY l.loan_date DESC, l.loan_id DESC
        """, new { readerId }).ToList();

    // Запит 4. Боржники: книги, які не повернули вчасно
    public List<Debtor> GetDebtors() => Db.Query<Debtor>(
        """
        SELECT
            l.loan_id,
            r.first_name + N' ' + r.last_name AS reader,
            r.phone,
            b.title,
            l.loan_date,
            l.due_date,
            DATEDIFF(DAY, l.due_date, CAST(GETDATE() AS DATE)) AS days_overdue
        FROM Loans l
        JOIN Readers r ON r.reader_id = l.reader_id
        JOIN Books b ON b.book_id = l.book_id
        WHERE l.return_date IS NULL
          AND l.due_date < CAST(GETDATE() AS DATE)
        ORDER BY days_overdue DESC
        """).ToList();

    // Повернення книги: сьогоднішня дата. Доступні примірники перерахує тригер
    public void Return(int loanId) => Db.Execute(
        "UPDATE Loans SET return_date = CAST(GETDATE() AS DATE) WHERE loan_id = @loanId AND return_date IS NULL",
        new { loanId });
}
