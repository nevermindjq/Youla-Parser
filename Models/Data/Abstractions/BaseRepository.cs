using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.CompilerServices;

using Models.Abstractions;

namespace Models.Data.Abstractions;

public abstract class BaseRepository<TEntity, TKey, TContext>(TContext context) : IRepository<TEntity, TKey> 
    where TEntity : class, IEntity<TKey>
    where TContext : DbContext {
    public virtual async Task<bool> AddAsync(TEntity obj) {
        return await TrySaveChangesAsync(() => context.AddAsync(obj).AsTask());
    }

    public virtual Task<bool> AddAsync(IEnumerable<TEntity?> obj) {
        return TrySaveChangesAsync(() => context.AddRangeAsync(obj.Where(x => x is not null)!));
    }

    public virtual async Task<bool> RemoveAsync(TKey key) {
        if (await context.FindAsync<TEntity>(key) is not { } entity) {
            return false;
        }

        context.Remove(entity);

        return await TrySaveChangesAsync();
    }

    public virtual Task<bool> UpdateAsync(TEntity obj) {
        context.Update(obj);

        return TrySaveChangesAsync();
    }

    public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null) {
        var set = context.Set<TEntity>();
        
        if (predicate is not null) {
            return set.AnyAsync(predicate);
        }
        
        return set.AnyAsync();
    }

    public IAsyncEnumerable<TEntity> All(Func<TEntity, bool>? predicate) {
        IEnumerable<TEntity> set = context.Set<TEntity>();

        if (predicate is not null) {
            set = set.Where(predicate);
        }

        return set.ToAsyncEnumerable();
    }
    
    public virtual Task<TEntity?> FindAsync(TKey key) => context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id!.Equals(key));

    public virtual Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate) => context.Set<TEntity>().FirstOrDefaultAsync(predicate);

    private protected async Task<bool> TrySaveChangesAsync(Func<Task>? task = null) {
        try {
            await (task?.Invoke() ?? Task.CompletedTask);
            await context.SaveChangesAsync();
            
            return true;
        }
        catch {
            context.ChangeTracker.Clear();
        }

        return false;
    }
}