using Models.Data.Abstractions;
using Models.Net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Youla.Models;

public sealed class AuthorizedHttpClient(HttpMessageHandler handler, IRepository<Cookie, string> cookies, IRepository<Proxy, long> proxies) : ProxyHttpClient(handler, proxies) {
    public Cookie? Cookies { get; set; }
    public double PhoneDelay { get; set; } = 5;

    public async Task<string?> ParsePhone(string userId) {
        byte @try = 0;
        
        Repeat:
        await Task.Delay(TimeSpan.FromMinutes(PhoneDelay));
        
        var url = $"https://api.youla.ru/api/v1/users/{userId}/contact_info?app_id=web%2F3&uid={Cookies!.Uid}&timestamp={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await SendAsync(request);

        if (!response.IsSuccessStatusCode) {
            @try++;
            
            if (@try == 1) {
                await _UpdateCookiesAsync();
                goto Repeat;
            }
            
            Cookies.IsInvalidToken = true;

            await cookies.UpdateAsync(Cookies);
            
            return null;
        }
        
        var content = await response.Content.ReadAsStringAsync();

        var json = JsonConvert.DeserializeObject<JObject>(content);
        var phone = json["data"]["phone"].Value<JObject?>();

        if (phone is not null) {
            return phone["raw"]!.Value<string>();
        }

        Cookies.IsShadowBanned = true;
        await cookies.UpdateAsync(Cookies);
        
        return null;
    }
    
    private async Task _UpdateCookiesAsync(string url = "https://youla.ru") {
        var response = await GetAsync(url);
        var headers = response.Headers
            .Where(header => header.Key == "Set-Cookie")
            .SelectMany(x => x.Value)
            .Where(x => x.StartsWith("youla_auth"))
            .Select(x => (x[..x.IndexOf('=')], x[(x.IndexOf('=')+1)..x.IndexOf(';')]))
            .Select(x => x.Item2 == "deleted" ? (null, null) : x)
            .Where(x => x.Item1 is not null)
            .ToDictionary(x => x.Item1!, x => x.Item2!);

        if (!headers.TryGetValue("youla_auth", out var youla_auth)) {
            return;
        }

        Cookies.YoulaAuth = youla_auth;
        Cookies.YoulaAuthRefresh = headers["youla_auth_refresh"];
        Cookies.YoulaAuthRefreshSwitchUser = headers["youla_auth_refresh_switch_user"];

        DefaultRequestHeaders.Authorization = new("Bearer", Cookies.YoulaAuth);
        
        await cookies.UpdateAsync(Cookies);
    }
}