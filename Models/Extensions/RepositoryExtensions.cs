using System.Linq.Expressions;

using Models.Abstractions;
using Models.Data.Abstractions;

namespace Models.Extensions;

public static class RepositoryExtensions {
    public static async Task<T> FindOrWaitAsync<T, TKey>(this IRepository<T, TKey> repository, Expression<Func<T, bool>> predicate, int timeout = 5000)
        where T : class, IEntity<TKey> {
        T? entity;

        do {
            entity = await repository.FindAsync(predicate);

            if (entity is null)
                await Task.Delay(timeout);

        } while (entity is null);

        return entity;
    }
}