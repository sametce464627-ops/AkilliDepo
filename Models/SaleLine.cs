namespace AkilliDepo.Models;

public sealed class SaleLine
{
    public Product Product { get; }
    public string ProductName { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal Subtotal => Quantity * UnitPrice;

    public SaleLine(Product product, int quantity, decimal unitPrice)
    {
        Product = product;
        ProductName = product.Name;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
