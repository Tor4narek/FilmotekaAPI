using FilmotekaAPI.DTOs.Common;
using FilmotekaAPI.DTOs.Films;
using FilmotekaAPI.DTOs.Persons;
using FilmotekaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FilmotekaAPI.Controllers;

[ApiController]
[Route("api/persons")]
public class PersonsController(IKinopoiskService kp) : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult> SearchPersons(
        [FromQuery] string name,
        [FromQuery] int page = 1,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "Query parameter 'name' is required." });

        var result = await kp.SearchPersonsAsync(name, page, ct);
        if (result is null) return Ok(new { total = 0, items = Array.Empty<object>() });

        return Ok(new
        {
            total = result.Total,
            items = result.Items.Select(p => new
            {
                id = p.KinopoiskId,
                name = p.NameRu ?? p.NameEn ?? string.Empty,
                originalName = p.NameEn,
                sex = p.Sex,
                photoUrl = p.PosterUrl
            })
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PersonDto>> GetPerson(int id, CancellationToken ct)
    {
        var person = await kp.GetPersonAsync(id, ct);
        if (person is null) return NotFound();

        return Ok(new PersonDto
        {
            Id = person.PersonId,
            Name = person.NameRu ?? person.NameEn ?? string.Empty,
            OriginalName = person.NameEn,
            BirthDate = person.Birthday,
            BirthPlace = person.Birthplace,
            PhotoUrl = person.PosterUrl,
            Bio = string.Join(" ", person.Facts),
            Professions = (person.Profession ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList()
        });
    }

    [HttpGet("{id:int}/filmography")]
    public async Task<ActionResult<PaginatedResponse<FilmDto>>> GetFilmography(
        int id,
        [FromQuery] FilmographyParams p,
        CancellationToken ct)
    {
        var person = await kp.GetPersonAsync(id, ct);
        if (person is null) return NotFound();

        var films = person.Films.AsEnumerable();

        if (!string.IsNullOrEmpty(p.Type))
        {
            var kpType = p.Type.ToLower() == "serial" ? "TV_SERIES" : "FILM";
            // PersonResponse.films don't have type — filter isn't available here, return all.
        }

        films = p.Sort.ToLower() switch
        {
            "year" when p.Order.ToLower() == "asc" => films.OrderBy(f => f.FilmId),
            _ => films.OrderByDescending(f => f.FilmId)
        };

        var total = films.Count();
        var paged = films.Skip((p.Page - 1) * p.Limit).Take(p.Limit).ToList();

        return Ok(new PaginatedResponse<FilmDto>
        {
            Data = paged.Select(f => new FilmDto
            {
                Id = f.FilmId,
                Title = f.NameRu ?? f.NameEn ?? string.Empty,
                OriginalTitle = f.NameEn,
                Type = "film",
                Rating = double.TryParse(f.Rating, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : null
            }).ToList(),
            Meta = new DTOs.Common.PaginationMeta
            {
                Page = p.Page,
                Limit = p.Limit,
                Total = total,
                TotalPages = (int)Math.Ceiling((double)total / p.Limit)
            }
        });
    }
}
