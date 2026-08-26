using ClosedXML.Excel;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;
using Microsoft.AspNetCore.Http;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Factories;

public interface IDynamicPriceImportFactory
{
    public Task ImportProductFromXSLTDataAsync(IFormFile file);
    public Task ExportProductAsync(Stream stream);
}

public class DynamicPriceImportFactory(IDynamicPriceService dynamicPriceService) : IDynamicPriceImportFactory
{
    public async Task ExportProductAsync(Stream stream)
    {
        var products = await dynamicPriceService.GetProductsNotDynamicallyPricedAsync();

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet();

        ws.Cell("A2").InsertData(products.Select(n => new
        {
            Sku = n.Sku,
            Name = n.Name
        }));

        wb.SaveAs(stream);
    }

    public async Task ImportProductFromXSLTDataAsync(IFormFile file)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.FirstOrDefault();


        var rows = ws.RangeUsed().RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            var sku = row.Cell(1).Value;
            var metal = row.Cell(5).Value;
            var weight = row.Cell(6).Value;
        }
    }
}
