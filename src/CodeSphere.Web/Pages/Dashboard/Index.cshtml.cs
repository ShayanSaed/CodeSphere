using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly IReportService _reportService;
    public IndexModel(IReportService reportService) => _reportService = reportService;

    public DashboardStatsDto Stats { get; set; } = new();

    public async Task OnGetAsync()
    {
        Stats = await _reportService.GetDashboardStatsAsync();
    }

    // AJAX/Fetch handler used to refresh the charts without a full reload.
    public async Task<JsonResult> OnGetStatsJsonAsync()
    {
        var stats = await _reportService.GetDashboardStatsAsync();
        return new JsonResult(stats);
    }
}
