using LibraryMatchmaker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LibraryMatchmaker.Pages.Loans;

// Видачі з вкладками: усі, на руках і боржники (запит 4)
public class IndexModel(LoanRepository loans) : ListPageModel<Loan>(loans)
{
    // Вкладка з адреси сторінки: ?filter=active або ?filter=overdue
    [BindProperty(SupportsGet = true)] public string? Filter { get; set; }

    public List<LoanListItem> Loans { get; private set; } = [];
    public List<Debtor> Debtors { get; private set; } = [];

    public void OnGet()
    {
        if (Filter == "overdue")
            Debtors = loans.GetDebtors();
        else
            Loans = loans.GetList().Where(l => Filter != "active" || l.ReturnDate == null).ToList();
    }

    // Кнопка «Повернути»: ставить сьогоднішню дату повернення
    public IActionResult OnPostReturn(int id)
    {
        try
        {
            loans.Return(id);
            TempData["Message"] = "Книгу повернено.";
        }
        catch (SqlException ex)
        {
            TempData["Error"] = DbErrors.Describe(ex);
        }
        return RedirectToPage(new { Filter });
    }
}
