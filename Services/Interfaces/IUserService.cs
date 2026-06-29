using FilmotekaAPI.DTOs.Common;
using FilmotekaAPI.DTOs.Films;
using FilmotekaAPI.DTOs.Users;

namespace FilmotekaAPI.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task<string> UpdateAvatarAsync(Guid id, IFormFile file, CancellationToken ct = default);

    Task<PaginatedResponse<FilmDto>> GetWatchlistAsync(Guid userId, string? type, int page, int limit, CancellationToken ct = default);
    Task AddToWatchlistAsync(Guid userId, int filmId, CancellationToken ct = default);
    Task RemoveFromWatchlistAsync(Guid userId, int filmId, CancellationToken ct = default);

    Task<PaginatedResponse<FilmDto>> GetFavoritesAsync(Guid userId, string? type, int page, int limit, CancellationToken ct = default);
    Task AddToFavoritesAsync(Guid userId, int filmId, CancellationToken ct = default);
    Task RemoveFromFavoritesAsync(Guid userId, int filmId, CancellationToken ct = default);

    Task<PaginatedResponse<FilmDto>> GetHistoryAsync(Guid userId, int page, int limit, CancellationToken ct = default);
    Task AddToHistoryAsync(Guid userId, int filmId, CancellationToken ct = default);
    Task RemoveFromHistoryAsync(Guid userId, int filmId, CancellationToken ct = default);
    Task ClearHistoryAsync(Guid userId, CancellationToken ct = default);
}
