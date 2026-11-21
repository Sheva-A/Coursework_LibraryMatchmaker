using LibraryMatchmaker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace LibraryMatchmaker.Pages;

// Спільна логіка сторінки зі списком: видалення запису кнопкою «Видалити»
public abstract class ListPageModel<T>(Repository<T> repository) : PageModel
{
    public IActionResult OnPostDelete(int id)
    {
        try
        {
            repository.Delete(id);
            TempData["Message"] = "Запис видалено.";
        }
        catch (SqlException ex)
        {
            // 547 при видаленні: на запис посилаються інші таблиці
            TempData["Error"] = ex.Number == 547 ? DbErrors.InUse : DbErrors.Describe(ex);
        }
        return RedirectToPage();
    }
}

// Спільна логіка сторінки додавання і редагування.
// Без id у адресі сторінка створює новий запис, з id редагує наявний
public abstract class EditPageModel<T>(Repository<T> repository) : PageModel
    where T : class, IEntity, new()
{
    [BindProperty]
    public T Item { get; set; } = new();

    public bool IsNew => Item.Id == 0;

    public IActionResult OnGet(int? id)
    {
        if (id != null)
        {
            var item = repository.GetById(id.Value);
            if (item == null) return NotFound();
            Item = item;
        }
        else
        {
            FillNew();
        }
        LoadLists();
        return Page();
    }

    public IActionResult OnPost()
    {
        if (ModelState.IsValid)
        {
            try
            {
                Save();
                TempData["Message"] = "Зміни збережено.";
                return RedirectToPage("Index");
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", DbErrors.Describe(ex));
            }
        }
        LoadLists();
        return Page();
    }

    // Зберігає запис і повертає його id
    protected virtual int Save()
    {
        if (IsNew) return repository.Add(Item);
        repository.Update(Item);
        return Item.Id;
    }

    // Списки для випадних меню (автори, жанри тощо)
    protected virtual void LoadLists() { }

    // Початкові значення нового запису, наприклад читач з адреси сторінки
    protected virtual void FillNew() { }

    // Число з адреси сторінки (?readerId=2), якщо воно там є
    protected int? QueryInt(string name) =>
        int.TryParse(Request.Query[name], out int value) ? value : null;
}
