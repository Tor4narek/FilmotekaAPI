namespace FilmotekaAPI.Entities;

public class WatchHistoryItem
{
    public Guid UserId { get; set; }
    public int FilmId { get; set; }
    public DateTime WatchedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
}
