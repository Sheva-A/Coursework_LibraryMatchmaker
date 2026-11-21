using LibraryMatchmaker.Data;

namespace LibraryMatchmaker.Pages.Authors;

public class EditModel(AuthorRepository authors) : EditPageModel<Author>(authors);
