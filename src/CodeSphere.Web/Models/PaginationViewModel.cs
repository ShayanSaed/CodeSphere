namespace CodeSphere.Web.Models;

/// <summary>
/// Drives the shared "_Pagination" partial used by every paginated list page
/// (home, Articles/Manage, Bookmarks, Users directory) so all of them get
/// the same compact "1 2 3 ... 27 28 29" style pager with Previous/Next,
/// instead of each page reimplementing its own page-number loop.
/// </summary>
public class PaginationViewModel
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }

    /// <summary>The Razor Page to link back to, e.g. "/Index" or "/Articles/Manage/Index".</summary>
    public string PageName { get; set; } = string.Empty;

    /// <summary>All other active filter/sort values to preserve on every page link (PageNumber is added automatically — do not include it here).</summary>
    public Dictionary<string, string> RouteValues { get; set; } = new();
}
