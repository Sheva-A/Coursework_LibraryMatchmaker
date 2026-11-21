using LibraryMatchmaker.Data;

namespace LibraryMatchmaker.Pages.Publishers;

public class EditModel(PublisherRepository publishers) : EditPageModel<Publisher>(publishers);
