using System.ComponentModel.DataAnnotations;

namespace LibraryMatchmaker;

// Моделі таблиць БД. Назви властивостей збігаються з колонками без підкреслень:
// author_id -> AuthorId. Це класи з set, бо їх заповнюють форми додавання і редагування.

// Запис з первинним ключем. Потрібен спільній сторінці редагування,
// щоб зрозуміти, новий це запис (Id = 0) чи наявний
public interface IEntity
{
    int Id { get; }
}

// Тексти помилок перевірки форм.
// Числа й дати завжди обов'язкові, але [Required] біля них вказано, щоб браузер
// показував українське повідомлення, а не стандартне англійське
public static class Msg
{
    public const string Required = "Заповніть це поле";
    public const string TooLong = "Не більше {1} символів";
    public const string Choose = "Оберіть значення зі списку";
}

public class Author : IEntity
{
    public int AuthorId { get; set; }

    [Required(ErrorMessage = Msg.Required), StringLength(50, ErrorMessage = Msg.TooLong)]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = Msg.Required), StringLength(50, ErrorMessage = Msg.TooLong)]
    public string FirstName { get; set; } = "";

    [YearRange(1000)]
    public int? BirthYear { get; set; }

    [StringLength(50, ErrorMessage = Msg.TooLong)]
    public string? Country { get; set; }

    public int Id => AuthorId;
    public string FullName => $"{FirstName} {LastName}";
}

public class Publisher : IEntity
{
    public int PublisherId { get; set; }

    [Required(ErrorMessage = Msg.Required), StringLength(100, ErrorMessage = Msg.TooLong)]
    public string Name { get; set; } = "";

    [StringLength(50, ErrorMessage = Msg.TooLong)]
    public string? City { get; set; }

    [StringLength(50, ErrorMessage = Msg.TooLong)]
    public string? Country { get; set; }

    public int Id => PublisherId;
}

public class Genre : IEntity
{
    public int GenreId { get; set; }

    [Required(ErrorMessage = Msg.Required), StringLength(50, ErrorMessage = Msg.TooLong)]
    public string Name { get; set; } = "";

    public int Id => GenreId;
}

public class Book : IEntity
{
    public int BookId { get; set; }

    [Required(ErrorMessage = Msg.Required), StringLength(300, ErrorMessage = Msg.TooLong)]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = Msg.Choose)]
    public int AuthorId { get; set; }

    [Required(ErrorMessage = Msg.Choose)]
    public int PublisherId { get; set; }

    [Required(ErrorMessage = Msg.Choose)]
    public int GenreId { get; set; }

    [Required(ErrorMessage = Msg.Required)]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "ISBN має складатися з 13 цифр без дефісів")]
    public string Isbn { get; set; } = "";

    [YearRange(1450)]
    public int? YearPublished { get; set; }

    [Required(ErrorMessage = Msg.Required), Range(0, 1000, ErrorMessage = "Від 0 до 1000 примірників")]
    public int CopiesTotal { get; set; }

    // Рахує тригер у БД, у формі не редагується
    public int CopiesAvailable { get; set; }

    public int Id => BookId;
    public string TitleWithCopies => $"{Title} (вільно: {CopiesAvailable})";
}

public class Reader : IEntity
{
    public int ReaderId { get; set; }

    [Required(ErrorMessage = Msg.Required), StringLength(50, ErrorMessage = Msg.TooLong)]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = Msg.Required), StringLength(50, ErrorMessage = Msg.TooLong)]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = Msg.Required)]
    [RegularExpression(@"^[0-9+() -]{10,15}$",
        ErrorMessage = "Від 10 до 15 символів: цифри, +, дужки, пробіл і дефіс")]
    public string Phone { get; set; } = "";

    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Некоректний email")]
    [StringLength(100, ErrorMessage = Msg.TooLong)]
    public string? Email { get; set; }

    [Required(ErrorMessage = Msg.Required), NotInFuture]
    public DateTime RegistrationDate { get; set; } = DateTime.Today;

    // Улюблені жанри (таблиця Reader_Genres), заповнюються окремо
    public List<int> GenreIds { get; set; } = [];

    public int Id => ReaderId;
    public string FullName => $"{FirstName} {LastName}";
}

public class Loan : IEntity, IValidatableObject
{
    public int LoanId { get; set; }

    [Required(ErrorMessage = Msg.Choose)]
    public int ReaderId { get; set; }

    [Required(ErrorMessage = Msg.Choose)]
    public int BookId { get; set; }

    [Required(ErrorMessage = Msg.Required), NotInFuture]
    public DateTime LoanDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = Msg.Required)]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(21);

    [NotInFuture]
    public DateTime? ReturnDate { get; set; }

    public int Id => LoanId;

    // Порівняння дат між собою, як у CK_Loans_due_date і CK_Loans_return_date
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (DueDate <= LoanDate)
            yield return new ValidationResult("Термін має бути пізніше за дату видачі", [nameof(DueDate)]);
        if (ReturnDate < LoanDate)
            yield return new ValidationResult("Не раніше за дату видачі", [nameof(ReturnDate)]);
    }
}

public class Review : IEntity
{
    public int ReviewId { get; set; }

    [Required(ErrorMessage = Msg.Choose)]
    public int ReaderId { get; set; }

    [Required(ErrorMessage = Msg.Choose)]
    public int BookId { get; set; }

    [Required(ErrorMessage = Msg.Required), Range(1, 5, ErrorMessage = "Оцінка від 1 до 5")]
    public byte Rating { get; set; } = 5;

    [StringLength(500, ErrorMessage = Msg.TooLong)]
    public string? Comment { get; set; }

    [Required(ErrorMessage = Msg.Required), NotInFuture]
    public DateTime ReviewDate { get; set; } = DateTime.Today;

    public int Id => ReviewId;
}

// Результати запитів. Порядок полів збігається з порядком колонок у SELECT

public record BookListItem(int BookId, string Title, string Author, string Genre, string Publisher,
    string Isbn, int? YearPublished, int CopiesTotal, int CopiesAvailable);

public record BookReview(string Reader, byte Rating, string? Comment, DateTime ReviewDate);

public record LoanListItem(int LoanId, string Reader, string Title, DateTime LoanDate,
    DateTime DueDate, DateTime? ReturnDate, string Status);

public record LoanHistoryItem(int BookId, string Title, string Author, DateTime LoanDate,
    DateTime DueDate, DateTime? ReturnDate, string Status, byte? Rating);

public record Debtor(int LoanId, string Reader, string Phone, string Title, DateTime LoanDate, DateTime DueDate,
    int DaysOverdue);

public record PopularBook(int BookId, string Title, string Author, int LoansCount, decimal? AvgRating);

public record GenreStats(int GenreId, string Genre, int BooksCount, int LoansCount, decimal? AvgRating,
    int FansCount);

public record ReviewListItem(int ReviewId, string Reader, string Title, byte Rating, string? Comment,
    DateTime ReviewDate);

public record Recommendation(int BookId, string Title, string Author, string Genre, decimal? AvgRating,
    int CopiesAvailable, int MatchPercent, string Reason);
