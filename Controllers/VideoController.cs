using FilmotekaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FilmotekaAPI.Controllers;

[ApiController]
[Route("api/video")]
[EnableRateLimiting("video")]
public class VideoController(IVideoExtractionService videoService) : ControllerBase
{
    [HttpGet("extract")]
    public async Task<IActionResult> Extract([FromQuery] string iframeUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(iframeUrl))
            return BadRequest(new { error = "iframeUrl is required." });

        if (!Uri.TryCreate(iframeUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return BadRequest(new { error = "iframeUrl must be a valid http/https URL." });

        var videoUrl = await videoService.ExtractAsync(iframeUrl, ct);
        if (videoUrl is null)
            return NotFound(new { error = "Could not extract a video URL from the provided iframe." });

        return Ok(new { videoUrl });
    }
}
