using FilmotekaAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace FilmotekaAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<WatchlistItem> WatchlistItems => Set<WatchlistItem>();
    public DbSet<FavoriteItem> FavoriteItems => Set<FavoriteItem>();
    public DbSet<SupportRequest> SupportRequests => Set<SupportRequest>();
    public DbSet<WatchHistoryItem> WatchHistoryItems => Set<WatchHistoryItem>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Name).HasColumnName("name").IsRequired();
            e.Property(u => u.Email).HasColumnName("email").IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            e.Property(u => u.AvatarUrl).HasColumnName("avatar_url");
            e.Property(u => u.CreatedAt).HasColumnName("created_at");
        });

        mb.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.UserId).HasColumnName("user_id");
            e.Property(r => r.Token).HasColumnName("token").IsRequired();
            e.HasIndex(r => r.Token).IsUnique();
            e.Property(r => r.ExpiresAt).HasColumnName("expires_at");
            e.Property(r => r.RevokedAt).HasColumnName("revoked_at");
            e.Property(r => r.CreatedAt).HasColumnName("created_at");
            e.HasOne(r => r.User).WithMany(u => u.RefreshTokens).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<Comment>(e =>
        {
            e.ToTable("comments");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.FilmId).HasColumnName("film_id");
            e.Property(c => c.UserId).HasColumnName("user_id");
            e.Property(c => c.Text).HasColumnName("text").IsRequired();
            e.Property(c => c.LikesCount).HasColumnName("likes_count").HasDefaultValue(0);
            e.Property(c => c.CreatedAt).HasColumnName("created_at");
            e.HasOne(c => c.User).WithMany(u => u.Comments).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(c => c.FilmId);
        });

        mb.Entity<WatchlistItem>(e =>
        {
            e.ToTable("watchlist");
            e.HasKey(w => new { w.UserId, w.FilmId });
            e.Property(w => w.UserId).HasColumnName("user_id");
            e.Property(w => w.FilmId).HasColumnName("film_id");
            e.Property(w => w.AddedAt).HasColumnName("added_at");
            e.HasOne(w => w.User).WithMany(u => u.WatchlistItems).HasForeignKey(w => w.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<FavoriteItem>(e =>
        {
            e.ToTable("favorites");
            e.HasKey(f => new { f.UserId, f.FilmId });
            e.Property(f => f.UserId).HasColumnName("user_id");
            e.Property(f => f.FilmId).HasColumnName("film_id");
            e.Property(f => f.AddedAt).HasColumnName("added_at");
            e.HasOne(f => f.User).WithMany(u => u.FavoriteItems).HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<SupportRequest>(e =>
        {
            e.ToTable("support_requests");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("id");
            e.Property(s => s.Name).HasColumnName("name").IsRequired();
            e.Property(s => s.Email).HasColumnName("email").IsRequired();
            e.Property(s => s.Message).HasColumnName("message").IsRequired();
            e.Property(s => s.CreatedAt).HasColumnName("created_at");
        });

        mb.Entity<WatchHistoryItem>(e =>
        {
            e.ToTable("watch_history");
            e.HasKey(h => new { h.UserId, h.FilmId });
            e.Property(h => h.UserId).HasColumnName("user_id");
            e.Property(h => h.FilmId).HasColumnName("film_id");
            e.Property(h => h.WatchedAt).HasColumnName("watched_at");
            e.HasOne(h => h.User).WithMany(u => u.WatchHistoryItems).HasForeignKey(h => h.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
