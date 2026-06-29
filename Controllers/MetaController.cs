using FilmotekaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FilmotekaAPI.Controllers;

[ApiController]
[Route("api/meta")]
public class MetaController(IKinopoiskService kp) : ControllerBase
{
    [HttpGet("genres")]
    public async Task<IActionResult> GetGenres(CancellationToken ct)
    {
        var filters = await kp.GetFiltersAsync(ct);
        if (filters is null) return StatusCode(503, new { error = "Unable to fetch genres." });
        return Ok(filters.Genres.Select(g => new { id = g.Id, name = g.Genre }));
    }

    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries(CancellationToken ct)
    {
        var filters = await kp.GetFiltersAsync(ct);
        if (filters is null) return StatusCode(503, new { error = "Unable to fetch countries." });
        return Ok(filters.Countries.Select(c => new { id = c.Id, name = c.Country }));
    }

    [HttpGet("quality-options")]
    public IActionResult GetQualityOptions()
        => Ok(new[]
        {
            new { id = "hd", label = "HD 720p" },
            new { id = "4k", label = "4K Ultra HD" }
        });
}
