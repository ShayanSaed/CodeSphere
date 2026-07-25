namespace CodeSphere.Core.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportTrendingArticlesToPdfAsync();
    Task<byte[]> ExportTrendingArticlesToExcelAsync();
    Task<byte[]> ExportUserActivityToPdfAsync();
    Task<byte[]> ExportUserActivityToExcelAsync();
}
