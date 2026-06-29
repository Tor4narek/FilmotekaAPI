using System.ComponentModel.DataAnnotations;
using FilmotekaAPI.Data;
using FilmotekaAPI.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FilmotekaAPI.Controllers;

[ApiController]
[Route("api/support")]
public class SupportController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SupportTicketRequest request, CancellationToken ct)
    {
        var ticket = new SupportRequest
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLower(),
            Message = request.Message.Trim()
        };

        db.SupportRequests.Add(ticket);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public class SupportTicketRequest
{
    [Required, MinLength(2)] public string Name { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(10)] public string Message { get; set; } = string.Empty;
}
