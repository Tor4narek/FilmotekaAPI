using FilmotekaAPI.Kinopoisk;

namespace FilmotekaAPI.Services.Interfaces;

public interface IKinopoiskService
{
    Task<KpFilm?> GetFilmAsync(int id, CancellationToken ct = default);
    Task<KpFilmSearchByFiltersResponse?> GetFilmsAsync(Dictionary<string, string> queryParams, CancellationToken ct = default);
    Task<KpFilmCollectionResponse?> GetCollectionAsync(string collectionType, int page = 1, CancellationToken ct = default);
    Task<KpFilmSearchResponse?> SearchFilmsAsync(string keyword, int page = 1, CancellationToken ct = default);
    Task<KpSimilarFilmResponse?> GetSimilarFilmsAsync(int filmId, CancellationToken ct = default);
    Task<List<KpStaffMember>> GetStaffAsync(int filmId, CancellationToken ct = default);
    Task<KpPersonResponse?> GetPersonAsync(int personId, CancellationToken ct = default);
    Task<KpPersonSearchResponse?> SearchPersonsAsync(string name, int page = 1, CancellationToken ct = default);
    Task<KpFiltersResponse?> GetFiltersAsync(CancellationToken ct = default);
    Task<KpSeasonResponse?> GetSeasonsAsync(int filmId, CancellationToken ct = default);
    Task<KpImageResponse?> GetImagesAsync(int filmId, string type, CancellationToken ct = default);
}
