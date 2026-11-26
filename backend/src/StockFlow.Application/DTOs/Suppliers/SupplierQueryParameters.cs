
namespace StockFlow.Application.DTOs.Suppliers;

public class SupplierQueryParameters
{
    public string? Search { get; set; }

    public bool? IsActive { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
