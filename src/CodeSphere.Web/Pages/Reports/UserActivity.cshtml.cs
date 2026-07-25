using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Reports;

public class UserActivityModel : PageModel
{
    private readonly IReportService _reportService;
    private readonly IExportService _exportService;

    public UserActivityModel(IReportService reportService, IExportService exportService)
    {
        _reportService = reportService;
        _exportService = exportService;
    }

    public List<UserActivityReportRow> Rows { get; set; } = new();

    public async Task OnGetAsync()
    {
        Rows = await _reportService.GetUserActivityAsync();
    }

    public async Task<IActionResult> OnGetExportPdfAsync()
    {
        var bytes = await _exportService.ExportUserActivityToPdfAsync();
        return File(bytes, "application/pdf", "UserActivity.pdf");
    }

    public async Task<IActionResult> OnGetExportExcelAsync()
    {
        var bytes = await _exportService.ExportUserActivityToExcelAsync();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "UserActivity.xlsx");
    }
}
