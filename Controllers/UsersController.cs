using System.Security.Claims;
using FilmotekaAPI.DTOs.Common;
using FilmotekaAPI.DTOs.Films;
using FilmotekaAPI.DTOs.Users;
using FilmotekaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FilmotekaAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IUserService userService) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id, CancellationToken ct)
    {
        var user = await userService.GetByIdAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        if (id != CurrentUserId) return Forbid();
        return Ok(await userService.UpdateAsync(id, request, ct));
    }

    [HttpPut("{id:guid}/avatar")]
    public async Task<ActionResult> UpdateAvatar(Guid id, IFormFile file, CancellationToken ct)
    {
        if (id != CurrentUserId) return Forbid();
        if (file is null || file.Length == 0) return BadRequest(new { error = "No file provided." });
        if (file.Length > 5 * 1024 * 1024) return BadRequest(new { error = "File size exceeds 5MB." });

        var url = await userService.UpdateAvatarAsync(id, file, ct);
        return Ok(new { avatarUrl = url });
    }

    // --- Watchlist ---

    [HttpGet("{id:guid}/watchlist")]
    public async Task<ActionResult<PaginatedResponse<FilmDto>>> GetWatchlist(
        Guid id,
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
        => Ok(await userService.GetWatchlistAsync(id, type, page, limit, ct));

    [HttpPost("watchlist/{filmId:int}")]
    public async Task<IActionResult> AddToWatchlist(int filmId, CancellationToken ct)
    {
        await userService.AddToWatchlistAsync(CurrentUserId, filmId, ct);
        return NoContent();
    }

    [HttpDelete("watchlist/{filmId:int}")]
    public async Task<IActionResult> RemoveFromWatchlist(int filmId, CancellationToken ct)
    {
        await userService.RemoveFromWatchlistAsync(CurrentUserId, filmId, ct);
        return NoContent();
    }

    // --- Favorites ---

    [HttpGet("{id:guid}/favorites")]
    public async Task<ActionResult<PaginatedResponse<FilmDto>>> GetFavorites(
        Guid id,
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
        => Ok(await userService.GetFavoritesAsync(id, type, page, limit, ct));

    [HttpPost("favorites/{filmId:int}")]
    public async Task<IActionResult> AddToFavorites(int filmId, CancellationToken ct)
    {
        await userService.AddToFavoritesAsync(CurrentUserId, filmId, ct);
        return NoContent();
    }

    [HttpDelete("favorites/{filmId:int}")]
    public async Task<IActionResult> RemoveFromFavorites(int filmId, CancellationToken ct)
    {
        await userService.RemoveFromFavoritesAsync(CurrentUserId, filmId, ct);
        return NoContent();
    }

    // --- Watch History ---

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<PaginatedResponse<FilmDto>>> GetHistory(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
        => Ok(await userService.GetHistoryAsync(id, page, limit, ct));

    [HttpPost("history/{filmId:int}")]
    public async Task<IActionResult> AddToHistory(int filmId, CancellationToken ct)
    {
        await userService.AddToHistoryAsync(CurrentUserId, filmId, ct);
        return NoContent();
    }

    [HttpDelete("history/{filmId:int}")]
    public async Task<IActionResult> RemoveFromHistory(int filmId, CancellationToken ct)
    {
        await userService.RemoveFromHistoryAsync(CurrentUserId, filmId, ct);
        return NoContent();
    }

    [HttpDelete("history")]
    public async Task<IActionResult> ClearHistory(CancellationToken ct)
    {
        await userService.ClearHistoryAsync(CurrentUserId, ct);
        return NoContent();
    }
}
