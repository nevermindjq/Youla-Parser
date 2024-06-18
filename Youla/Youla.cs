using System.Text;

using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

using Models;
using Models.City;
using Models.Configs;
using Models.Converters;
using Models.Data.Abstractions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Youla.Models;
using Youla.Services;

namespace Youla;

public class Youla(
    Settings settings, 
    ProxyHttpClientFactory factory_proxy,
    AuthorizedHttpClientFactory factory_authorized,
    IRepository<Seller, string> sellers) {
    private const string _UrlCategories = "https://api.youla.ru/web/v1/categories/rotator";
    private const string _UrlCities = "https://youla.ru/cities";
    
    public async IAsyncEnumerable<Category> ParseCategoriesAsync() {
        var http = await factory_proxy.CreateAsync();
        var content = await _GetStringAsync(http, _UrlCategories);
        await http.DisposeAsync();
        
        var json = JsonConvert.DeserializeAnonymousType(content, new
        {
            data = new List<Category>(),
            status = 400,
        }, new()
        {
            Converters = new List<JsonConverter>
            {
                new JsonCategoryConverter()
            }
        });

        if (json is not { status: 200 }) {
            yield break;
        }

        foreach (var category in json.data) {
            yield return category;
        }
    }

    public async IAsyncEnumerable<City> ParseCitiesAsync() {
        var http = await factory_proxy.CreateAsync();
        var html = new HtmlDocument();
        html.Load(await _GetStringAsync(http, _UrlCities));
        await http.DisposeAsync();
        
        foreach (var node in html.QuerySelectorAll("li.cities_list__item > a")) {
            var href = node.Attributes["href"]!.Value!.Trim();
            
            yield return new()
            {
                Name = node.InnerText.Trim(),
                Slug = href["https://youla.ru/".Length..]
            };
        }
    }
    
    public async Task<City> CityAsync(string name) {
        var http = await factory_proxy.CreateAsync();
        var suggestion = await _ApiTownSuggestionAsync(http, name);
        var content = await _GetStringAsync(http, _UrlCity(suggestion.Reference));
        await http.DisposeAsync();

        return JsonConvert.DeserializeObject<City>(content, new JsonToCityConverter())!;
    }
    
    public async IAsyncEnumerable<Product> GetProducts(int limit = 100, int page = 0) {
        var http = await factory_proxy.CreateAsync();
        var content = await _GetStringAsync(http, _UrlProducts(limit, page));
        await http.DisposeAsync();
        var json = JsonConvert.DeserializeObject<JObject>(content)!;
        //var serializer = new JsonSerializer()
        //{
        //    Converters =
        //    {
        //        new JsonApiToProductConverter()
        //    }
        //};
        
        foreach (var product in json["data"]!.ToObject<JArray>()!) {
            if (product.Value<bool>("is_deleted") || product.Value<string>("id") is not {} id) {
                continue;
            }

            yield return await GetProductInfo(id);
        }
    }

    public async Task<Product> GetProductInfo(string id) {
        var http = await factory_proxy.CreateAsync();
        var content = await _GetStringAsync(http, _UrlProductInfo(id));
        var product = JsonConvert.DeserializeObject<Product>(content, new JsonApiToProductConverter())!;
        
        if (await sellers.FindAsync(product.Seller.Id) is { }) {
            product.SellerId = product.Seller.Id;
            product.Seller = null;
        }
        else {
            product.Seller = await _SellerInfo(http, product.Seller!.Id, product.Seller.CanCall);
        }
        
        await http.DisposeAsync();

        //if (product.Seller is {Phone: not null, CanCall: true}) {
        //    return product;
        //}

        //product.Seller = null;
            
        return product;
    }

    public async Task<Seller> _SellerInfo(ProxyHttpClient http, string userId, bool canCall) {
        var infoUser = await _GetStringAsync(http, _UrlSellerInfo(userId));
        var seller = JsonConvert.DeserializeObject<Seller>(infoUser, new JsonToSellerConverter())!;

        seller.CanCall = canCall;
        
#if SKIP_PHONE_PARSING
        return seller; 
#else
        if (!seller.CanCall) {
            return seller;
        }
        
        // phone
        if (!await factory.CanCreateAsync()) {
            return seller;
        }
        
        var authorized = await factory.CreateClientAsync();
        var phone = await authorized.ParsePhone(seller.Id);
        await authorized.DisposeAsync();

        if (phone is null) {
            return seller;
        }

        seller.Phone = phone;
        
        return seller;
#endif
    }

    // Private
    private string _UrlCity(string reference) =>
        $"https://api-gw.youla.ru/geoproxy/api/v1/geocoding/reference?reference={reference}&app_id=android%2F11086";
    
    private string _UrlSuggestions(string name) =>
        $"https://api-gw.youla.ru/geoproxy/api/v1/suggest?q={name}&location=0,0&app_id=android%2F11086";
    
    private string _UrlProductInfo(string id) => $"https://api.youla.io/api/v1/product/{id}?app_id=android%2F11086";
    
    private string _UrlProducts(int limit, int page) {
        var builder =
            new StringBuilder($"https://api.youla.io/api/v1/products" +
                              $"?sort_field=date_published" +
                              $"&app_id=android%2F11086" +
                              $"&app=com.allgoritm.youla" +
                              $"&sort_direction=desc" +
                              $"&distance_max=50000" +
                              $"&limit={limit}" +
                              $"&page={page}");
        
        if (settings.Region is not null) {
            builder.Append($"&latitude={settings.Region!.Latitude}&longitude={settings.Region.Longitude}");
        }

        if (settings.Categories.Count != 0) {
            builder.Append($"&category={string.Join(',', settings.Categories.Select(x => x.Id))}");
        }
        
        return builder.ToString();
    }
    
    private string _UrlSellerInfo(string userId) =>
        $"https://api.youla.io/api/v1/user/{userId}?app_id=android%2F11086";
    
    private async Task<LocationSuggestion> _ApiTownSuggestionAsync(ProxyHttpClient http, string name) {
        var content = await _GetStringAsync(http, _UrlSuggestions(name));

        return JsonConvert.DeserializeObject<JObject>(content)!["suggestions"]!
            .ToObject<List<LocationSuggestion>>()!
            .First(x => x.Type == "city");
    }
    
    private async Task<string> _GetStringAsync(ProxyHttpClient http, string url) {
        var response = await http.GetAsync(url);

        if (!response.IsSuccessStatusCode) {
            throw new HttpRequestException(
                $"Error in request\n{url}\n{await response.Content.ReadAsStringAsync()}");
        }

        return await response.Content.ReadAsStringAsync();
    }
}