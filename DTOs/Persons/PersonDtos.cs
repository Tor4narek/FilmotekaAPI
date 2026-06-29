namespace FilmotekaAPI.DTOs.Persons;

public class PersonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? OriginalName { get; set; }
    public string? BirthDate { get; set; }
    public string? BirthPlace { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Bio { get; set; }
    public List<string> Professions { get; set; } = [];
}

public class FilmographyParams
{
    public string? Type { get; set; }
    public string Sort { get; set; } = "year";
    public string Order { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}
