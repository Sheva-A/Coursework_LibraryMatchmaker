-- Курсова робота: база даних "Книжковий матчмейкер"
-- Бібліотека, яка підбирає читачам книги за їхніми вподобаннями

USE master;
GO

-- Якщо база вже існує, видаляємо її, щоб скрипт можна було запускати повторно
IF DB_ID(N'LibraryMatchmaker') IS NOT NULL
BEGIN
    ALTER DATABASE LibraryMatchmaker SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LibraryMatchmaker;
END;
GO

-- Українська колація: правильне сортування за абеткою (Г, Ґ, Д, ..., І, Ї)
-- і порівняння без урахування регістру (жанр "фентезі" = "Фентезі")
CREATE DATABASE LibraryMatchmaker COLLATE Ukrainian_100_CI_AS;
GO

USE LibraryMatchmaker;
GO

-- Автори
CREATE TABLE Authors (
    author_id  INT IDENTITY(1,1) CONSTRAINT PK_Authors PRIMARY KEY,
    last_name  NVARCHAR(50) NOT NULL,
    first_name NVARCHAR(50) NOT NULL,
    birth_year INT NULL CONSTRAINT CK_Authors_birth_year CHECK (birth_year BETWEEN 1000 AND YEAR(GETDATE())),
    country    NVARCHAR(50) NULL,
    -- Ім'я і прізвище не можуть бути порожніми рядками
    CONSTRAINT CK_Authors_names CHECK (LEN(last_name) > 0 AND LEN(first_name) > 0)
);

-- Видавництва
CREATE TABLE Publishers (
    publisher_id INT IDENTITY(1,1) CONSTRAINT PK_Publishers PRIMARY KEY,
    name         NVARCHAR(100) NOT NULL CONSTRAINT UQ_Publishers_name UNIQUE
                 CONSTRAINT CK_Publishers_name CHECK (LEN(name) > 0),
    city         NVARCHAR(50) NULL,
    country      NVARCHAR(50) NULL
);

-- Жанри (довідник, щоб жанр не писали щоразу текстом по-різному)
CREATE TABLE Genres (
    genre_id INT IDENTITY(1,1) CONSTRAINT PK_Genres PRIMARY KEY,
    name     NVARCHAR(50) NOT NULL CONSTRAINT UQ_Genres_name UNIQUE
             CONSTRAINT CK_Genres_name CHECK (LEN(name) > 0)
);

-- Книги
CREATE TABLE Books (
    book_id          INT IDENTITY(1,1) CONSTRAINT PK_Books PRIMARY KEY,
    title            NVARCHAR(300) NOT NULL CONSTRAINT CK_Books_title CHECK (LEN(title) > 0),
    author_id        INT NOT NULL CONSTRAINT FK_Books_Authors REFERENCES Authors(author_id),
    publisher_id     INT NOT NULL CONSTRAINT FK_Books_Publishers REFERENCES Publishers(publisher_id),
    genre_id         INT NOT NULL CONSTRAINT FK_Books_Genres REFERENCES Genres(genre_id),
    isbn             NVARCHAR(13) NOT NULL CONSTRAINT UQ_Books_isbn UNIQUE
                     CONSTRAINT CK_Books_isbn CHECK (LEN(isbn) = 13 AND isbn NOT LIKE '%[^0-9]%'),
    year_published   INT NULL CONSTRAINT CK_Books_year CHECK (year_published BETWEEN 1450 AND YEAR(GETDATE())),
    copies_total     INT NOT NULL CONSTRAINT DF_Books_copies_total DEFAULT 0
                     CONSTRAINT CK_Books_copies_total CHECK (copies_total >= 0),
    copies_available INT NOT NULL CONSTRAINT DF_Books_copies_available DEFAULT 0
                     CONSTRAINT CK_Books_copies_available CHECK (copies_available >= 0),
    CONSTRAINT CK_Books_copies CHECK (copies_available <= copies_total)
);

-- Читачі
CREATE TABLE Readers (
    reader_id         INT IDENTITY(1,1) CONSTRAINT PK_Readers PRIMARY KEY,
    last_name         NVARCHAR(50) NOT NULL,
    first_name        NVARCHAR(50) NOT NULL,
    -- Телефон: від 10 до 15 символів, лише цифри, +, дужки, пробіл і дефіс
    phone             NVARCHAR(15) NOT NULL CONSTRAINT UQ_Readers_phone UNIQUE
                      CONSTRAINT CK_Readers_phone CHECK (LEN(phone) >= 10 AND phone NOT LIKE '%[^0-9+() -]%'),
    email             NVARCHAR(100) NULL CONSTRAINT CK_Readers_email CHECK (email LIKE '%_@_%._%'),
    registration_date DATE NOT NULL CONSTRAINT DF_Readers_registration_date DEFAULT GETDATE()
                      CONSTRAINT CK_Readers_registration_date CHECK (registration_date <= CAST(GETDATE() AS DATE)),
    CONSTRAINT CK_Readers_names CHECK (LEN(last_name) > 0 AND LEN(first_name) > 0)
);

-- Email необов'язковий, але якщо вказаний, то має бути унікальним
-- (звичайний UNIQUE дозволив би лише одного читача без email)
CREATE UNIQUE INDEX UX_Readers_email ON Readers(email) WHERE email IS NOT NULL;

-- Видачі книг
CREATE TABLE Loans (
    loan_id     INT IDENTITY(1,1) CONSTRAINT PK_Loans PRIMARY KEY,
    reader_id   INT NOT NULL CONSTRAINT FK_Loans_Readers REFERENCES Readers(reader_id),
    book_id     INT NOT NULL CONSTRAINT FK_Loans_Books REFERENCES Books(book_id),
    loan_date   DATE NOT NULL CONSTRAINT DF_Loans_loan_date DEFAULT GETDATE()
                CONSTRAINT CK_Loans_loan_date CHECK (loan_date <= CAST(GETDATE() AS DATE)),
    due_date    DATE NOT NULL,
    return_date DATE NULL,
    CONSTRAINT CK_Loans_due_date CHECK (due_date > loan_date),
    CONSTRAINT CK_Loans_return_date CHECK (return_date >= loan_date AND return_date <= CAST(GETDATE() AS DATE))
);

-- Читач не може мати на руках два примірники однієї книги одночасно
CREATE UNIQUE INDEX UX_Loans_active ON Loans(reader_id, book_id) WHERE return_date IS NULL;

-- Улюблені жанри читача (зв'язок багато-до-багатьох між Readers і Genres)
CREATE TABLE Reader_Genres (
    reader_id INT NOT NULL CONSTRAINT FK_ReaderGenres_Readers REFERENCES Readers(reader_id) ON DELETE CASCADE,
    genre_id  INT NOT NULL CONSTRAINT FK_ReaderGenres_Genres REFERENCES Genres(genre_id) ON DELETE CASCADE,
    CONSTRAINT PK_Reader_Genres PRIMARY KEY (reader_id, genre_id)
);

-- Оцінки та відгуки читачів про прочитані книги
CREATE TABLE Reviews (
    review_id   INT IDENTITY(1,1) CONSTRAINT PK_Reviews PRIMARY KEY,
    reader_id   INT NOT NULL CONSTRAINT FK_Reviews_Readers REFERENCES Readers(reader_id),
    book_id     INT NOT NULL CONSTRAINT FK_Reviews_Books REFERENCES Books(book_id),
    rating      TINYINT NOT NULL CONSTRAINT CK_Reviews_rating CHECK (rating BETWEEN 1 AND 5),
    comment     NVARCHAR(500) NULL,
    review_date DATE NOT NULL CONSTRAINT DF_Reviews_review_date DEFAULT GETDATE()
                CONSTRAINT CK_Reviews_review_date CHECK (review_date <= CAST(GETDATE() AS DATE)),
    CONSTRAINT UQ_Reviews_reader_book UNIQUE (reader_id, book_id)
);
GO

-- Доступні примірники = всього примірників мінус ті, що зараз на руках.
-- Перераховуються після будь-якої зміни у видачах (додали, повернули, відредагували, видалили).
CREATE TRIGGER trg_Loans_SyncCopies ON Loans
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Не можна видати більше примірників, ніж є в бібліотеці
    IF EXISTS (
        SELECT 1
        FROM Books b
        WHERE b.book_id IN (SELECT book_id FROM inserted)
          AND b.copies_total < (SELECT COUNT(*) FROM Loans l
                                WHERE l.book_id = b.book_id AND l.return_date IS NULL)
    )
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50001, N'Немає вільних примірників цієї книги.', 1;
    END;

    UPDATE b
    SET copies_available = b.copies_total - (SELECT COUNT(*) FROM Loans l
                                             WHERE l.book_id = b.book_id AND l.return_date IS NULL)
    FROM Books b
    WHERE b.book_id IN (SELECT book_id FROM inserted UNION SELECT book_id FROM deleted);
END;
GO

-- Якщо додали книгу або змінили загальну кількість примірників, перераховуємо доступні
CREATE TRIGGER trg_Books_SyncCopies ON Books
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT UPDATE(copies_total) RETURN;

    UPDATE b
    SET copies_available = b.copies_total - (SELECT COUNT(*) FROM Loans l
                                             WHERE l.book_id = b.book_id AND l.return_date IS NULL)
    FROM Books b
    WHERE b.book_id IN (SELECT book_id FROM inserted);
END;
GO

-- Оцінити можна лише книгу, яку читач уже брав і повернув не пізніше за дату відгуку
CREATE TRIGGER trg_Reviews_CheckLoan ON Reviews
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE NOT EXISTS (
            SELECT 1 FROM Loans l
            WHERE l.reader_id = i.reader_id
              AND l.book_id = i.book_id
              AND l.return_date <= i.review_date)
    )
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50002, N'Оцінити можна лише книгу, яку читач уже повернув. Дата відгуку не може бути раніше за дату повернення.', 1;
    END;
END;
GO

-- Те саме правило з боку видач: не можна видалити чи змінити видачу так,
-- щоб у наявного відгуку не лишилося поверненої книги
CREATE TRIGGER trg_Loans_KeepReviews ON Loans
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM Reviews rv
        WHERE EXISTS (SELECT 1 FROM deleted d
                      WHERE d.reader_id = rv.reader_id AND d.book_id = rv.book_id)
          AND NOT EXISTS (
            SELECT 1 FROM Loans l
            WHERE l.reader_id = rv.reader_id
              AND l.book_id = rv.book_id
              AND l.return_date <= rv.review_date)
    )
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50003, N'На цю видачу спирається відгук читача. Спочатку видаліть або змініть відгук.', 1;
    END;
END;
GO

-- Підбір книг для читача (сам "матчмейкер").
-- Беремо книги, яких читач ще не брав, і рахуємо відсоток збігу:
--   +50, якщо жанр книги є серед улюблених жанрів читача;
--   +30, якщо автор книги "улюблений" (читач ставив його книгам 4 або 5);
--   до +20 за середню оцінку книги від усіх читачів (оцінка 5 дає 20, оцінка 4 дає 16).
CREATE PROCEDURE sp_RecommendBooks
    @reader_id INT,
    @top INT = 5
AS
BEGIN
    SET NOCOUNT ON;

    WITH FavAuthors AS (
        SELECT DISTINCT b.author_id
        FROM Reviews r
        JOIN Books b ON b.book_id = r.book_id
        WHERE r.reader_id = @reader_id AND r.rating >= 4
    ),
    AvgRatings AS (
        SELECT book_id, CAST(AVG(CAST(rating AS DECIMAL(4,2))) AS DECIMAL(3,1)) AS avg_rating
        FROM Reviews
        GROUP BY book_id
    ),
    Scored AS (
        SELECT
            b.book_id,
            b.title,
            a.first_name + N' ' + a.last_name AS author,
            g.name AS genre,
            ar.avg_rating,
            b.copies_available,
            CASE WHEN EXISTS (SELECT 1 FROM Reader_Genres rg
                              WHERE rg.reader_id = @reader_id AND rg.genre_id = b.genre_id)
                 THEN 50 ELSE 0 END AS genre_points,
            CASE WHEN b.author_id IN (SELECT author_id FROM FavAuthors)
                 THEN 30 ELSE 0 END AS author_points,
            CAST(ROUND(ISNULL(ar.avg_rating, 0) * 4, 0) AS INT) AS rating_points
        FROM Books b
        JOIN Authors a ON a.author_id = b.author_id
        JOIN Genres g ON g.genre_id = b.genre_id
        LEFT JOIN AvgRatings ar ON ar.book_id = b.book_id
        WHERE NOT EXISTS (SELECT 1 FROM Loans l
                          WHERE l.reader_id = @reader_id AND l.book_id = b.book_id)
    )
    SELECT TOP (@top)
        book_id,
        title,
        author,
        genre,
        avg_rating,
        copies_available,
        genre_points + author_points + rating_points AS match_percent,
        CONCAT_WS(N', ',
            CASE WHEN genre_points > 0 THEN N'улюблений жанр' END,
            CASE WHEN author_points > 0 THEN N'улюблений автор' END,
            CASE WHEN avg_rating >= 4 THEN N'високий рейтинг' END) AS reason
    FROM Scored
    ORDER BY match_percent DESC, avg_rating DESC, title;
END;
GO
