using CoreApp.Entities;
using CoreApp.Repositories;

namespace Infrastructure.Memory;

public class MemoryGenericRepository<T> : IGenericRepositoryAsync<T>
    where T : EntityBase
{
    protected Dictionary<Guid, T> _data = new();

    public Task<T?> FindByIdAsync(Guid id)
    {
        var result = _data.TryGetValue(id, out var value) ? value : null;
        return Task.FromResult(result);
    }

    public Task<IEnumerable<T>> FindAllAsync()
    {
        return Task.FromResult<IEnumerable<T>>(_data.Values.ToList());
    }

    public Task<PagedResult<T>> FindPagedAsync(int page, int pageSize)
    {
        var items = _data.Values
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new PagedResult<T>(items, _data.Count, page, pageSize);
        return Task.FromResult(result);
    }

    public Task<T> AddAsync(T entity)
    {
        _data[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<T> UpdateAsync(T entity)
    {
        if (!_data.ContainsKey(entity.Id))
            throw new KeyNotFoundException($"Entity with id {entity.Id} not found.");

        _data[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task RemoveByIdAsync(Guid id)
    {
        if (!_data.Remove(id))
            throw new KeyNotFoundException($"Entity with id {id} not found.");

        return Task.CompletedTask;
    }
}
