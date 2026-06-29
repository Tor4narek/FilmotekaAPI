using FilmotekaAPI.DTOs.Common;
using FilmotekaAPI.DTOs.Films;

namespace FilmotekaAPI.Services.Interfaces;

public interface IFilmService
{
    Task<PaginatedResponse<FilmDto>> GetFilmsAsync(FilmQueryParams p, CancellationToken ct = default);
    Task<FilmDetailDto?> GetFeaturedAsync(CancellationToken ct = default);
    Task<SlidersResponse> GetSlidersAsync(CancellationToken ct = default);
    Task<PaginatedResponse<FilmDto>> SearchFilmsAsync(string q, int page, int limit, CancellationToken ct = default);
    Task<FilmDetailDto?> GetFilmByIdAsync(int id, CancellationToken ct = default);
    Task<List<FilmDto>> GetSimilarFilmsAsync(int id, int limit, CancellationToken ct = default);
    Task<PaginatedResponse<FilmDto>> GetCollectionAsync(string type, int page, CancellationToken ct = default);
}
