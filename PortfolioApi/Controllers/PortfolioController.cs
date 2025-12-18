using GitHubService;
using Microsoft.AspNetCore.Mvc;
using GitHubService.Models; // כדי להשתמש ב-RepositoryData

namespace PortfolioApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // ה-Route יהיה: api/portfolio
    public class PortfolioController : ControllerBase
    {
        private readonly IGitHubService _githubService;

        // הזרקת ה-IGitHubService (ה-Decorator המכיל את ה-Caching)
        public PortfolioController(IGitHubService githubService)
        {
            _githubService = githubService;
        }

        // --- 1. Endpoint: שליפת הפורטפוליו האישי (עם Caching) ---
        // Route: GET api/portfolio/my
        [HttpGet("my")]
        public async Task<ActionResult<List<RepositoryData>>> GetPortfolio()
        {
            var portfolio = await _githubService.GetPortfolio();
            return Ok(portfolio);
        }

        // --- 2. Endpoint: חיפוש כללי ב-GitHub ---
        // Route: GET api/portfolio/search?repoName=...&language=...&username=...
        [HttpGet("search")]
        public async Task<IActionResult> SearchRepositories(
            [FromQuery] string? repoName,
            [FromQuery] string? language,
            [FromQuery] string? username)
        {
            var results = await _githubService.SearchRepositories(repoName, language, username);

            // ניתן לבצע Map למודל נקי יותר, אך נחזיר את אובייקטי Octokit כפי שהם לשם הפשטות
            return Ok(results);
        }
    }
}