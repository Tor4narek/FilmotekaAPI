using System.Text.Json.Serialization;

namespace FilmotekaAPI.Kinopoisk;

public class KpFiltersResponse
{
    [JsonPropertyName("genres")] public List<KpFilterGenre> Genres { get; set; } = [];
    [JsonPropertyName("countries")] public List<KpFilterCountry> Countries { get; set; } = [];
}

public class KpFilterGenre
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("genre")] public string Genre { get; set; } = string.Empty;
}

public class KpFilterCountry
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("country")] public string Country { get; set; } = string.Empty;
}

public class KpSeasonResponse
{
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("items")] public List<KpSeason> Items { get; set; } = [];
}

public class KpSeason
{
    [JsonPropertyName("number")] public int Number { get; set; }
    [JsonPropertyName("episodes")] public List<KpEpisode> Episodes { get; set; } = [];
}

public class KpEpisode
{
    [JsonPropertyName("seasonNumber")] public int SeasonNumber { get; set; }
    [JsonPropertyName("episodeNumber")] public int EpisodeNumber { get; set; }
    [JsonPropertyName("nameRu")] public string? NameRu { get; set; }
    [JsonPropertyName("nameEn")] public string? NameEn { get; set; }
    [JsonPropertyName("synopsis")] public string? Synopsis { get; set; }
    [JsonPropertyName("releaseDate")] public string? ReleaseDate { get; set; }
}
