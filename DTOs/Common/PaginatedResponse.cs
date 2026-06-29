namespace FilmotekaAPI.DTOs.Common;

public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = [];
    public PaginationMeta Meta { get; set; } = new();
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
}
