using AkilliDepo.Interfaces;

namespace AkilliDepo.Models;

public sealed class StockMovement : IEntity
{
    public int Id { get; }
    public Product Product { get; }
    public int Quantity { get; }
    public DateTime Date { get; }
    public User PerformedBy { get; }
    public StockMovementType Type { get; }

    public StockMovement(int id, Product product, int quantity, User performedBy, StockMovementType type)
    {
        if (id <= 0 || quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        Id = id;
        Product = product ?? throw new ArgumentNullException(nameof(product));
        PerformedBy = performedBy ?? throw new ArgumentNullException(nameof(performedBy));
        Quantity = quantity;
        Type = type;
        Date = DateTime.Now;
    }
}
