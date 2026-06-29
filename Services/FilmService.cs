using System.Text.Json;
using FilmotekaAPI.DTOs.Common;
using FilmotekaAPI.DTOs.Films;
using FilmotekaAPI.Kinopoisk;
using FilmotekaAPI.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace FilmotekaAPI.Services;

public class FilmService(
    IKinopoiskService kp,
    IDistributedCache cache) : IFilmService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public async Task<PaginatedResponse<FilmDto>> GetFilmsAsync(FilmQueryParams p, CancellationToken ct = default)
    {
        var filters = await kp.GetFiltersAsync(ct);

        var query = new Dictionary<string, string>
        {
            ["page"] = p.Page.ToString(),
        };

        if (!string.IsNullOrEmpty(p.Type))
            query["type"] = MapType(p.Type);

        if (p.Genre?.Count > 0 && filters is not null)
        {
            var genreId = filters.Genres.FirstOrDefault(g => string.Equals(g.Genre, p.Genre[0], StringComparison.OrdinalIgnoreCase))?.Id;
            if (genreId.HasValue) query["genres"] = genreId.Value.ToString();
        }

        if (p.Country?.Count > 0 && filters is not null)
        {
            var countryId = filters.Countries.FirstOrDefault(c => string.Equals(c.Country, p.Country[0], StringComparison.OrdinalIgnoreCase))?.Id;
            if (countryId.HasValue) query["countries"] = countryId.Value.ToString();
        }

        if (p.YearFrom.HasValue) query["yearFrom"] = p.YearFrom.Value.ToString();
        if (p.YearTo.HasValue) query["yearTo"] = p.YearTo.Value.ToString();

        if (p.RatingMin.HasValue)
        {
            query["ratingFrom"] = p.RatingMin.Value.ToString();
            query["ratingTo"] = "10";
        }

        query["order"] = MapSort(p.Sort);

        var response = await kp.GetFilmsAsync(query, ct);
        if (response is null)
            return new PaginatedResponse<FilmDto> { Meta = new PaginationMeta { Page = p.Page, Limit = p.Limit } };

        return new PaginatedResponse<FilmDto>
        {
            Data = response.Items.Select(MapFilterItemToDto).ToList(),
            Meta = new PaginationMeta
            {
                Page = p.Page,
                Limit = 20,
                Total = response.Total,
                TotalPages = response.TotalPages
            }
        };
    }

    public async Task<FilmDetailDto?> GetFeaturedAsync(CancellationToken ct = default)
    {
        var cached = await cache.GetStringAsync("app:featured", ct);
        if (cached is not null)
        {
            try { return JsonSerializer.Deserialize<FilmDetailDto>(cached, JsonOpts); }
            catch { /* fall through */ }
        }

        var collection = await kp.GetCollectionAsync("TOP_250_BEST_FILMS", 1, ct);
        var first = collection?.Items.FirstOrDefault();
        if (first is null) return null;

        var detail = await GetFilmByIdAsync(first.KinopoiskId, ct);
        if (detail is not null)
        {
            await cache.SetStringAsync("app:featured", JsonSerializer.Serialize(detail, JsonOpts),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) }, ct);
        }

        return detail;
    }

    public async Task<SlidersResponse> GetSlidersAsync(CancellationToken ct = default)
    {
        var cached = await cache.GetStringAsync("app:sliders", ct);
        if (cached is not null)
        {
            try { return JsonSerializer.Deserialize<SlidersResponse>(cached, JsonOpts) ?? new(); }
            catch { /* fall through */ }
        }

        var t1 = kp.GetCollectionAsync("TOP_250_BEST_FILMS", 1, ct);
        var t2 = kp.GetCollectionAsync("TOP_100_POPULAR_FILMS", 1, ct);
        var t3 = kp.GetCollectionAsync("TOP_AWAIT_FILMS", 1, ct);
        var t4 = kp.GetFilmsAsync(new Dictionary<string, string> { ["type"] = "TV_SERIES", ["order"] = "YEAR", ["page"] = "1" }, ct);
        await Task.WhenAll(t1, t2, t3, t4);
        var top250 = t1.Result;
        var popular = t2.Result;
        var awaited = t3.Result;
        var newSerials = t4.Result;

        var sliders = new SlidersResponse
        {
            Medium = top250?.Items.Take(10).Select(MapCollectionItemToDto).ToList() ?? [],
            Small = popular?.Items.Take(10).Select(MapCollectionItemToDto).ToList() ?? [],
            Big = awaited?.Items.Take(10).Select(MapCollectionItemToDto).ToList() ?? [],
            NewEpisodes = newSerials?.Items.Take(10).Select(MapFilterItemToDto).ToList() ?? []
        };

        await cache.SetStringAsync("app:sliders", JsonSerializer.Serialize(sliders, JsonOpts),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) }, ct);

        return sliders;
    }

    public async Task<PaginatedResponse<FilmDto>> SearchFilmsAsync(string q, int page, int limit, CancellationToken ct = default)
    {
        var response = await kp.SearchFilmsAsync(q, page, ct);
        if (response is null)
            return new PaginatedResponse<FilmDto> { Meta = new PaginationMeta { Page = page, Limit = limit } };

        return new PaginatedResponse<FilmDto>
        {
            Data = response.Films.Select(MapSearchItemToDto).ToList(),
            Meta = new PaginationMeta
            {
                Page = page,
                Limit = limit,
                Total = response.SearchFilmsCountResult,
                TotalPages = response.PagesCount
            }
        };
    }

    public async Task<FilmDetailDto?> GetFilmByIdAsync(int id, CancellationToken ct = default)
    {
        var filmTask       = kp.GetFilmAsync(id, ct);
        var staffTask      = kp.GetStaffAsync(id, ct);
        var wallpapersTask = kp.GetImagesAsync(id, "WALLPAPER",   ct);
        var screensTask    = kp.GetImagesAsync(id, "SCREENSHOT",   ct);
        await Task.WhenAll(filmTask, staffTask, wallpapersTask, screensTask);

        var film = filmTask.Result;
        var staff = staffTask.Result;
        if (film is null) return null;

        int? episodesCount = null;
        if (film.Serial == true || film.Type is "TV_SERIES" or "MINI_SERIES")
        {
            var seasons = await kp.GetSeasonsAsync(id, ct);
            episodesCount = seasons?.Items.Sum(s => s.Episodes.Count);
        }

        // Merge WALLPAPER + SCREENSHOT into a single images list
        var images = (wallpapersTask.Result?.Items ?? [])
            .Concat(screensTask.Result?.Items ?? [])
            .Select(i => new FilmImageDto { ImageUrl = i.ImageUrl, PreviewUrl = i.PreviewUrl })
            .ToList();

        var director = staff.FirstOrDefault(s => s.ProfessionKey == "DIRECTOR");
        var cast = staff.Where(s => s.ProfessionKey == "ACTOR").Take(20).ToList();

        return new FilmDetailDto
        {
            Id = film.KinopoiskId,
            Title = film.NameRu ?? film.NameEn ?? film.NameOriginal ?? string.Empty,
            OriginalTitle = film.NameOriginal,
            Type = MapKpType(film.Type),
            Year = film.Year ?? film.StartYear,
            Genres = film.Genres.Select(g => g.Genre).ToList(),
            Countries = film.Countries.Select(c => c.Country).ToList(),
            Duration = film.FilmLength,
            Seasons = film.Serial == true ? null : null,
            Rating = film.RatingKinopoisk,
            PosterUrl = film.PosterUrl,
            BackdropUrl = film.CoverUrl,
            Description = film.Description ?? film.ShortDescription,
            Director = director is null ? null : new PersonRefDto { Id = director.StaffId, Name = director.NameRu ?? director.NameEn ?? string.Empty },
            Cast = cast.Select(s => new CastMemberDto
            {
                Id = s.StaffId,
                Name = s.NameRu ?? s.NameEn ?? string.Empty,
                Role = s.Description,
                PhotoUrl = s.PosterUrl
            }).ToList(),
            HasSubtitles = false,
            Quality = null,
            EpisodesCount = episodesCount,
            Images = images
        };
    }

    public async Task<PaginatedResponse<FilmDto>> GetCollectionAsync(string type, int page, CancellationToken ct = default)
    {
        var response = await kp.GetCollectionAsync(type, page, ct);
        if (response is null)
            return new PaginatedResponse<FilmDto> { Meta = new PaginationMeta { Page = page, Limit = 20 } };

        return new PaginatedResponse<FilmDto>
        {
            Data = response.Items.Select(MapCollectionItemToDto).ToList(),
            Meta = new PaginationMeta
            {
                Page = page,
                Limit = 20,
                Total = response.Total,
                TotalPages = response.TotalPages
            }
        };
    }

    public async Task<List<FilmDto>> GetSimilarFilmsAsync(int id, int limit, CancellationToken ct = default)
    {
        var response = await kp.GetSimilarFilmsAsync(id, ct);
        return response?.Items.Take(limit).Select(s => new FilmDto
        {
            Id = s.FilmId,
            Title = s.NameRu ?? s.NameEn ?? s.NameOriginal ?? string.Empty,
            OriginalTitle = s.NameOriginal,
            Type = "film",
            PosterUrl = s.PosterUrl,
            BackdropUrl = null
        }).ToList() ?? [];
    }

    private static FilmDto MapCollectionItemToDto(KpFilmCollectionItem item) => new()
    {
        Id = item.KinopoiskId,
        Title = item.NameRu ?? item.NameEn ?? item.NameOriginal ?? string.Empty,
        OriginalTitle = item.NameOriginal,
        Type = MapKpType(item.Type),
        Year = item.Year,
        Genres = item.Genres.Select(g => g.Genre).ToList(),
        Countries = item.Countries.Select(c => c.Country).ToList(),
        Rating = item.RatingKinopoisk,
        PosterUrl = item.PosterUrl,
        BackdropUrl = item.CoverUrl
    };

    private static FilmDto MapFilterItemToDto(KpFilmFilterItem item) => new()
    {
        Id = item.KinopoiskId,
        Title = item.NameRu ?? item.NameEn ?? item.NameOriginal ?? string.Empty,
        OriginalTitle = item.NameOriginal,
        Type = MapKpType(item.Type),
        Year = item.Year.HasValue ? (int)item.Year.Value : null,
        Genres = item.Genres.Select(g => g.Genre).ToList(),
        Countries = item.Countries.Select(c => c.Country).ToList(),
        Rating = item.RatingKinopoisk,
        PosterUrl = item.PosterUrl,
        BackdropUrl = null
    };

    private static FilmDto MapSearchItemToDto(KpFilmSearchItem item) => new()
    {
        Id = item.FilmId,
        Title = item.NameRu ?? item.NameEn ?? string.Empty,
        OriginalTitle = item.NameEn,
        Type = MapKpType(item.Type),
        Year = int.TryParse(item.Year, out var y) ? y : null,
        Genres = item.Genres.Select(g => g.Genre).ToList(),
        Countries = item.Countries.Select(c => c.Country).ToList(),
        Rating = double.TryParse(item.Rating, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : null,
        PosterUrl = item.PosterUrl,
        BackdropUrl = null
    };

    private static string MapType(string type) => type.ToLower() switch
    {
        "serial" => "TV_SERIES",
        "film" => "FILM",
        _ => "ALL"
    };

    private static string MapKpType(string? kpType) => kpType?.ToUpper() switch
    {
        "TV_SERIES" or "MINI_SERIES" or "TV_SHOW" => "serial",
        _ => "film"
    };

    private static string MapSort(string sort) => sort.ToLower() switch
    {
        "year" => "YEAR",
        "title" => "RATING",
        _ => "RATING"
    };
}
