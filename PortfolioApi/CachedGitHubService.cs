using GitHubService;
using GitHubService.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Octokit;
using System.Net.Http.Headers;

namespace PortfolioApi.Decorators
{
    // הערה: יש לוודא שהפרויקט PortfolioApi מפנה לפרויקט GitHubService
    public class CachedGitHubService : IGitHubService
    {
        private readonly IGitHubService _decorated;
        private readonly IMemoryCache _cache;
        private readonly string _username;

        // מפתחות ל-Cache
        private const string CacheKeyPrefix = "Portfolio_";
        private const string LastFetchTimestampKey = "Portfolio_LastFetch";

        // הזרקת המופע המקורי של IGitHubService, IMemoryCache ו-GitHubOptions
        public CachedGitHubService(
            IGitHubService decorated,
            IMemoryCache cache,
            IOptions<GitHubService.Configuration.GitHubOptions> options)
        {
            _decorated = decorated;
            _cache = cache;
            _username = options.Value.Username;
        }

        // --- יישום GetPortfolio עם Cache חכם ---
        public async Task<List<RepositoryData>> GetPortfolio()
        {
            // 1. נסה לשלוף מה-Cache
            if (_cache.TryGetValue(CacheKeyPrefix, out List<RepositoryData>? portfolio))
            {
                // 2. שלוף את חותמת הזמן של השליפה האחרונה
                if (_cache.TryGetValue(LastFetchTimestampKey, out DateTimeOffset lastFetch))
                {
                    // 3. בדוק אם יש פעילות חדשה ב-GitHub מאז השליפה האחרונה
                    if (await HasUserActivitySince(lastFetch))
                    {
                        // פעילות חדשה נמצאה - נקה Cache ושלף מחדש
                        _cache.Remove(CacheKeyPrefix);
                    }
                    else
                    {
                        // ה-Cache טרי - החזר מיד
                        return portfolio!;
                    }
                }
                // אם חסרה חותמת זמן, נמשיך לשליפה מלאה
            }

            // 4. שליפה מלאה מה-Service האמיתי
            var freshData = await _decorated.GetPortfolio();
            var now = DateTimeOffset.UtcNow;

            // 5. שמירה ב-Cache ושמירת חותמת הזמן
            _cache.Set(CacheKeyPrefix, freshData, TimeSpan.FromHours(4)); // זמן ברירת מחדל ל-Cache
            _cache.Set(LastFetchTimestampKey, now);

            return freshData;
        }

        // הפונקציה Search אינה משתמשת ב-Cache (תמיד שולפת עדכנית)
        public Task<IReadOnlyList<Repository>> SearchRepositories(string repoName, string language, string username)
        {
            return _decorated.SearchRepositories(repoName, language, username);
        }

        // --- לוגיקה לבדיקת פעילות (משתמשת ב-Octokit) ---
        private async Task<bool> HasUserActivitySince(DateTimeOffset since)
        {
            var client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("ActivityChecker"));

            // קוראים ל-API רק עם שם המשתמש. Octokit מטפל בהגבלה אוטומטית.
            var events = await client.Activity.Events.GetAll();

            var latestPush = events
     // 1. סנן לפי המשתמש שלנו
     .Where(e => e.Actor.Login == _username)
     // 2. סנן לפי סוג האירוע (כמו קודם)
     .Where(e => e.Type == "PushEvent" || e.Type == "CommitCommentEvent")
     .OrderByDescending(e => e.CreatedAt)
     .FirstOrDefault();
            return latestPush != null && latestPush.CreatedAt > since;
            //// הערה: Octokit אינו חלק מ-GitHubService, נשתמש ב-Client אנונימי לצורך בדיקת ה-Activity
            //// זוהי דרך פשוטה לבצע את הבדיקה הנדרשת.
            //var client = new GitHubClient(new Octokit.ProductHeaderValue("ActivityChecker"));
            //// שליפת אירועי הפעילות הציבוריים של המשתמש (PushEvents הם הנפוצים)
            //// המתודה הנכונה לשליפת אירועים ציבוריים של משתמש ספציפי
            //// המתודה הנכונה לשליפת אירועי פעילות ציבוריים של משתמש
            //// חלופה הכוללת הגבלת מספר האירועים (לביצועים)
            //var options = new Octokit.ApiOptions { PageSize = 30, PageCount = 1 };
            //var events = await client.Activity.Events.GetAll(_username);

            //// ... שאר הלוגיקה נשארת כפי שהייתה
            //var latestPush = events
            //    .Where(e => e.Type == "PushEvent" || e.Type == "CommitCommentEvent")
            //    .OrderByDescending(e => e.CreatedAt)
            //    .FirstOrDefault();

            //return latestPush != null && latestPush.CreatedAt > since;
        }
    }
}