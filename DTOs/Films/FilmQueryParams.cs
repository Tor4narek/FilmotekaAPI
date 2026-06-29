namespace FilmotekaAPI.DTOs.Films;

public class FilmQueryParams
{
    public string? Type { get; set; }
    public List<string>? Genre { get; set; }
    public List<string>? Country { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public int? RatingMin { get; set; }
    public bool? HasSubtitles { get; set; }
    public List<string>? Quality { get; set; }
    public string Sort { get; set; } = "rating";
    public string Order { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}
