namespace LibraryMatchmaker;

// Як показувати значення на сторінках
public static class Fmt
{
    public static string Date(DateTime? date) => date?.ToString("dd.MM.yyyy") ?? "—";

    // Оцінка зірочками: 4 -> ★★★★☆
    public static string Stars(byte? rating) =>
        rating == null ? "—" : new string('★', rating.Value) + new string('☆', 5 - rating.Value);

    public static string Rating(decimal? rating) => rating?.ToString("0.0") ?? "—";

    // Колір позначки статусу видачі
    public static string StatusClass(string status) => status switch
    {
        "Повернено" => "status-returned",
        "Прострочено" => "status-overdue",
        _ => "status-onloan"
    };
}
