using AkilliDepo.Interfaces;

namespace AkilliDepo.Models;

public sealed class Category : IEntity, IEquatable<Category>
{
    public int Id { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    public Category(int id, string name, string description)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Kategori adı boş olamaz.");
        Id = id;
        Name = name.Trim();
        Description = description.Trim();
    }

    public bool Equals(Category? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is Category other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => $"{Id} - {Name}";
}
