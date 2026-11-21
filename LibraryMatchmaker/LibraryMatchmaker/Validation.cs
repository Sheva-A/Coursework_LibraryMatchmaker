using System.ComponentModel.DataAnnotations;

namespace LibraryMatchmaker;

// Рік від заданого до поточного. Так само перевіряє БД (CK_Authors_birth_year, CK_Books_year)
public class YearRangeAttribute(int min) : ValidationAttribute
{
    public override bool IsValid(object? value) =>
        value is not int year || (year >= min && year <= DateTime.Today.Year);

    public override string FormatErrorMessage(string name) => $"Рік від {min} до {DateTime.Today.Year}";
}

// Дата не пізніше за сьогодні: видача, повернення, реєстрація, відгук
public class NotInFutureAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value is not DateTime date || date.Date <= DateTime.Today;

    public override string FormatErrorMessage(string name) => "Дата не може бути в майбутньому";
}
