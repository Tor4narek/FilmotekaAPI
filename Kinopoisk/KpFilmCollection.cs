using System.Text.Json.Serialization;

namespace FilmotekaAPI.Kinopoisk;

public class KpFilmCollectionResponse
{
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("totalPages")] public int TotalPages { get; set; }
    [JsonPropertyName("items")] public List<KpFilmCollectionItem> Items { get; set; } = [];
}

public class KpFilmCollectionItem
{
    [JsonPropertyName("kinopoiskId")] public int KinopoiskId { get; set; }
    [JsonPropertyName("nameRu")] public string? NameRu { get; set; }
    [JsonPropertyName("nameEn")] public string? NameEn { get; set; }
    [JsonPropertyName("nameOriginal")] public string? NameOriginal { get; set; }
    [JsonPropertyName("countries")] public List<KpCountry> Countries { get; set; } = [];
    [JsonPropertyName("genres")] public List<KpGenre> Genres { get; set; } = [];
    [JsonPropertyName("ratingKinopoisk")] public double? RatingKinopoisk { get; set; }
    [JsonPropertyName("ratingImdb")] public double? RatingImdb { get; set; }
    [JsonPropertyName("year")] public int? Year { get; set; }      // int, не string
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("posterUrl")] public string? PosterUrl { get; set; }
    [JsonPropertyName("posterUrlPreview")] public string? PosterUrlPreview { get; set; }
    [JsonPropertyName("coverUrl")] public string? CoverUrl { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
}

public class KpFilmSearchByFiltersResponse
{
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("totalPages")] public int TotalPages { get; set; }
    [JsonPropertyName("items")] public List<KpFilmFilterItem> Items { get; set; } = [];
}

public class KpFilmFilterItem
{
    [JsonPropertyName("kinopoiskId")] public int KinopoiskId { get; set; }
    [JsonPropertyName("imdbId")] public string? ImdbId { get; set; }
    [JsonPropertyName("nameRu")] public string? NameRu { get; set; }
    [JsonPropertyName("nameEn")] public string? NameEn { get; set; }
    [JsonPropertyName("nameOriginal")] public string? NameOriginal { get; set; }
    [JsonPropertyName("countries")] public List<KpCountry> Countries { get; set; } = [];
    [JsonPropertyName("genres")] public List<KpGenre> Genres { get; set; } = [];
    [JsonPropertyName("ratingKinopoisk")] public double? RatingKinopoisk { get; set; }
    [JsonPropertyName("ratingImdb")] public double? RatingImdb { get; set; }
    [JsonPropertyName("year")] public double? Year { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("posterUrl")] public string? PosterUrl { get; set; }
    [JsonPropertyName("posterUrlPreview")] public string? PosterUrlPreview { get; set; }
}

public class KpFilmSearchResponse
{
    [JsonPropertyName("keyword")] public string? Keyword { get; set; }
    [JsonPropertyName("pagesCount")] public int PagesCount { get; set; }
    [JsonPropertyName("searchFilmsCountResult")] public int SearchFilmsCountResult { get; set; }
    [JsonPropertyName("films")] public List<KpFilmSearchItem> Films { get; set; } = [];
}

public class KpFilmSearchItem
{
    [JsonPropertyName("filmId")] public int FilmId { get; set; }
    [JsonPropertyName("nameRu")] public string? NameRu { get; set; }
    [JsonPropertyName("nameEn")] public string? NameEn { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("year")] public string? Year { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("filmLength")] public string? FilmLength { get; set; }
    [JsonPropertyName("countries")] public List<KpCountry> Countries { get; set; } = [];
    [JsonPropertyName("genres")] public List<KpGenre> Genres { get; set; } = [];
    [JsonPropertyName("rating")] public string? Rating { get; set; }
    [JsonPropertyName("ratingVoteCount")] public int? RatingVoteCount { get; set; }
    [JsonPropertyName("posterUrl")] public string? PosterUrl { get; set; }
    [JsonPropertyName("posterUrlPreview")] public string? PosterUrlPreview { get; set; }
}

public class KpSimilarFilmResponse
{
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("items")] public List<KpSimilarFilmItem> Items { get; set; } = [];
}

public class KpSimilarFilmItem
{
    [JsonPropertyName("filmId")] public int FilmId { get; set; }
    [JsonPropertyName("nameRu")] public string? NameRu { get; set; }
    [JsonPropertyName("nameEn")] public string? NameEn { get; set; }
    [JsonPropertyName("nameOriginal")] public string? NameOriginal { get; set; }
    [JsonPropertyName("posterUrl")] public string? PosterUrl { get; set; }
    [JsonPropertyName("posterUrlPreview")] public string? PosterUrlPreview { get; set; }
    [JsonPropertyName("relationType")] public string? RelationType { get; set; }
}
