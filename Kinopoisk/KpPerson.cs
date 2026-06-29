using System.Text.Json.Serialization;

namespace FilmotekaAPI.Kinopoisk;

public class KpPersonSearchResponse
{
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("items")] public List<KpPersonSearchItem> Items { get; set; } = [];
}

public class KpPersonSearchItem
{
    [JsonPropertyName("kinopoiskId")] public int KinopoiskId { get; set; }
    [JsonPropertyName("webUrl")] public string? WebUrl { get; set; }
    [JsonPropertyName("nameRu")] public string? NameRu { get; set; }
    [JsonPropertyName("nameEn")] public string? NameEn { get; set; }
    [JsonPropertyName("sex")] public string? Sex { get; set; }
    [JsonPropertyName("posterUrl")] public string? PosterUrl { get; set; }
}

public class KpPersonResponse
{
    [JsonPropertyName("personId")] public int PersonId { get; set; }
    [JsonPropertyName("webUrl")] public string? WebUrl { get; set; }
    [JsonPropertyName("nameRu")] public string? NameRu { get; set; }
    [JsonPropertyName("nameEn")] public string? NameEn { get; set; }
    [JsonPropertyName("sex")] public string? Sex { get; set; }
    [JsonPropertyName("posterUrl")] public string? PosterUrl { get; set; }
    [JsonPropertyName("growth")] public int? Growth { get; set; }
    [JsonPropertyName("birthday")] public string? Birthday { get; set; }
    [JsonPropertyName("death")] public string? Death { get; set; }
    [JsonPropertyName("age")] public int? Age { get; set; }
    [JsonPropertyName("birthplace")] public string? Birthplace { get; set; }
    [JsonPropertyName("deathplace")] public string? Deathplace { get; set; }
    [JsonPropertyName("hasAwards")] public int? HasAwards { get; set; }
    [JsonPropertyName("profession")] public string? Profession { get; set; }
    [JsonPropertyName("facts")] public List<string> Facts { get; set; } = [];
    [JsonPropertyName("films")] public List<KpPersonFilm> Films { get; set; } = [];
}

public class KpPersonFilm
{
    [JsonPropertyName("filmId")] public int FilmId { get; set; }
    [JsonPropertyName("nameRu")] public string? NameRu { get; set; }
    [JsonPropertyName("nameEn")] public string? NameEn { get; set; }
    [JsonPropertyName("rating")] public string? Rating { get; set; }
    [JsonPropertyName("general")] public bool General { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("professionKey")] public string? ProfessionKey { get; set; }
}
