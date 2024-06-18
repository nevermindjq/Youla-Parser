using System.Linq.Expressions;

using Models.Abstractions;

namespace Models.Data.Abstractions;

public interface IRepository<T, in TKey> 
    where T : class, IEntity<TKey> {
    Task<bool> AddAsync(T obj);
    Task<bool> AddAsync(IEnumerable<T?> obj);
    Task<bool> RemoveAsync(TKey key);
    Task<bool> UpdateAsync(T obj);
    
    Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null);
    IAsyncEnumerable<T> All(Func<T, bool>? predicate = null);
    Task<T?> FindAsync(TKey key);
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
}