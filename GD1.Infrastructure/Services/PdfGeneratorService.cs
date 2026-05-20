using GD1.Application.Interfaces.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;

namespace GD1.Infrastructure.Services
{
    public class PdfGeneratorService : IPdfGeneratorService
    {
        static PdfGeneratorService()
        {
            // Required by QuestPDF Community License
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateFromHtml(string html)
        {
            // Strip HTML tags for content extraction (QuestPDF uses fluent API, not HTML rendering)
            // We build the PDF programmatically from the stored HTML structure
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11).LineHeight(1.5f));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ctx => ComposeContent(ctx, html));
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("GD1 Grand Auto Depot — Confidential Agreement | Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            });

            return doc.GeneratePdf();
        }

        private void ComposeHeader(IContainer container)
        {
            container.BorderBottom(2).BorderColor("#4f46e5").PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("GD1 Grand Auto Depot")
                        .FontSize(22).Bold().FontColor("#4f46e5");
                    col.Item().Text("Vehicle Storage Services")
                        .FontSize(11).FontColor("#6b7280");
                });
                row.ConstantItem(120).AlignRight().AlignBottom().Text("OFFICIAL AGREEMENT")
                    .FontSize(9).Bold().FontColor("#9ca3af").LetterSpacing(0.5f);
            });
        }

        private void ComposeContent(IContainer container, string html)
        {
            container.PaddingTop(20).Column(col =>
            {
                col.Spacing(12);

                // Title
                col.Item().AlignCenter().Text("Vehicle Storage Agreement")
                    .FontSize(16).Bold().FontColor("#1a1a2e");

                col.Item().AlignCenter().Text($"Generated on: {DateTime.UtcNow:dd MMM yyyy}")
                    .FontSize(10).FontColor("#9ca3af");

                col.Item().PaddingTop(8).LineHorizontal(0.5f).LineColor("#e5e7eb");

                // Extract plain text sections from HTML
                var sections = ExtractSections(html);

                foreach (var section in sections)
                {
                    if (section.IsHeader)
                    {
                        col.Item().PaddingTop(6).Text(section.Text)
                            .FontSize(11).Bold().FontColor("#4f46e5");
                        col.Item().LineHorizontal(0.5f).LineColor("#c7d2fe");
                    }
                    else if (section.IsKeyValue)
                    {
                        col.Item().Background("#f9fafb").Border(0.5f).BorderColor("#e5e7eb")
                            .Padding(8).Row(row =>
                            {
                                row.RelativeItem().Text(section.Key).FontSize(10).FontColor("#6b7280");
                                row.RelativeItem().Text(section.Value).FontSize(10).Bold().FontColor("#111827");
                            });
                    }
                    else if (section.IsTermItem)
                    {
                        col.Item().PaddingLeft(4).Row(row =>
                        {
                            row.ConstantItem(20).Text($"{section.Index}.").FontSize(10).FontColor("#4f46e5");
                            row.RelativeItem().Text(section.Text).FontSize(10).FontColor("#374151");
                        });
                    }
                    else if (section.IsTotalCost)
                    {
                        col.Item().Background("#eff6ff").Border(0.5f).BorderColor("#bfdbfe")
                            .Padding(12).Column(c =>
                            {
                                c.Item().Text("Estimated Total Cost").FontSize(10).FontColor("#3b82f6");
                                c.Item().Text(section.Text).FontSize(18).Bold().FontColor("#1d4ed8");
                            });
                    }
                    else
                    {
                        col.Item().Text(section.Text).FontSize(10).FontColor("#374151");
                    }
                }

                col.Item().PaddingTop(16).LineHorizontal(0.5f).LineColor("#e5e7eb");

                // Footer signature area
                col.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Customer Acceptance").FontSize(10).FontColor("#6b7280");
                        c.Item().PaddingTop(4).Background("#d1fae5").Border(0.5f).BorderColor("#6ee7b7")
                            .Padding(8).Text("✓ Digitally Accepted via GD1 Platform")
                            .FontSize(10).Bold().FontColor("#065f46");
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("GD1 Grand Auto Depot").FontSize(10).FontColor("#6b7280");
                        c.Item().PaddingTop(4).Text("Authorized Representative")
                            .FontSize(10).Bold().FontColor("#111827");
                        c.Item().Text("GD1 Operations Team").FontSize(9).FontColor("#9ca3af");
                    });
                });

                col.Item().PaddingTop(12).AlignCenter()
                    .Text("This is a system-generated agreement from the GD1 Grand Auto Depot platform.")
                    .FontSize(9).FontColor("#d1d5db").Italic();
            });
        }

        // Lightweight HTML section extractor - parses key info from the stored HTML template
        private static ContentSection[] ExtractSections(string html)
        {
            var sections = new System.Collections.Generic.List<ContentSection>();
            if (string.IsNullOrWhiteSpace(html)) return sections.ToArray();

            // Extract key-value pairs from info-item divs
            var infoPattern = System.Text.RegularExpressions.Regex.Matches(
                html,
                @"<div class=""info-label"">(.*?)</div>\s*<div class=""info-value"">(.*?)</div>",
                System.Text.RegularExpressions.RegexOptions.Singleline);

            // Extract section titles
            var sectionTitles = System.Text.RegularExpressions.Regex.Matches(
                html,
                @"<div class=""section-title"">(.*?)</div>",
                System.Text.RegularExpressions.RegexOptions.Singleline);

            // Extract total cost
            var totalMatch = System.Text.RegularExpressions.Regex.Match(
                html,
                @"<div class=""total"">(.*?)</div>",
                System.Text.RegularExpressions.RegexOptions.Singleline);

            // Extract terms list items
            var termItems = System.Text.RegularExpressions.Regex.Matches(
                html,
                @"<li><strong>(.*?)</strong>(.*?)</li>",
                System.Text.RegularExpressions.RegexOptions.Singleline);

            // Build structured content
            // 1. Customer section
            sections.Add(new ContentSection { IsHeader = true, Text = "Customer Details" });
            int infoIdx = 0;
            // Add customer info (first 2)
            for (int i = 0; i < Math.Min(2, infoPattern.Count); i++, infoIdx++)
                sections.Add(new ContentSection { IsKeyValue = true, Key = StripHtml(infoPattern[i].Groups[1].Value), Value = StripHtml(infoPattern[i].Groups[2].Value) });

            // 2. Vehicle section
            sections.Add(new ContentSection { IsHeader = true, Text = "Vehicle Details" });
            for (int i = 2; i < Math.Min(5, infoPattern.Count); i++)
                sections.Add(new ContentSection { IsKeyValue = true, Key = StripHtml(infoPattern[i].Groups[1].Value), Value = StripHtml(infoPattern[i].Groups[2].Value) });

            // 3. Storage section
            sections.Add(new ContentSection { IsHeader = true, Text = "Storage Property" });
            for (int i = 5; i < infoPattern.Count; i++)
                sections.Add(new ContentSection { IsKeyValue = true, Key = StripHtml(infoPattern[i].Groups[1].Value), Value = StripHtml(infoPattern[i].Groups[2].Value) });

            // 4. Total cost
            if (totalMatch.Success)
                sections.Add(new ContentSection { IsTotalCost = true, Text = StripHtml(totalMatch.Groups[1].Value) });

            // 5. Terms & conditions
            sections.Add(new ContentSection { IsHeader = true, Text = "Terms & Conditions" });
            int termIndex = 1;
            foreach (System.Text.RegularExpressions.Match m in termItems)
                sections.Add(new ContentSection
                {
                    IsTermItem = true,
                    Index = termIndex++,
                    Text = $"{StripHtml(m.Groups[1].Value)}: {StripHtml(m.Groups[2].Value)}"
                });

            return sections.ToArray();
        }

        private static string StripHtml(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(
                System.Web.HttpUtility.HtmlDecode(input ?? ""), "<.*?>", "").Trim();
        }

        private class ContentSection
        {
            public bool IsHeader { get; set; }
            public bool IsKeyValue { get; set; }
            public bool IsTermItem { get; set; }
            public bool IsTotalCost { get; set; }
            public string Text { get; set; } = string.Empty;
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public int Index { get; set; }
        }
    }
}
