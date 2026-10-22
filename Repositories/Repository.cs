using AkilliDepo.Interfaces;

namespace AkilliDepo.Repositories;

public sealed class Repository<T> where T : class, IEntity
{
    private readonly List<T> _items = new();

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (GetById(item.Id) is not null)
            throw new InvalidOperationException($"{typeof(T).Name} için {item.Id} Id zaten kayıtlı.");
        _items.Add(item);
    }

    public bool Remove(int id)
    {
        T? item = GetById(id);
        return item is not null && _items.Remove(item);
    }

    public T? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);
    public IReadOnlyList<T> GetAll() => _items.ToList().AsReadOnly();
    public IEnumerable<T> Find(Func<T, bool> predicate) => _items.Where(predicate).ToArray();
    public int Count => _items.Count;
}
