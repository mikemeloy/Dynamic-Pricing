namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.Common;

public class CartItemDetails
{
    public int ProductId { get; set; }
    public int CartItemId { get; set; }
    public int CustomerId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}