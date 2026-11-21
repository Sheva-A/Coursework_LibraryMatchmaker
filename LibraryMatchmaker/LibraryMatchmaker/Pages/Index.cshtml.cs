using LibraryMatchmaker.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryMatchmaker.Pages;

// Головна: вибір читача для підбору книг
public class IndexModel(ReaderRepository readers) : PageModel
{
    public SelectList Readers { get; private set; } = default!;

    public void OnGet() =>
        Readers = new SelectList(readers.GetAll(), nameof(Reader.ReaderId), nameof(Reader.FullName));
}
