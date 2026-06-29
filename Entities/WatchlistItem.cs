namespace FilmotekaAPI.Entities;

public class WatchlistItem
{
    public Guid UserId { get; set; }
    public int FilmId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
