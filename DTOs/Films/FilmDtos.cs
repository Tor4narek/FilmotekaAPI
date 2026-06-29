namespace FilmotekaAPI.DTOs.Films;

public class FilmDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? OriginalTitle { get; set; }
    public string Type { get; set; } = string.Empty;
    public int? Year { get; set; }
    public List<string> Genres { get; set; } = [];
    public List<string> Countries { get; set; } = [];
    public int? Duration { get; set; }
    public int? Seasons { get; set; }
    public double? Rating { get; set; }
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
}

public class FilmDetailDto : FilmDto
{
    public string? Description { get; set; }
    public PersonRefDto? Director { get; set; }
    public List<CastMemberDto> Cast { get; set; } = [];
    public bool HasSubtitles { get; set; }
    public string? Quality { get; set; }
    public int? EpisodesCount { get; set; }
    public List<FilmImageDto> Images { get; set; } = [];
}

public class FilmImageDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? PreviewUrl { get; set; }
}

public class PersonRefDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CastMemberDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? PhotoUrl { get; set; }
}

public class SlidersResponse
{
    public List<FilmDto> Medium { get; set; } = [];
    public List<FilmDto> Small { get; set; } = [];
    public List<FilmDto> Big { get; set; } = [];
    public List<FilmDto> NewEpisodes { get; set; } = [];
}

public class CommentDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public CommentUserDto User { get; set; } = null!;
    public int LikesCount { get; set; }
}

public class CommentUserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

public class CreateCommentRequest
{
    public string Text { get; set; } = string.Empty;
}
