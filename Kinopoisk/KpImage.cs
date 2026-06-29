namespace FilmotekaAPI.Kinopoisk;

public class KpImageResponse
{
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public List<KpImageItem> Items { get; set; } = [];
}

public class KpImageItem
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? PreviewUrl { get; set; }
}
