using System.Linq.Expressions;

using Models.Data.Abstractions;
using Models.Net;

namespace Models.Data;

// Bot
public class UsersRepository(AppDbContext context) : BaseRepository<User, long, AppDbContext>(context);

public class ApproveMessagesRepository(AppDbContext context) : BaseRepository<ApproveMessage, string, AppDbContext>(context);

// Youla
public class CategoriesRepository(AppDbContext context) : BaseRepository<Category, int, AppDbContext>(context);

public class CitiesRepository(AppDbContext context) : BaseRepository<City.City, string, AppDbContext>(context);

public class ProductsRepository(AppDbContext context) : BaseRepository<Product, string, AppDbContext>(context);

public class SellersRepository(AppDbContext context) : BaseRepository<Seller, string, AppDbContext>(context);

// Http
public class CookiesRepository(AppDbContext context) : BaseRepository<Cookie, string, AppDbContext>(context);

public class ProxiesRepository(AppDbContext context) : BaseRepository<Proxy, long, AppDbContext>(context) {
    public override async Task<Proxy?> FindAsync(Expression<Func<Proxy, bool>> predicate) {
        var proxy = await base.FindAsync(predicate);

        if (proxy == null) {
            return null;
        }
        
        proxy.InUse = true;
        
        await UpdateAsync(proxy);
        
        return proxy;
    }
}