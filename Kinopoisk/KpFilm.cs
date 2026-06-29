using System.Text.Json.Serialization;

namespace FilmotekaAPI.Kinopoisk;

public class KpFilm
{
    [JsonPropertyName("kinopoiskId")] public int KinopoiskId { get; set; }
    [JsonPropertyName("nameRu")] public string? NameRu { get; set; }
    [JsonPropertyName("nameEn")] public string? NameEn { get; set; }
    [JsonPropertyName("nameOriginal")] public string? NameOriginal { get; set; }
    [JsonPropertyName("posterUrl")] public string? PosterUrl { get; set; }
    [JsonPropertyName("posterUrlPreview")] public string? PosterUrlPreview { get; set; }
    [JsonPropertyName("coverUrl")] public string? CoverUrl { get; set; }
    [JsonPropertyName("logoUrl")] public string? LogoUrl { get; set; }
    [JsonPropertyName("reviewsCount")] public int ReviewsCount { get; set; }
    [JsonPropertyName("ratingGoodReview")] public double? RatingGoodReview { get; set; }
    [JsonPropertyName("ratingKinopoisk")] public double? RatingKinopoisk { get; set; }
    [JsonPropertyName("ratingKinopoiskVoteCount")] public int? RatingKinopoiskVoteCount { get; set; }
    [JsonPropertyName("ratingImdb")] public double? RatingImdb { get; set; }
    [JsonPropertyName("ratingImdbVoteCount")] public int? RatingImdbVoteCount { get; set; }
    [JsonPropertyName("ratingFilmCritics")] public double? RatingFilmCritics { get; set; }
    [JsonPropertyName("ratingAwait")] public double? RatingAwait { get; set; }
    [JsonPropertyName("ratingRfCritics")] public double? RatingRfCritics { get; set; }
    [JsonPropertyName("webUrl")] public string? WebUrl { get; set; }
    [JsonPropertyName("year")] public int? Year { get; set; }
    [JsonPropertyName("filmLength")] public int? FilmLength { get; set; }
    [JsonPropertyName("slogan")] public string? Slogan { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("shortDescription")] public string? ShortDescription { get; set; }
    [JsonPropertyName("editorAnnotation")] public string? EditorAnnotation { get; set; }
    [JsonPropertyName("isTicketsAvailable")] public bool IsTicketsAvailable { get; set; }
    [JsonPropertyName("productionStatus")] public string? ProductionStatus { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("ratingMpaa")] public string? RatingMpaa { get; set; }
    [JsonPropertyName("ratingAgeLimits")] public string? RatingAgeLimits { get; set; }
    [JsonPropertyName("hasImax")] public bool HasImax { get; set; }
    [JsonPropertyName("has3D")] public bool Has3D { get; set; }
    [JsonPropertyName("lastSync")] public string? LastSync { get; set; }
    [JsonPropertyName("countries")] public List<KpCountry> Countries { get; set; } = [];
    [JsonPropertyName("genres")] public List<KpGenre> Genres { get; set; } = [];
    [JsonPropertyName("startYear")] public int? StartYear { get; set; }
    [JsonPropertyName("endYear")] public int? EndYear { get; set; }
    [JsonPropertyName("serial")] public bool? Serial { get; set; }
    [JsonPropertyName("shortFilm")] public bool? ShortFilm { get; set; }
    [JsonPropertyName("completed")] public bool? Completed { get; set; }
    [JsonPropertyName("imdbId")] public string? ImdbId { get; set; }
}

public class KpCountry
{
    [JsonPropertyName("country")] public string Country { get; set; } = string.Empty;
}

public class KpGenre
{
    [JsonPropertyName("genre")] public string Genre { get; set; } = string.Empty;
}
