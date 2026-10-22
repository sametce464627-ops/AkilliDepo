namespace AkilliDepo.Models;

public sealed class ProductSalesStat
{
    public Product Product { get; }
    public int QuantitySold { get; }

    public ProductSalesStat(Product product, int quantitySold)
    {
        Product = product;
        QuantitySold = quantitySold;
    }
}
