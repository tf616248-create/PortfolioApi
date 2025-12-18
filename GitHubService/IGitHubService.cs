using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GitHubService.Models;
using Octokit; // שימוש באובייקט Repository של Octokit לחיפוש

namespace GitHubService
{
    public interface IGitHubService
    {
        // מחזירה את הנתונים המלאים עבור תיק העבודות האישי
        Task<List<RepositoryData>> GetPortfolio();

        // מקבלת פרמטרים אופציונליים ומחזירה רשימת Repositories ציבוריים
        Task<IReadOnlyList<Repository>> SearchRepositories(string repoName, string language, string username);
    }
}
