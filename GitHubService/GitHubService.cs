//using GitHubService.Configuration;
//using GitHubService.Models;
//using Microsoft.Extensions.Options;
//using Octokit;
//using System.Net;

//namespace GitHubService
//{
//    public class GitHubService : IGitHubService
//    {
//        private readonly GitHubClient _githubClient;
//        private readonly string _username;

//        public GitHubService(IOptions<GitHubOptions> options)
//        {
//            // ודא שה-Token קיים לפני יצירת הלקוח
//            if (string.IsNullOrEmpty(options.Value.Token))
//            {
//                // ניתן להוסיף כאן לוגיקה לטיפול בשגיאה או להשתמש בלקוח אנונימי עבור פונקציות ציבוריות
//                throw new ArgumentNullException("GitHub Token is missing. Cannot access private portfolio data.");
//            }

//            _username = options.Value.Username;

//            // יצירת ה-Client עם פרטי ההזדהות (נדרש עבור גישה למידע אישי)
//            _githubClient = new GitHubClient(new ProductHeaderValue("PortfolioApp"))
//            {
//                Credentials = new Credentials(options.Value.Token)
//            };
//        }

//        // --- יישום GetPortfolio ---
//        public async Task<List<RepositoryData>> GetPortfolio()
//        {
//            // 1. שלב ראשוני: שליפת כל ה-Repositories של המשתמש
//            var repos = await _githubClient.Repository.GetAllForCurrent();
//            var portfolio = new List<RepositoryData>();

//            foreach (var repo in repos)
//            {
//                var repoData = new RepositoryData
//                {
//                    Name = repo.Name,
//                    Description = repo.Description,
//                    Stars = repo.StargazersCount,
//                    HtmlUrl = repo.HtmlUrl,
//                    HomepageUrl = repo.Homepage,
//                    Languages = new Dictionary<string, long>()
//                };

//                // 2. שליפת שפות:
//                var languages = await _githubClient.Repository.GetAllLanguages(repo.Owner.Login, repo.Name);
//                repoData.Languages = languages.ToDictionary(l => l.Name, l => l.NumberOfBytes);

//                // 3. שליפת קומיט אחרון:
//                // ניתן לקרוא רק את הקומיט הראשון (האחרון)
//                var commits = await _githubClient.Repository.Commit.GetAll(repo.Owner.Login, repo.Name, new ApiOptions { PageSize = 1, PageCount = 1 });
//                repoData.LastCommitDate = commits.FirstOrDefault()?.Commit.Author.Date;

//                // 4. שליפת Pull Requests:
//                var prs = await _githubClient.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name, new PullRequestRequest { State = ItemStateFilter.All });
//                repoData.PullRequestCount = prs.Count;

//                portfolio.Add(repoData);
//            }

//            return portfolio;
//        }
//        public async Task<IReadOnlyList<Repository>> SearchRepositories(string repoName, string language, string username)
//        {
//            // ... (קוד בניית queryParts נשאר זהה כמו בתיקון 1)
//            var queryParts = new List<string>();

//            if (!string.IsNullOrEmpty(repoName))
//            {
//                queryParts.Add($"{repoName} in:name");
//            }
//            if (!string.IsNullOrEmpty(language))
//            {
//                queryParts.Add($"language:{language}");
//            }
//            if (!string.IsNullOrEmpty(username))
//            {
//                queryParts.Add($"user:{username}");
//            }

//            string fullQuery = string.Empty;
//            if (queryParts.Any())
//            {
//                fullQuery = string.Join(" ", queryParts);
//            }

//            // יצירת הבקשה עם השאילתה המלאה באמצעות הקונסטרקטור
//            var request = new SearchRepositoriesRequest(fullQuery);

//            // אין צורך יותר ב- request.Q = ...

//            var result = await _githubClient.Search.SearchRepo(request);
//            return result.Items;
//        }
//        // --- יישום SearchRepositories ---
//        //public async Task<IReadOnlyList<Repository>> SearchRepositories(string repoName, string language, string username)
//        //{
//        //    var request = new SearchRepositoriesRequest();

//        //    // בניית הקריטריונים בהתאם לפרמטרים שהתקבלו
//        //    if (!string.IsNullOrEmpty(repoName))
//        //        request.Q = $"{repoName} in:name";

//        //    if (!string.IsNullOrEmpty(language))
//        //    {
//        //        // 1. הגדרת משתנה Enum
//        //        Language langEnum;

//        //        // 2. ניסיון המרה בטוח באמצעות TryParse:
//        //        // אם ההמרה הצליחה (כלומר, המחרוזת תואמת שם ב-Octokit.Language), השתמש בערך.
//        //        if (Enum.TryParse(language, true, out langEnum))
//        //        {
//        //            request.Language = langEnum;
//        //        }
//        //        // אם ההמרה נכשלה, אפשר להתעלם מהפרמטר (או להוסיף לוגיקה לטיפול בשגיאה).
//        //    }
//        //    //
//        //    if (!string.IsNullOrEmpty(username))
//        //        request.User = username;

//        //    // Octokit מאפשר חיפוש נקי לפי קריטריונים מרובים
//        //    var result = await _githubClient.Search.SearchRepo(request);

//        //    return result.Items;
//        //}
//    }
//}



using GitHubService.Configuration;
using GitHubService.Models;
using Microsoft.Extensions.Options;
using Octokit;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace GitHubService
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _githubClient;
        private readonly string _username;

        public GitHubService(IOptions<GitHubOptions> options)
        {
            // ודא שה-Token קיים לפני יצירת הלקוח
            if (string.IsNullOrEmpty(options.Value.Token))
            {
                throw new ArgumentNullException("GitHub Token is missing. Cannot access private portfolio data.");
            }

            _username = options.Value.Username;

            // יצירת ה-Client עם פרטי ההזדהות (נדרש עבור גישה למידע אישי)
            _githubClient = new GitHubClient(new ProductHeaderValue("PortfolioApp"))
            {
                Credentials = new Credentials(options.Value.Token)
            };
        }

        // --- יישום GetPortfolio: שליפת נתונים מפורטים עם טיפול בשגיאות ---
        public async Task<List<RepositoryData>> GetPortfolio()
        {
            // 1. שלב ראשוני: שליפת כל ה-Repositories של המשתמש המזדהה
            var repos = await _githubClient.Repository.GetAllForCurrent();
            var portfolio = new List<RepositoryData>();

            foreach (var repo in repos)
            {
                var repoData = new RepositoryData
                {
                    Name = repo.Name,
                    Description = repo.Description,
                    Stars = repo.StargazersCount,
                    HtmlUrl = repo.HtmlUrl,
                    HomepageUrl = repo.Homepage,
                    Languages = new Dictionary<string, long>()
                };

                // עוטפים ב-try-catch לטיפול בשגיאות כמו "Git Repository is empty" (קוד 409)
                try
                {
                    // 2. שליפת שפות
                    var languages = await _githubClient.Repository.GetAllLanguages(repo.Owner.Login, repo.Name);
                    repoData.Languages = languages.ToDictionary(l => l.Name, l => l.NumberOfBytes);

                    // 3. שליפת קומיט אחרון (זו הקריאה המועדת ביותר לשגיאת 409)
                    var commits = await _githubClient.Repository.Commit.GetAll(repo.Owner.Login, repo.Name, new ApiOptions { PageSize = 1, PageCount = 1 });
                    repoData.LastCommitDate = commits.FirstOrDefault()?.Commit.Author.Date;

                    // 4. שליפת Pull Requests
                    var prs = await _githubClient.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name, new PullRequestRequest { State = ItemStateFilter.All });
                    repoData.PullRequestCount = prs.Count;
                }
                catch (Octokit.ApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict || ex.Message.Contains("Git Repository is empty"))
                {
                    // אם המאגר ריק (שגיאה 409), מדלגים על ה-Repository וממשיכים
                    // המידע הסטטי (שם, כוכבים, קישור) כבר נשמר. שדות דינמיים יישארו ברירת מחדל.
                }
                catch (Exception)
                {
                    // טיפול בשגיאות אחרות (אם נדרש)
                }

                portfolio.Add(repoData);
            }

            return portfolio;
        }

        // --- יישום SearchRepositories: חיפוש כללי (ללא Caching) ---
        public async Task<IReadOnlyList<Repository>> SearchRepositories(string repoName, string language, string username)
        {
            // משתמשים בתיקון 1: בניית מחרוזת שאילתה כוללת
            var queryParts = new List<string>();

            if (!string.IsNullOrEmpty(repoName))
            {
                // חיפוש לפי שם ה-Repository
                queryParts.Add($"{repoName} in:name");
            }
            if (!string.IsNullOrEmpty(language))
            {
                // חיפוש לפי שפה ספציפית
                queryParts.Add($"language:{language}");
            }
            if (!string.IsNullOrEmpty(username))
            {
                // חיפוש לפי משתמש ספציפי
                queryParts.Add($"user:{username}");
            }

            string fullQuery = string.Empty;
            if (queryParts.Any())
            {
                fullQuery = string.Join(" ", queryParts);
            }

            // יצירת הבקשה עם השאילתה המלאה באמצעות הקונסטרקטור
            var request = new SearchRepositoriesRequest(fullQuery);

            // Octokit מבצע חיפוש נקי
            var result = await _githubClient.Search.SearchRepo(request);

            return result.Items;
        }
    }
}