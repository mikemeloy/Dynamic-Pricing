using ClosedXML.Excel;
using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Extensions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;
using Microsoft.AspNetCore.Http;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Factories;

public interface IDynamicPriceImportFactory
{
    public Task ImportProductFromXSLTDataAsync(IFormFile file);
    public Task ExportProductAsync(Stream stream);
}

public static class Column
{
    public static string Sku => "A";
    public static string ProductName => "B";
    public static string Manufacturer => "C";
    public static string Pattern => "D";
    public static string MetalType => "E";
    public static string Weight => "F";
}


public class DynamicPriceImportFactory(IDynamicPriceService dynamicPriceService) : IDynamicPriceImportFactory
{
    private string _sheetName => "Products";
    public async Task ExportProductAsync(Stream stream)
    {
        var products = await dynamicPriceService.GetProductsNotDynamicallyPricedAsync();
        var metalTypes = await dynamicPriceService.GetMetalTypesAsync();

        using var wb = new XLWorkbook();
        var productWorkSheet = wb.AddWorksheet(_sheetName);

        productWorkSheet.Column(Column.Sku).Width = 20;
        productWorkSheet.Column(Column.ProductName).Width = 50;
        productWorkSheet.Column(Column.Manufacturer).Width = 30;
        productWorkSheet.Column(Column.Pattern).Width = 20;
        productWorkSheet.Column(Column.MetalType).Width = 20;
        productWorkSheet.Column(Column.Weight).Width = 20;

        productWorkSheet.Cell($"{Column.Sku}1").Value = nameof(Column.Sku);
        productWorkSheet.Cell($"{Column.ProductName}1").Value = nameof(Column.ProductName);
        productWorkSheet.Cell($"{Column.Manufacturer}1").Value = nameof(Column.Manufacturer);
        productWorkSheet.Cell($"{Column.Pattern}1").Value = nameof(Column.Pattern);
        productWorkSheet.Cell($"{Column.MetalType}1").Value = nameof(Column.MetalType);
        productWorkSheet.Cell($"{Column.Weight}1").Value = nameof(Column.Weight);

        productWorkSheet.Cell("A2").InsertData(products.Select(n => new
        {
            Sku = n.Sku,
            ProductName = n.ProductName,
            Manufacturer = n.ManufacturerName,
            Pattern = "",
            MetalType = n.MetalTypeName,
            Weight = n.Weight
        }));

        var metalTypeWorkSheet = wb.AddWorksheet("Metal Types");

        metalTypeWorkSheet.Cell("A1")
            .InsertData(metalTypes
            .Where(mt => !string.IsNullOrEmpty(mt.ApiSymbol) && !mt.Deleted)
            .Select(mt => new
            {
                mt.Name,
                mt.ApiSymbol,
                mt.CurrentValue,
            }));

        wb.SaveAs(stream);
    }

    public async Task ImportProductFromXSLTDataAsync(IFormFile file)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.FirstOrDefault(f => f.Name.Equals(_sheetName, StringComparison.CurrentCultureIgnoreCase));

        if (ws.IsNull())
        {
            return;
        }

        var metalTypes = await dynamicPriceService.GetMetalTypesAsync();

        foreach (var row in ws.RangeUsed().RowsUsed().Skip(1))
        {
            var sku = row.Cell(1).GetValue<string>();
            var metal = row.Cell(5).GetValue<string>();
            var hasWeight = row.Cell(6).TryGetValue<decimal>(out var val);

            await dynamicPriceService.UpdateProductBySkuAsync(
                sku: sku,
                weight: hasWeight ? val : 0,
                metalType: metalTypes.GetMetalTypeIdByName(metal)
            );
        }
    }
}