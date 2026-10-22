namespace AkilliDepo.Models;

public sealed class SaleReceipt
{
    public DateTime Date { get; }
    public IReadOnlyList<SaleLine> Lines { get; }
    public IReadOnlyList<Product> CriticalProducts { get; }
    public decimal TaxRate { get; }
    public decimal Subtotal { get; }
    public decimal Tax { get; }
    public decimal Total { get; }

    public SaleReceipt(IEnumerable<SaleLine> lines, IEnumerable<Product> criticalProducts, decimal taxRate)
    {
        Date = DateTime.Now;
        Lines = lines.ToArray();
        CriticalProducts = criticalProducts.ToArray();
        TaxRate = taxRate;
        Subtotal = Lines.Sum(x => x.Subtotal);
        Tax = decimal.Round(Subtotal * taxRate, 2, MidpointRounding.AwayFromZero);
        Total = Subtotal + Tax;
    }
}
