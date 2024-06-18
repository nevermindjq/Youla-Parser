using Models.Data.Abstractions;
using Models.Net;

namespace Youla.Models;

public class ProxyHttpClient(HttpMessageHandler handler, IRepository<Proxy, long> proxies) : HttpClient(handler, true), IAsyncDisposable {
    public Proxy? Proxy { get; set; }
    
    #region Dispose Async

    private async ValueTask DisposeAsyncCore() {
        if (Proxy is null) {
            return;
        }
        
        Proxy.InUse = false;

        await proxies.UpdateAsync(Proxy);
    }

    public async ValueTask DisposeAsync() {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    #endregion
}