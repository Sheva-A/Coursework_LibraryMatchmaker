using LibraryMatchmaker.Data;

namespace LibraryMatchmaker.Pages.Reviews;

public class IndexModel(ReviewRepository reviews) : ListPageModel<Review>(reviews)
{
    public List<ReviewListItem> Reviews { get; private set; } = [];

    public void OnGet() => Reviews = reviews.GetList();
}
