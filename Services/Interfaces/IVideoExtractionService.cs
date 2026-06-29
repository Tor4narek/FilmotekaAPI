namespace FilmotekaAPI.Services.Interfaces;

public interface IVideoExtractionService
{
    Task<string?> ExtractAsync(string iframeUrl, CancellationToken ct = default);
}
