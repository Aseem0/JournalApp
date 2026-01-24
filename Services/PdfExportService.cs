using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using JournalApp.Models;
using System.Text.RegularExpressions;
using QuestColors = QuestPDF.Helpers.Colors;

namespace JournalApp.Services;

public class PdfExportService
{
    public byte[] GenerateJournalPdf(List<JournalItem> entries)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Inch);
                page.PageColor(QuestColors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(QuestColors.Black));

                page.Header().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("My Journal Archive").SemiBold().FontSize(20).FontColor(QuestColors.Blue.Medium);
                        col.Item().Text($"Generated on {DateTime.Now:MMM dd, yyyy}").FontSize(10).FontColor(QuestColors.Grey.Medium);
                    });
                });

                page.Content().PaddingVertical(10).Column(column =>
                {
                    if (!entries.Any())
                    {
                        column.Item().Text("No entries found for the selected range.").Italic();
                        return;
                    }

                    foreach (var entry in entries.OrderByDescending(e => e.EntryDate))
                    {
                        column.Item().PaddingBottom(15).Column(entryColumn =>
                        {
                            // Date and Mood Header
                            entryColumn.Item().BorderBottom(1).BorderColor(QuestColors.Grey.Lighten2).PaddingBottom(5).Row(row =>
                            {
                                row.RelativeItem().Text(entry.EntryDate.ToString("dddd, MMMM dd, yyyy")).SemiBold().FontSize(13);
                                row.AutoItem().Text(entry.PrimaryMood).Italic().FontColor(QuestColors.Blue.Medium);
                            });

                            // Tags
                            if (entry.TagList.Any())
                            {
                                entryColumn.Item().PaddingTop(2).Text(t =>
                                {
                                    t.Span("Tags: ").Bold().FontSize(9);
                                    t.Span(string.Join(", ", entry.TagList)).FontSize(9).FontColor(QuestColors.Grey.Darken1);
                                });
                            }

                            // Content
                            entryColumn.Item().PaddingTop(8).Text(StripHtml(entry.Content)).LineHeight(1.5f);
                        });
                    }
                });

                page.Footer().PaddingTop(20).Column(footerCol =>
                {
                    footerCol.Item().LineHorizontal(0.5f).LineColor(QuestColors.Grey.Lighten1);
                    footerCol.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text("Personal Journal App").FontSize(10).FontColor(QuestColors.Grey.Medium);
                        row.AutoItem().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                    });
                });
            });
        }).GeneratePdf();
    }

    private string StripHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        
        // Basic conversion for common tags
        var text = input.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<p>", "").Replace("</p>", "\n\n");
        
        // Strip remaining tags
        var plainText = Regex.Replace(text, "<.*?>", string.Empty);
        
        return System.Net.WebUtility.HtmlDecode(plainText).Trim();
    }
}
