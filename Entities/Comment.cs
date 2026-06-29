namespace FilmotekaAPI.Entities;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int FilmId { get; set; }
    public Guid UserId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
