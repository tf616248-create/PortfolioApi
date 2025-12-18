using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubService.Models
{
    public class RepositoryData
    {
        // נתונים בסיסיים שנשלפים ישירות מאובייקט Repository של Octokit
        public string Name { get; set; }
        public string Description { get; set; }
        public int Stars { get; set; }
        public string HtmlUrl { get; set; } // הקישור ל-repo
        public string HomepageUrl { get; set; } // הקישור לאתר (אם יש)

        // נתונים הדורשים קריאה נפרדת
        public Dictionary<string, long> Languages { get; set; } // שפה וכמות בתים
        public DateTimeOffset? LastCommitDate { get; set; }
        public int PullRequestCount { get; set; }
    }
}
