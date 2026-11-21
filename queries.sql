-- Інформаційно-пошукові запити бази LibraryMatchmaker
-- Ті самі запити використовує застосунок, параметри (@...) передаються з форм.
-- Запускати весь файл: спочатку задаються тестові значення параметрів.

USE LibraryMatchmaker;
GO

DECLARE @search NVARCHAR(100) = N'Кідрук';  -- текст пошуку (назва або автор), NULL = усі книги
DECLARE @genreId INT = NULL;                -- жанр, NULL = усі жанри
DECLARE @onlyAvailable BIT = 0;             -- 1 = лише книги, які зараз є в наявності
DECLARE @bookId INT = 2;                    -- книга для перегляду відгуків
DECLARE @readerId INT = 2;                  -- читач для історії та підбору книг

-- 1. Пошук книг у каталозі за назвою або автором, з фільтром за жанром і наявністю
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
WHERE (@search IS NULL
       OR b.title LIKE N'%' + @search + N'%'
       OR a.first_name + N' ' + a.last_name LIKE N'%' + @search + N'%')
  AND (@genreId IS NULL OR b.genre_id = @genreId)
  AND (@onlyAvailable = 0 OR b.copies_available > 0)
ORDER BY b.title;

-- 2. Відгуки про книгу (від найновіших)
SELECT
    r.first_name + N' ' + r.last_name AS reader,
    rv.rating,
    rv.comment,
    rv.review_date
FROM Reviews rv
JOIN Readers r ON r.reader_id = rv.reader_id
WHERE rv.book_id = @bookId
ORDER BY rv.review_date DESC;

-- 3. Історія видач читача зі статусом кожної видачі та його оцінкою
SELECT
    b.book_id,
    b.title,
    a.first_name + N' ' + a.last_name AS author,
    l.loan_date,
    l.due_date,
    l.return_date,
    CASE
        WHEN l.return_date IS NOT NULL THEN N'Повернено'
        WHEN l.due_date < CAST(GETDATE() AS DATE) THEN N'Прострочено'
        ELSE N'На руках'
    END AS status,
    rv.rating
FROM Loans l
JOIN Books b ON b.book_id = l.book_id
JOIN Authors a ON a.author_id = b.author_id
LEFT JOIN Reviews rv ON rv.reader_id = l.reader_id AND rv.book_id = l.book_id
WHERE l.reader_id = @readerId
ORDER BY l.loan_date DESC, l.loan_id DESC;

-- 4. Боржники: книги, які не повернули вчасно
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
ORDER BY days_overdue DESC;

-- 5. Найпопулярніші книги: кількість видач і середня оцінка
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
ORDER BY loans_count DESC, avg_rating DESC, b.title;

-- 6. Статистика за жанрами: скільки книг, видач, яка середня оцінка
--    і скільки читачів обрали жанр улюбленим (видно, що варто докупити)
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
ORDER BY loans_count DESC, genre;

-- 7. Підбір книг для читача (матчмейкер)
EXEC sp_RecommendBooks @reader_id = @readerId;
