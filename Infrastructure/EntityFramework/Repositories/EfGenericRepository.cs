using CoreApp.Entities;
using CoreApp.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Repositories;

public class EfGenericRepository<T>(DbSet<T> set) : IGenericRepositoryAsync<T>
    where T : EntityBase
{
    public virtual async Task<T?> FindByIdAsync(Guid id)
        => await set.FindAsync(id);

    public async Task<IEnumerable<T>> FindAllAsync()
        => await set.ToListAsync();

    public async Task<PagedResult<T>> FindPagedAsync(int page, int pageSize)
    {
        var items = await set.AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PagedResult<T>(items, await set.CountAsync(), page, pageSize);
    }

    public async Task<T> AddAsync(T entity)
    {
        var entry = await set.AddAsync(entity);
        return entry.Entity;
    }

    public Task<T> UpdateAsync(T entity)
    {
        var entry = set.Update(entity);
        return Task.FromResult(entry.Entity);
    }

    public async Task RemoveByIdAsync(Guid id)
    {
        var entity = await set.FindAsync(id)
            ?? throw new KeyNotFoundException($"Entity with id {id} not found.");
        set.Remove(entity);
    }
}