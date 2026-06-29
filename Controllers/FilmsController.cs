using System.Security.Claims;
using FilmotekaAPI.Data;
using FilmotekaAPI.DTOs.Common;
using FilmotekaAPI.DTOs.Films;
using FilmotekaAPI.Entities;
using FilmotekaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmotekaAPI.Controllers;

[ApiController]
[Route("api/films")]
public class FilmsController(IFilmService filmService, AppDbContext db, IKinopoiskService kp) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<FilmDto>>> GetFilms([FromQuery] FilmQueryParams p, CancellationToken ct)
        => Ok(await filmService.GetFilmsAsync(p, ct));

    [HttpGet("featured")]
    public async Task<ActionResult<FilmDetailDto>> GetFeatured(CancellationToken ct)
    {
        var film = await filmService.GetFeaturedAsync(ct);
        return film is null ? NotFound() : Ok(film);
    }

    [HttpGet("sliders")]
    public async Task<ActionResult<SlidersResponse>> GetSliders(CancellationToken ct)
        => Ok(await filmService.GetSlidersAsync(ct));

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedResponse<FilmDto>>> Search(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest(new { error = "Query parameter 'q' is required." });
        return Ok(await filmService.SearchFilmsAsync(q, page, limit, ct));
    }

    /// <summary>
    /// Возвращает фильмы из коллекции Kinopoisk.
    /// Доступные type: TOP_250_MOVIES, TOP_250_TV_SHOWS, KIDS_ANIMATION_THEME,
    /// TOP_100_POPULAR_FILMS, TOP_AWAIT_FILMS, VAMPIRE_THEME, COMICS_THEME,
    /// CATASTROPHE_THEME, ZOMBIE_THEME, FAMILY_THEME, TOP_250_BEST_FILMS.
    /// </summary>
    [HttpGet("collections")]
    public async Task<ActionResult<PaginatedResponse<FilmDto>>> GetCollection(
        [FromQuery] string type = "TOP_250_MOVIES",
        [FromQuery] int page = 1,
        CancellationToken ct = default)
        => Ok(await filmService.GetCollectionAsync(type, page, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FilmDetailDto>> GetById(int id, CancellationToken ct)
    {
        var film = await filmService.GetFilmByIdAsync(id, ct);
        return film is null ? NotFound() : Ok(film);
    }

    [HttpGet("{id:int}/similar")]
    public async Task<ActionResult<List<FilmDto>>> GetSimilar(int id, [FromQuery] int limit = 10, CancellationToken ct = default)
        => Ok(await filmService.GetSimilarFilmsAsync(id, limit, ct));

    [HttpGet("{id:int}/seasons")]
    public async Task<ActionResult> GetSeasons(int id, CancellationToken ct)
    {
        var data = await kp.GetSeasonsAsync(id, ct);
        if (data is null) return NotFound();

        return Ok(new
        {
            total = data.Total,
            items = data.Items.Select(s => new
            {
                number = s.Number,
                episodesCount = s.Episodes.Count
            })
        });
    }

    [HttpGet("{id:int}/seasons/{season:int}/episodes")]
    public async Task<ActionResult> GetEpisodes(int id, int season, CancellationToken ct)
    {
        var data = await kp.GetSeasonsAsync(id, ct);
        if (data is null) return NotFound();

        var s = data.Items.FirstOrDefault(s => s.Number == season);
        if (s is null) return NotFound(new { error = $"Season {season} not found." });

        return Ok(new
        {
            season = s.Number,
            episodesCount = s.Episodes.Count,
            episodes = s.Episodes
                .OrderBy(e => e.EpisodeNumber)
                .Select(e => new
                {
                    number = e.EpisodeNumber,
                    title = e.NameRu ?? e.NameEn,
                    titleOriginal = e.NameEn,
                    synopsis = e.Synopsis,
                    releaseDate = e.ReleaseDate
                })
        });
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<PaginatedResponse<CommentDto>>> GetComments(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        var query = db.Comments
            .Where(c => c.FilmId == id)
            .OrderByDescending(c => c.CreatedAt)
            .Include(c => c.User);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync(ct);

        return Ok(new PaginatedResponse<CommentDto>
        {
            Data = items.Select(c => new CommentDto
            {
                Id = c.Id,
                Text = c.Text,
                CreatedAt = c.CreatedAt,
                LikesCount = c.LikesCount,
                User = new CommentUserDto { Id = c.User.Id, Name = c.User.Name, AvatarUrl = c.User.AvatarUrl }
            }).ToList(),
            Meta = new PaginationMeta { Page = page, Limit = limit, Total = total, TotalPages = (int)Math.Ceiling((double)total / limit) }
        });
    }

    [Authorize]
    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(int id, [FromBody] CreateCommentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { error = "Comment text cannot be empty." });

        var userId = Guid.Parse(User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!);
        var user = await db.Users.FindAsync([userId], ct);
        if (user is null) return Unauthorized();

        var comment = new Comment { FilmId = id, UserId = userId, Text = request.Text.Trim() };
        db.Comments.Add(comment);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetComments), new { id }, new CommentDto
        {
            Id = comment.Id,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            LikesCount = 0,
            User = new CommentUserDto { Id = user.Id, Name = user.Name, AvatarUrl = user.AvatarUrl }
        });
    }

    [Authorize]
    [HttpDelete("{id:int}/comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(int id, Guid commentId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!);

        var comment = await db.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.FilmId == id, ct);
        if (comment is null) return NotFound();
        if (comment.UserId != userId) return Forbid();

        db.Comments.Remove(comment);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
