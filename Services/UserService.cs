using FilmotekaAPI.Data;
using FilmotekaAPI.DTOs.Common;
using FilmotekaAPI.DTOs.Films;
using FilmotekaAPI.DTOs.Users;
using FilmotekaAPI.Entities;
using FilmotekaAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FilmotekaAPI.Services;

public class UserService(AppDbContext db, IFilmService filmService, IWebHostEnvironment env) : IUserService
{
    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync([id], ct);
        if (user is null) return null;

        var watchlistCount = await db.WatchlistItems.CountAsync(w => w.UserId == id, ct);
        var favoritesCount = await db.FavoriteItems.CountAsync(f => f.UserId == id, ct);

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            WatchlistCount = watchlistCount,
            FavoritesCount = favoritesCount
        };
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (request.Name is not null) user.Name = request.Name.Trim();

        if (request.Email is not null)
        {
            var newEmail = request.Email.ToLower().Trim();
            if (await db.Users.AnyAsync(u => u.Email == newEmail && u.Id != id, ct))
                throw new InvalidOperationException("Email already in use.");
            user.Email = newEmail;
        }

        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<string> UpdateAvatarAsync(Guid id, IFormFile file, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("User not found.");

        var uploadsDir = Path.Combine(env.WebRootPath ?? "wwwroot", "avatars");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(file.FileName).ToLower();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(ext)) throw new InvalidOperationException("Unsupported image format.");

        var fileName = $"{id}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream, ct);

        user.AvatarUrl = $"/avatars/{fileName}";
        await db.SaveChangesAsync(ct);

        return user.AvatarUrl;
    }

    public async Task<PaginatedResponse<FilmDto>> GetWatchlistAsync(Guid userId, string? type, int page, int limit, CancellationToken ct = default)
        => await GetFilmListAsync(db.WatchlistItems.Where(w => w.UserId == userId).Select(w => w.FilmId), type, page, limit, ct);

    public async Task AddToWatchlistAsync(Guid userId, int filmId, CancellationToken ct = default)
    {
        if (!await db.WatchlistItems.AnyAsync(w => w.UserId == userId && w.FilmId == filmId, ct))
        {
            db.WatchlistItems.Add(new WatchlistItem { UserId = userId, FilmId = filmId });
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task RemoveFromWatchlistAsync(Guid userId, int filmId, CancellationToken ct = default)
    {
        var item = await db.WatchlistItems.FindAsync([userId, filmId], ct);
        if (item is not null)
        {
            db.WatchlistItems.Remove(item);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<PaginatedResponse<FilmDto>> GetFavoritesAsync(Guid userId, string? type, int page, int limit, CancellationToken ct = default)
        => await GetFilmListAsync(db.FavoriteItems.Where(f => f.UserId == userId).Select(f => f.FilmId), type, page, limit, ct);

    public async Task AddToFavoritesAsync(Guid userId, int filmId, CancellationToken ct = default)
    {
        if (!await db.FavoriteItems.AnyAsync(f => f.UserId == userId && f.FilmId == filmId, ct))
        {
            db.FavoriteItems.Add(new FavoriteItem { UserId = userId, FilmId = filmId });
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task RemoveFromFavoritesAsync(Guid userId, int filmId, CancellationToken ct = default)
    {
        var item = await db.FavoriteItems.FindAsync([userId, filmId], ct);
        if (item is not null)
        {
            db.FavoriteItems.Remove(item);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<PaginatedResponse<FilmDto>> GetHistoryAsync(Guid userId, int page, int limit, CancellationToken ct = default)
    {
        var query = db.WatchHistoryItems
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.WatchedAt)
            .Select(h => h.FilmId);

        return await GetFilmListAsync(query, null, page, limit, ct);
    }

    public async Task AddToHistoryAsync(Guid userId, int filmId, CancellationToken ct = default)
    {
        var existing = await db.WatchHistoryItems.FindAsync([userId, filmId], ct);
        if (existing is not null)
        {
            existing.WatchedAt = DateTime.UtcNow;
        }
        else
        {
            db.WatchHistoryItems.Add(new WatchHistoryItem { UserId = userId, FilmId = filmId });
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveFromHistoryAsync(Guid userId, int filmId, CancellationToken ct = default)
    {
        var item = await db.WatchHistoryItems.FindAsync([userId, filmId], ct);
        if (item is not null)
        {
            db.WatchHistoryItems.Remove(item);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task ClearHistoryAsync(Guid userId, CancellationToken ct = default)
    {
        var items = await db.WatchHistoryItems.Where(h => h.UserId == userId).ToListAsync(ct);
        if (items.Count > 0)
        {
            db.WatchHistoryItems.RemoveRange(items);
            await db.SaveChangesAsync(ct);
        }
    }

    private async Task<PaginatedResponse<FilmDto>> GetFilmListAsync(
        IQueryable<int> filmIdQuery, string? type, int page, int limit, CancellationToken ct)
    {
        var allIds = await filmIdQuery.ToListAsync(ct);
        var total = allIds.Count;

        var pagedIds = allIds.Skip((page - 1) * limit).Take(limit).ToList();

        var films = new List<FilmDto>();
        foreach (var id in pagedIds)
        {
            var film = await filmService.GetFilmByIdAsync(id, ct);
            if (film is null) continue;
            if (type is not null && !string.Equals(film.Type, type, StringComparison.OrdinalIgnoreCase)) continue;
            films.Add(film);
        }

        return new PaginatedResponse<FilmDto>
        {
            Data = films,
            Meta = new DTOs.Common.PaginationMeta
            {
                Page = page,
                Limit = limit,
                Total = total,
                TotalPages = (int)Math.Ceiling((double)total / limit)
            }
        };
    }
}
