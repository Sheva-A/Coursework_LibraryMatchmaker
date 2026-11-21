using LibraryMatchmaker.Data;

namespace LibraryMatchmaker.Pages.Publishers;

public class IndexModel(PublisherRepository publishers) : ListPageModel<Publisher>(publishers)
{
    public List<Publisher> Publishers { get; private set; } = [];

    public void OnGet() => Publishers = publishers.GetAll();
}
