using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSphere.Api.Controllers;

/// <summary>
/// Serves the two mandatory print-ready reports (requirement #9) as JSON
/// for API consumers, plus PDF/Excel export endpoints (bonus).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IExportService _exportService;

    public ReportsController(IReportService reportService, IExportService exportService)
    {
        _reportService = reportService;
        _exportService = exportService;
    }

    /// <summary>GET /api/reports/trending?top=20</summary>
    [HttpGet("trending")]
    [AllowAnonymous]
    public async Task<ActionResult<List<TrendingArticleReportRow>>> GetTrending([FromQuery] int top = 20) =>
        Ok(await _reportService.GetTrendingArticlesAsync(top));

    /// <summary>GET /api/reports/user-activity</summary>
    [HttpGet("user-activity")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<List<UserActivityReportRow>>> GetUserActivity() =>
        Ok(await _reportService.GetUserActivityAsync());

    /// <summary>GET /api/reports/dashboard — aggregate stats for admin charts.</summary>
    [HttpGet("dashboard")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboard() =>
        Ok(await _reportService.GetDashboardStatsAsync());

    /// <summary>GET /api/reports/trending/export/pdf</summary>
    [HttpGet("trending/export/pdf")]
    [AllowAnonymous]
    public async Task<IActionResult> ExportTrendingPdf()
    {
        var bytes = await _exportService.ExportTrendingArticlesToPdfAsync();
        return File(bytes, "application/pdf", "TrendingArticles.pdf");
    }

    /// <summary>GET /api/reports/trending/export/excel</summary>
    [HttpGet("trending/export/excel")]
    [AllowAnonymous]
    public async Task<IActionResult> ExportTrendingExcel()
    {
        var bytes = await _exportService.ExportTrendingArticlesToExcelAsync();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TrendingArticles.xlsx");
    }

    /// <summary>GET /api/reports/user-activity/export/pdf (Admin only)</summary>
    [HttpGet("user-activity/export/pdf")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ExportUserActivityPdf()
    {
        var bytes = await _exportService.ExportUserActivityToPdfAsync();
        return File(bytes, "application/pdf", "UserActivity.pdf");
    }

    /// <summary>GET /api/reports/user-activity/export/excel (Admin only)</summary>
    [HttpGet("user-activity/export/excel")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ExportUserActivityExcel()
    {
        var bytes = await _exportService.ExportUserActivityToExcelAsync();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "UserActivity.xlsx");
    }
}
