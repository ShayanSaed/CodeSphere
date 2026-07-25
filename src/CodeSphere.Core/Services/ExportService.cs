using ClosedXML.Excel;
using CodeSphere.Core.DTOs;
using CodeSphere.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CodeSphere.Core.Services;

/// <summary>
/// Bonus feature: generates print-ready PDF and Excel exports of the two
/// core reports (Trending Articles, User Activity) using QuestPDF and
/// ClosedXML respectively.
/// </summary>
public class ExportService : IExportService
{
    private const int TrendingReportRowLimit = 50;
    private const int UserActivityReportRowLimit = 50;

    private readonly IReportService _reportService;

    public ExportService(IReportService reportService)
    {
        _reportService = reportService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> ExportTrendingArticlesToPdfAsync()
    {
        var rows = await _reportService.GetTrendingArticlesAsync(TrendingReportRowLimit);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Text("CodeSphere — Trending Articles Report")
                    .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(50);
                        columns.ConstantColumn(50);
                        columns.ConstantColumn(50);
                        columns.ConstantColumn(60);
                    });

                    table.Header(header =>
                    {
                        foreach (var title in new[] { "#", "Title", "Author", "Category", "Views", "Cmts", "Rctns", "Score" })
                        {
                            header.Cell().Element(HeaderCellStyle).Text(title).SemiBold();
                        }
                    });

                    var rank = 1;
                    foreach (var r in rows)
                    {
                        table.Cell().Element(BodyCellStyle).Text(rank.ToString());
                        table.Cell().Element(BodyCellStyle).Text(r.Title);
                        table.Cell().Element(BodyCellStyle).Text(r.Author);
                        table.Cell().Element(BodyCellStyle).Text(r.CategoryName);
                        table.Cell().Element(BodyCellStyle).Text(r.ViewCount.ToString());
                        table.Cell().Element(BodyCellStyle).Text(r.CommentCount.ToString());
                        table.Cell().Element(BodyCellStyle).Text(r.ReactionCount.ToString());
                        table.Cell().Element(BodyCellStyle).Text(r.EngagementScore.ToString());
                        rank++;
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated on ");
                    x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportTrendingArticlesToExcelAsync()
    {
        var rows = await _reportService.GetTrendingArticlesAsync(TrendingReportRowLimit);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Trending Articles");

        string[] headers = { "Article ID", "Title", "Author", "Category", "Views", "Comments", "Reactions", "Engagement Score" };
        for (var i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        var row = 2;
        foreach (var r in rows)
        {
            ws.Cell(row, 1).Value = r.ArticleID;
            ws.Cell(row, 2).Value = r.Title;
            ws.Cell(row, 3).Value = r.Author;
            ws.Cell(row, 4).Value = r.CategoryName;
            ws.Cell(row, 5).Value = r.ViewCount;
            ws.Cell(row, 6).Value = r.CommentCount;
            ws.Cell(row, 7).Value = r.ReactionCount;
            ws.Cell(row, 8).Value = r.EngagementScore;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportUserActivityToPdfAsync()
    {
        var rows = await _reportService.GetUserActivityAsync(UserActivityReportRowLimit);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Text("CodeSphere — User Activity Report")
                    .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);

                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                    });

                    table.Header(header =>
                    {
                        foreach (var title in new[] { "#", "Username", "Full Name", "Articles", "Comments", "Reactions", "Followers" })
                            header.Cell().Element(HeaderCellStyle).Text(title).SemiBold();
                    });

                    var rank = 1;
                    foreach (var r in rows)
                    {
                        table.Cell().Element(BodyCellStyle).Text(rank.ToString());
                        table.Cell().Element(BodyCellStyle).Text(r.Username);
                        table.Cell().Element(BodyCellStyle).Text(r.FullName ?? "-");
                        table.Cell().Element(BodyCellStyle).Text(r.TotalArticles.ToString());
                        table.Cell().Element(BodyCellStyle).Text(r.TotalComments.ToString());
                        table.Cell().Element(BodyCellStyle).Text(r.TotalReactions.ToString());
                        table.Cell().Element(BodyCellStyle).Text(r.TotalFollowers.ToString());
                        rank++;
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated on ");
                    x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportUserActivityToExcelAsync()
    {
        var rows = await _reportService.GetUserActivityAsync(UserActivityReportRowLimit);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("User Activity");

        string[] headers = { "User ID", "Username", "Full Name", "Articles", "Comments", "Reactions", "Followers" };
        for (var i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        var row = 2;
        foreach (var r in rows)
        {
            ws.Cell(row, 1).Value = r.UserID;
            ws.Cell(row, 2).Value = r.Username;
            ws.Cell(row, 3).Value = r.FullName ?? "-";
            ws.Cell(row, 4).Value = r.TotalArticles;
            ws.Cell(row, 5).Value = r.TotalComments;
            ws.Cell(row, 6).Value = r.TotalReactions;
            ws.Cell(row, 7).Value = r.TotalFollowers;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static IContainer HeaderCellStyle(IContainer container) =>
        container.Background(Colors.Blue.Darken2).Padding(4).DefaultTextStyle(x => x.FontColor(Colors.White));

    private static IContainer BodyCellStyle(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4);
}
