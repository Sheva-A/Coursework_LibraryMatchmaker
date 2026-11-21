using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace LibraryMatchmaker.Data;

// Перекладає помилки SQL Server на зрозумілі повідомлення для бібліотекаря
public static class DbErrors
{
    public const string InUse = "Не можна видалити: цей запис використовується в інших таблицях.";

    private const string NotEmpty = "Заповніть обов'язкові поля.";
    private const string FewerCopies = "Примірників не може бути менше, ніж зараз видано читачам.";

    // Обмеження з create_database.sql і що означає їх порушення
    private static readonly Dictionary<string, string> Constraints = new()
    {
        ["CK_Authors_names"] = NotEmpty,
        ["CK_Authors_birth_year"] = "Рік народження має бути від 1000 до поточного.",
        ["CK_Publishers_name"] = NotEmpty,
        ["UQ_Publishers_name"] = "Видавництво з такою назвою вже є.",
        ["CK_Genres_name"] = NotEmpty,
        ["UQ_Genres_name"] = "Такий жанр уже є.",
        ["CK_Books_title"] = NotEmpty,
        ["UQ_Books_isbn"] = "Книга з таким ISBN уже є.",
        ["CK_Books_isbn"] = "ISBN має складатися з 13 цифр.",
        ["CK_Books_year"] = "Рік видання має бути від 1450 до поточного.",
        ["CK_Books_copies_total"] = "Кількість примірників не може бути від'ємною.",
        ["CK_Books_copies_available"] = FewerCopies,
        ["CK_Books_copies"] = FewerCopies,
        ["CK_Readers_names"] = NotEmpty,
        ["UQ_Readers_phone"] = "Читач з таким телефоном уже є.",
        ["CK_Readers_phone"] = "Телефон: від 10 до 15 символів, лише цифри, +, дужки, пробіл і дефіс.",
        ["UX_Readers_email"] = "Читач з таким email уже є.",
        ["CK_Readers_email"] = "Некоректний email.",
        ["CK_Readers_registration_date"] = "Дата реєстрації не може бути в майбутньому.",
        ["CK_Loans_loan_date"] = "Дата видачі не може бути в майбутньому.",
        ["CK_Loans_due_date"] = "Термін повернення має бути пізніше за дату видачі.",
        ["CK_Loans_return_date"] = "Дата повернення має бути не раніше за дату видачі і не пізніше за сьогодні.",
        ["UX_Loans_active"] = "Цей читач уже має на руках цю книгу.",
        ["CK_Reviews_rating"] = "Оцінка має бути від 1 до 5.",
        ["CK_Reviews_review_date"] = "Дата відгуку не може бути в майбутньому.",
        ["UQ_Reviews_reader_book"] = "Цей читач уже оцінив цю книгу.",
    };

    public static string Describe(SqlException ex)
    {
        // Помилки з тригерів (THROW 50001, 50002) уже мають зрозумілий текст
        var custom = ex.Errors.Cast<SqlError>().FirstOrDefault(e => e.Number >= 50000);
        if (custom != null) return custom.Message;

        string? name = ConstraintName(ex.Message);
        if (name != null && Constraints.TryGetValue(name, out string? message)) return message;

        // Зовнішній ключ при збереженні: обрано запис, якого вже немає
        if (name != null && name.StartsWith("FK_")) return Msg.Choose + ".";

        return "Помилка бази даних: " + ex.Message;
    }

    // Назва обмеження з тексту помилки SQL Server, наприклад
    // ... CHECK constraint "CK_Books_isbn" ... або ... unique index 'UX_Readers_email' ...
    public static string? ConstraintName(string message)
    {
        var match = Regex.Match(message, @"[""'](?<name>(?:PK|UQ|UX|CK|FK)_\w+)[""']");
        return match.Success ? match.Groups["name"].Value : null;
    }
}
