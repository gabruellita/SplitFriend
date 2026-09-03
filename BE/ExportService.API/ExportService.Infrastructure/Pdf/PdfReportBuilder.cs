using System.Globalization;
using ExportService.DTO.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ExportService.Infrastructure.Pdf;

public class PdfReportBuilder : IPdfReportBuilder
{
    private static readonly CultureInfo Ro = new("ro-RO");

    public byte[] Build(ReportModel model)
    {
        var doc = Document.Create(container =>
        {
            // Coperta
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(t => t.FontSize(11));
                page.Content().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text("Raport financiar").FontSize(26).Bold();
                    col.Item().Text(model.Header.UserLabel).FontSize(14);
                    col.Item().Text(model.Header.PeriodLabel).FontSize(12).FontColor(Colors.Grey.Darken1);
                    col.Item().Text($"Moneda: {model.Header.CurrencyCode}").FontSize(11);
                    col.Item().Text($"Generat: {model.Header.GeneratedAt.ToString("dd.MM.yyyy HH:mm", Ro)}")
                       .FontSize(10).FontColor(Colors.Grey.Medium);
                });
            });

            foreach (var section in model.Sections)
                AddSection(container, section, model.Header.CurrencyCode);

            if (model.CumulativeTotal is not null)
                AddSection(container, model.CumulativeTotal, model.Header.CurrencyCode);
        });

        return doc.GeneratePdf();
    }

    private static void AddSection(IDocumentContainer container, ReportSection section, string currency)
    {
        container.Page(page =>
        {
            page.Margin(40);
            page.Size(PageSizes.A4);
            page.DefaultTextStyle(t => t.FontSize(10));

            page.Header().PaddingBottom(10).Text(section.Title).FontSize(18).Bold();

            page.Content().Column(col =>
            {
                col.Spacing(14);

                if (section.Kpi is { } k)
                    col.Item().Row(row =>
                    {
                        Kpi(row, "Venituri",   k.TotalIncome,  currency, Colors.Green.Darken1);
                        Kpi(row, "Cheltuieli", k.TotalExpense, currency, Colors.Red.Darken1);
                        Kpi(row, "Sold net",   k.Net,          currency, Colors.Blue.Darken1);
                        if (k.SavingsRatePct is { } rate)
                            row.RelativeItem().Border(1).Padding(8).Column(c =>
                            {
                                c.Item().Text("Rata economisire").FontSize(9).FontColor(Colors.Grey.Darken1);
                                c.Item().Text($"{rate.ToString("0.#", Ro)}%").FontSize(14).Bold();
                            });
                    });

                if (section.TrendChartPng is { Length: > 0 } trend)
                {
                    col.Item().Text("Evolutie venituri vs cheltuieli").FontSize(12).SemiBold();
                    col.Item().Image(trend);
                }

                if (section.CategoryChartPng is { Length: > 0 } cat)
                {
                    col.Item().Text("Cheltuieli pe categorii").FontSize(12).SemiBold();
                    col.Item().Image(cat);
                }

                if (section.TopCategories is { Count: > 0 } top)
                {
                    col.Item().Text("Top categorii").FontSize(12).SemiBold();
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(); c.RelativeColumn(); });
                        table.Header(h =>
                        {
                            h.Cell().Text("Categorie").SemiBold();
                            h.Cell().AlignRight().Text("Total").SemiBold();
                            h.Cell().AlignRight().Text("%").SemiBold();
                        });
                        foreach (var t in top)
                        {
                            table.Cell().Text(t.CategoryName ?? "(fara)");
                            table.Cell().AlignRight().Text($"{t.Total.ToString("N2", Ro)} {currency}");
                            table.Cell().AlignRight().Text(t.Pct is { } p ? $"{p.ToString("0.#", Ro)}%" : "-");
                        }
                    });
                }

                if (section.Transactions is { Count: > 0 } txns)
                {
                    col.Item().Text("Extras tranzactii").FontSize(12).SemiBold();
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();    // data
                            c.RelativeColumn(2);   // descriere
                            c.RelativeColumn();    // categorie
                            c.RelativeColumn();    // tip
                            c.RelativeColumn();    // suma
                            if (section.RunningBalanceInStatement) c.RelativeColumn(); // sold
                        });
                        table.Header(h =>
                        {
                            h.Cell().Text("Data").SemiBold();
                            h.Cell().Text("Descriere").SemiBold();
                            h.Cell().Text("Categorie").SemiBold();
                            h.Cell().Text("Tip").SemiBold();
                            h.Cell().AlignRight().Text("Suma").SemiBold();
                            if (section.RunningBalanceInStatement) h.Cell().AlignRight().Text("Sold").SemiBold();
                        });

                        decimal balance = 0;
                        foreach (var t in txns.OrderBy(t => t.TransactionDate))
                        {
                            var signed = t.Kind == "INCOME" ? t.Amount : -t.Amount;
                            balance += signed;
                            table.Cell().Text(t.TransactionDate.ToString("dd.MM.yyyy", Ro));
                            table.Cell().Text(t.Description ?? "-");
                            table.Cell().Text(t.CategoryName ?? "-");
                            table.Cell().Text(t.Kind == "INCOME" ? "Venit" : "Cheltuiala");
                            table.Cell().AlignRight().Text($"{signed.ToString("N2", Ro)} {t.CurrencyCode ?? currency}");
                            if (section.RunningBalanceInStatement)
                                table.Cell().AlignRight().Text($"{balance.ToString("N2", Ro)} {currency}");
                        }
                    });
                }
            });

            page.Footer().AlignCenter().Text(x =>
            {
                x.CurrentPageNumber(); x.Span(" / "); x.TotalPages();
            });
        });
    }

    private static void Kpi(RowDescriptor row, string label, decimal value, string currency, string color)
        => row.RelativeItem().Border(1).Padding(8).Column(c =>
        {
            c.Item().Text(label).FontSize(9).FontColor(Colors.Grey.Darken1);
            c.Item().Text($"{value.ToString("N2", Ro)} {currency}").FontSize(14).Bold().FontColor(color);
        });
}
