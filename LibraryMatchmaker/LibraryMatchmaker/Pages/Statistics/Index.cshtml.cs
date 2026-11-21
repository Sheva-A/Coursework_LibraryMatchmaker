using LibraryMatchmaker.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryMatchmaker.Pages.Statistics;

// Найпопулярніші книги (запит 5) і статистика за жанрами (запит 6)
public class IndexModel(ReportRepository reports) : PageModel
{
    public List<PopularBook> Popular { get; private set; } = [];
    public List<GenreStats> Genres { get; private set; } = [];

    public void OnGet()
    {
        Popular = reports.GetPopular();
        Genres = reports.GetGenreStats();
    }
}
