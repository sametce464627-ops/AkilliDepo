namespace AkilliDepo.Models;

public sealed class SupplierStat
{
    public Supplier Supplier { get; }
    public int ProductCount { get; }
    public decimal StockValue { get; }

    public SupplierStat(Supplier supplier, int count, decimal stockValue)
    {
        Supplier = supplier;
        ProductCount = count;
        StockValue = stockValue;
    }
}
