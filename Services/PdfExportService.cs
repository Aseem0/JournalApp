using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using JournalApp.Models;
using System.Text.RegularExpressions;
using System.Net;
using QuestColors = QuestPDF.Helpers.Colors;

namespace JournalApp.Services;

// Service responsible for generating PDF archives of journal entries.
public class PdfExportService
{
    // Generates a PDF document containing a collection of journal entries.
    public byte[] GenerateJournalPdf(IEnumerable<JournalItem> entries)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                // Basic page configuration
                ConfigurePage(page);
                
                // Header section
                page.Header().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("My Journal Archive").SemiBold().FontSize(20).FontColor(QuestColors.Blue.Medium);
                        col.Item().Text($"Generated on {DateTime.Now:MMM dd, yyyy}").FontSize(10).FontColor(QuestColors.Grey.Medium);
                    });
                });

                // Main content: journal entries
                page.Content().PaddingVertical(10).Column(column =>
                {
                    var journalEntries = entries.OrderByDescending(e => e.EntryDate).ToList();

                    if (!journalEntries.Any())
                    {
                        column.Item().Text("No entries found for the selected range.").Italic();
                        return;
                    }

                    foreach (var entry in journalEntries)
                    {
                        RenderEntry(column, entry);
                    }
                });

                // Footer with page numbering
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

    // Configures global page settings.
    private static void ConfigurePage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin(1, Unit.Inch);
        page.PageColor(QuestColors.White);
        page.DefaultTextStyle(x => x.FontSize(11).FontColor(QuestColors.Black));
    }

    // Renders a single journal entry row in the PDF.
    private void RenderEntry(ColumnDescriptor column, JournalItem entry)
    {
        column.Item().PaddingBottom(15).Column(entryColumn =>
        {
            // Date and Mood Row
            entryColumn.Item().BorderBottom(1).BorderColor(QuestColors.Grey.Lighten2).PaddingBottom(5).Row(row =>
            {
                row.RelativeItem().Text(entry.EntryDate.ToString("dddd, MMMM dd, yyyy")).SemiBold().FontSize(13);
                row.AutoItem().Text(entry.PrimaryMood).Italic().FontColor(QuestColors.Blue.Medium);
            });

            // Tags section
            if (entry.TagList.Any())
            {
                entryColumn.Item().PaddingTop(2).Text(t =>
                {
                    t.Span("Tags: ").Bold().FontSize(9);
                    t.Span(string.Join(", ", entry.TagList)).FontSize(9).FontColor(QuestColors.Grey.Darken1);
                });
            }

            // Sanitized entry content
            entryColumn.Item().PaddingTop(8).Text(StripHtml(entry.Content)).LineHeight(1.5f);
        });
    }

    // Strips HTML tags from entry content for PDF rendering.
    private static string StripHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        
        // Manual conversion for common tags to maintain basic formatting
        var text = input.Replace("<br>", "\n")
                        .Replace("<br/>", "\n")
                        .Replace("<p>", string.Empty)
                        .Replace("</p>", "\n\n");
        
        // Remove all other HTML tags
        var plainText = Regex.Replace(text, "<.*?>", string.Empty);
        
        return WebUtility.HtmlDecode(plainText).Trim();
    }
}
