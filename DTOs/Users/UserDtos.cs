using System.ComponentModel.DataAnnotations;

namespace FilmotekaAPI.DTOs.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int WatchlistCount { get; set; }
    public int FavoritesCount { get; set; }
}

public class UpdateUserRequest
{
    [MinLength(2)] public string? Name { get; set; }
    [EmailAddress] public string? Email { get; set; }
}
