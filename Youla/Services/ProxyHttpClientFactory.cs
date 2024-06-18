using System.Net;

using Models.Data.Abstractions;
using Models.Extensions;
using Models.Net;

using Youla.Models;

namespace Youla.Services;

public class ProxyHttpClientFactory(IRepository<Proxy, long> proxies) {
    public async Task<ProxyHttpClient> CreateAsync() {
        var proxy = await proxies.FindOrWaitAsync(x => !x.InUse);
        
        var handler = new HttpClientHandler()
        {
            AllowAutoRedirect = false,
            UseProxy = true,
            Proxy = new WebProxy()
            {
                Address = new($"http://{proxy.Host}:{proxy.Port}"),
            },
        };

        if (proxy is { Username: not null, Password: not null }) {
            handler.Proxy.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
        }
        
        return new ProxyHttpClient(handler, proxies)
        {
            Proxy = proxy
        };
    }
}