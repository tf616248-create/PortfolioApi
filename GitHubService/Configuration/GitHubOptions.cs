using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubService.Configuration
{
    public class GitHubOptions // ודא שזה מוגדר כ-public
    {
        // קבוע המשמש כמפתח החלק ב-Configuration
        public const string GitHub = "GitHub";

        // המאפיינים שיקבלו את הנתונים מה-secrets.json
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
