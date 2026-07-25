using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Reports;

public class TrendingModel : PageModel
{
    private readonly IReportService _reportService;
    private readonly IExportService _exportService;

    public TrendingModel(IReportService reportService, IExportService exportService)
    {
        _reportService = reportService;
        _exportService = exportService;
    }

    public List<TrendingArticleReportRow> Rows { get; set; } = new();

    public async Task OnGetAsync()
    {
        Rows = await _reportService.GetTrendingArticlesAsync(0);
    }

    public async Task<IActionResult> OnGetExportPdfAsync()
    {
        var bytes = await _exportService.ExportTrendingArticlesToPdfAsync();
        return File(bytes, "application/pdf", "TrendingArticles.pdf");
    }

    public async Task<IActionResult> OnGetExportExcelAsync()
    {
        var bytes = await _exportService.ExportTrendingArticlesToExcelAsync();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TrendingArticles.xlsx");
    }
}
