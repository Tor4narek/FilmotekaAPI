using System.Text.Json.Serialization;

namespace FilmotekaAPI.Kinopoisk;

public class KpStaffMember
{
    [JsonPropertyName("staffId")] public int StaffId { get; set; }
    [JsonPropertyName("nameRu")] public string? NameRu { get; set; }
    [JsonPropertyName("nameEn")] public string? NameEn { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("posterUrl")] public string? PosterUrl { get; set; }
    [JsonPropertyName("professionText")] public string? ProfessionText { get; set; }
    [JsonPropertyName("professionKey")] public string? ProfessionKey { get; set; }
}
