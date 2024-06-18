using System.Net;

using Models.Configs;
using Models.Data.Abstractions;
using Models.Extensions;
using Models.Net;

using Youla.Models;

using Cookie = Models.Net.Cookie;

namespace Youla.Services;

public class AuthorizedHttpClientFactory(Settings settings, IRepository<Cookie, string> cookies, IRepository<Proxy, long> proxies) {
    public async Task<bool> CanCreateAsync() => await cookies.AnyAsync() && await proxies.AnyAsync();
    
    public async Task<AuthorizedHttpClient> CreateClientAsync() {
        // Configure
        var proxy = await proxies.FindOrWaitAsync(x => !x.InUse);
        var cookie = await cookies.FindOrWaitAsync(x => !x.IsShadowBanned && !x.IsInvalidToken);
        
        var handler = new HttpClientHandler()
        {
            AllowAutoRedirect = false,
            UseProxy = true,
            CookieContainer = _CreateCookieContainer(cookie),
            Proxy = new WebProxy()
            {
                Address = new($"http://{proxy.Host}:{proxy.Port}"),
            },
        };

        if (proxy is { Username: not null, Password: not null }) {
            handler.Proxy.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
        }
        
        var http = new AuthorizedHttpClient(handler, cookies, proxies)
        {
            Cookies = cookie,
            Proxy = proxy,
            PhoneDelay = settings.PhoneDelay
        };

        http.DefaultRequestHeaders.Authorization = new("Bearer", cookie.YoulaAuth);

        return http;
    }

    private CookieContainer _CreateCookieContainer(Cookie cookie) {
        var container = new CookieContainer();
        
        container.Add(new CookieCollection()
        {
            // .youla.ru
            new System.Net.Cookie("cto_bundle", cookie.CtoBundle) {
                Domain = ".youla.ru"
            },
            new System.Net.Cookie("youla_auth", cookie.YoulaAuth) {
                Domain = ".youla.ru"
            },
            new System.Net.Cookie("youla_auth_refresh", cookie.YoulaAuthRefresh) {
                Domain = ".youla.ru"
            },
            new System.Net.Cookie("_youla_uid", cookie.Uid) {
                Domain = ".youla.ru"
            },
            
            // youla.ru
            new System.Net.Cookie("domain_sid", cookie.DomainSid) {
                Domain = "youla.ru"
            },
            new System.Net.Cookie("sessid", cookie.SessId) {
                Domain = "youla.ru"
            },
        });

        return container;
    }
}