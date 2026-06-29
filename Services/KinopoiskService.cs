using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FilmotekaAPI.Kinopoisk;
using FilmotekaAPI.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace FilmotekaAPI.Services;

public class KinopoiskService(
    HttpClient httpClient,
    IDistributedCache cache,
    ILogger<KinopoiskService> logger) : IKinopoiskService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public async Task<KpFilm?> GetFilmAsync(int id, CancellationToken ct = default)
        => await GetCachedAsync<KpFilm>($"kp:film:{id}", $"/api/v2.2/films/{id}", TimeSpan.FromMinutes(30), ct);

    public async Task<KpFilmSearchByFiltersResponse?> GetFilmsAsync(Dictionary<string, string> queryParams, CancellationToken ct = default)
    {
        var paramStr = string.Join("&", queryParams.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}"));
        var hash = ComputeSha256(paramStr);
        var url = $"/api/v2.2/films?{paramStr}";
        return await GetCachedAsync<KpFilmSearchByFiltersResponse>($"kp:films:{hash}", url, TimeSpan.FromMinutes(5), ct);
    }

    public async Task<KpFilmCollectionResponse?> GetCollectionAsync(string collectionType, int page = 1, CancellationToken ct = default)
        => await GetCachedAsync<KpFilmCollectionResponse>(
            $"kp:collection:{collectionType}:{page}",
            $"/api/v2.2/films/collections?type={collectionType}&page={page}",
            TimeSpan.FromMinutes(30), ct);

    public async Task<KpFilmSearchResponse?> SearchFilmsAsync(string keyword, int page = 1, CancellationToken ct = default)
        => await GetCachedAsync<KpFilmSearchResponse>(
            $"kp:search:{ComputeSha256(keyword)}:{page}",
            $"/api/v2.1/films/search-by-keyword?keyword={Uri.EscapeDataString(keyword)}&page={page}",
            TimeSpan.FromMinutes(5), ct);

    public async Task<KpSimilarFilmResponse?> GetSimilarFilmsAsync(int filmId, CancellationToken ct = default)
        => await GetCachedAsync<KpSimilarFilmResponse>($"kp:similar:{filmId}", $"/api/v2.2/films/{filmId}/similars", TimeSpan.FromMinutes(30), ct);

    public async Task<List<KpStaffMember>> GetStaffAsync(int filmId, CancellationToken ct = default)
    {
        var result = await GetCachedAsync<List<KpStaffMember>>($"kp:staff:{filmId}", $"/api/v1/staff?filmId={filmId}", TimeSpan.FromMinutes(30), ct);
        return result ?? [];
    }

    public async Task<KpPersonResponse?> GetPersonAsync(int personId, CancellationToken ct = default)
        => await GetCachedAsync<KpPersonResponse>($"kp:person:{personId}", $"/api/v1/staff/{personId}", TimeSpan.FromHours(1), ct);

    public async Task<KpPersonSearchResponse?> SearchPersonsAsync(string name, int page = 1, CancellationToken ct = default)
        => await GetCachedAsync<KpPersonSearchResponse>(
            $"kp:persons:search:{ComputeSha256(name)}:{page}",
            $"/api/v1/persons?name={Uri.EscapeDataString(name)}&page={page}",
            TimeSpan.FromMinutes(10), ct);

    public async Task<KpFiltersResponse?> GetFiltersAsync(CancellationToken ct = default)
        => await GetCachedAsync<KpFiltersResponse>("kp:filters", "/api/v2.2/films/filters", TimeSpan.FromHours(24), ct);

    public async Task<KpSeasonResponse?> GetSeasonsAsync(int filmId, CancellationToken ct = default)
        => await GetCachedAsync<KpSeasonResponse>($"kp:seasons:{filmId}", $"/api/v2.2/films/{filmId}/seasons", TimeSpan.FromMinutes(30), ct);

    public async Task<KpImageResponse?> GetImagesAsync(int filmId, string type, CancellationToken ct = default)
        => await GetCachedAsync<KpImageResponse>($"kp:images:{filmId}:{type}", $"/api/v2.2/films/{filmId}/images?type={type}&page=1", TimeSpan.FromHours(1), ct);

    private async Task<T?> GetCachedAsync<T>(string cacheKey, string url, TimeSpan ttl, CancellationToken ct) where T : class
    {
        var cached = await cache.GetStringAsync(cacheKey, ct);
        if (cached is not null)
        {
            try { return JsonSerializer.Deserialize<T>(cached, JsonOpts); }
            catch { /* stale/corrupt cache — fall through */ }
        }

        try
        {
            var response = await httpClient.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Kinopoisk API returned {Status} for {Url}", (int)response.StatusCode, url);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);

            T? result;
            try
            {
                result = JsonSerializer.Deserialize<T>(json, JsonOpts);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Deserialize error for {Type} from {Url}. Body: {Body}",
                    typeof(T).Name, url, json[..Math.Min(json.Length, 500)]);
                return null;
            }

            if (result is not null)
            {
                await cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, ct);
            }

            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to fetch from Kinopoisk: {Url}", url);
            return null;
        }
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..16];
    }
}
