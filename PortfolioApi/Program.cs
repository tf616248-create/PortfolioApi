//// חובה להוסיף:
//using GitHubService;
//using Scrutor;
//using PortfolioApi.Decorators;
//using GitHubService;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
///



//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//// רישום שירותי זיכרון עבור Caching
//builder.Services.AddMemoryCache();
//var app = builder.Build();
//// רישום IGitHubService, והוספת המעטפת (Decorator)
//builder.Services.AddScoped<IGitHubService, GitHubService.GitHubService>(); // רישום השירות האמיתי
//builder.Services.Decorate<IGitHubService, CachedGitHubService>();
//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();




using GitHubService;
using GitHubService.Configuration;
using PortfolioApi.Decorators;
using Scrutor;

var builder = WebApplication.CreateBuilder(args);

// --- אופציות ו-Configuration ---
// 1. רישום שירותי זיכרון עבור In-Memory Caching.
builder.Services.AddMemoryCache();

// 2. קריאת ה-Configuration (כולל secrets.json) והזרקתה למחלקת GitHubOptions.
builder.Services.Configure<GitHubOptions>(
    builder.Configuration.GetSection(GitHubOptions.GitHub));


// --- רישום ה-Service וה-Decorator (באמצעות Scrutor) ---
// 3. רישום השירות האמיתי (הבסיסי) שמתחבר ל-GitHub.
builder.Services.AddScoped<IGitHubService, GitHubService.GitHubService>();

// 4. רישום ה-Decorator (המעטפת) שמטפל ב-Caching.
// כאשר קוד יבקש IGitHubService, הוא יקבל את CachedGitHubService שמעטף את GitHubService.
builder.Services.Decorate<IGitHubService, CachedGitHubService>();


// --- שירותי API סטנדרטיים ---
builder.Services.AddControllers();
// לימוד: הוספת Swagger/OpenAPI כדי להקל על הבדיקות
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// --- בניית ה-App ---
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();