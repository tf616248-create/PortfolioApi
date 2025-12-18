// //// çåáä ìäåñéó:
// //using GitHubService;
// //using Scrutor;
// //using PortfolioApi.Decorators;
// //using GitHubService;

// //var builder = WebApplication.CreateBuilder(args);

// //// Add services to the container.
// ///



// //builder.Services.AddControllers();
// //// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// //builder.Services.AddEndpointsApiExplorer();
// //builder.Services.AddSwaggerGen();
// //// øéùåí ùéøåúé æéëøåï òáåø Caching
// //builder.Services.AddMemoryCache();
// //var app = builder.Build();
// //// øéùåí IGitHubService, åäåñôú äîòèôú (Decorator)
// //builder.Services.AddScoped<IGitHubService, GitHubService.GitHubService>(); // øéùåí äùéøåú äàîéúé
// //builder.Services.Decorate<IGitHubService, CachedGitHubService>();
// //// Configure the HTTP request pipeline.
// //if (app.Environment.IsDevelopment())
// //{
// //    app.UseSwagger();
// //    app.UseSwaggerUI();
// //}

// //app.UseHttpsRedirection();

// //app.UseAuthorization();

// //app.MapControllers();

// //app.Run();




// using GitHubService;
// using GitHubService.Configuration;
// using PortfolioApi.Decorators;
// using Scrutor;

// var builder = WebApplication.CreateBuilder(args);

// // --- àåôöéåú å-Configuration ---
// // 1. øéùåí ùéøåúé æéëøåï òáåø In-Memory Caching.
// builder.Services.AddMemoryCache();

// // 2. ÷øéàú ä-Configuration (ëåìì secrets.json) åäæø÷úä ìîçì÷ú GitHubOptions.
// builder.Services.Configure<GitHubOptions>(
//     builder.Configuration.GetSection(GitHubOptions.GitHub));


// // --- øéùåí ä-Service åä-Decorator (áàîöòåú Scrutor) ---
// // 3. øéùåí äùéøåú äàîéúé (äáñéñé) ùîúçáø ì-GitHub.
// builder.Services.AddScoped<IGitHubService, GitHubService.GitHubService>();

// // 4. øéùåí ä-Decorator (äîòèôú) ùîèôì á-Caching.
// // ëàùø ÷åã éá÷ù IGitHubService, äåà é÷áì àú CachedGitHubService ùîòèó àú GitHubService.
// builder.Services.Decorate<IGitHubService, CachedGitHubService>();


// // --- ùéøåúé API ñèðãøèééí ---
// builder.Services.AddControllers();
// // ìéîåã: äåñôú Swagger/OpenAPI ëãé ìä÷ì òì äáãé÷åú
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();


// // --- áðééú ä-App ---
// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();

// app.UseAuthorization();

// app.MapControllers();

// app.Run();



















using GitHubService;
using GitHubService.Configuration;
using PortfolioApi.Decorators;
using Scrutor;

var builder = WebApplication.CreateBuilder(args);

// --- אופציות ו-Configuration ---
// 1. רישום שירותי זיכרון עבור In-Memory Caching.
builder.Services.AddMemoryCache();

// 2. קריאת ה-Configuration ועדכון ידני ממשתני סביבה של GitHub
builder.Services.Configure<GitHubOptions>(options =>
{
    // טעינת ברירת המחדל מה-appsettings.json או secrets.json
    builder.Configuration.GetSection(GitHubOptions.GitHub).Bind(options);

    // בדיקה ודריסה של הערכים במידה וקיימים משתני סביבה (עבור GitHub Actions)
    var envUsername = builder.Configuration["GH_USERNAME"];
    var envToken = builder.Configuration["GH_TOKEN"];

    if (!string.IsNullOrEmpty(envUsername)) options.Username = envUsername;
    if (!string.IsNullOrEmpty(envToken)) options.Token = envToken;
});

// --- רישום ה-Service וה-Decorator (באמצעות Scrutor) ---
// 3. רישום השירות האמיתי (הבסיסי) שמתחבר ל-GitHub.
builder.Services.AddScoped<IGitHubService, GitHubService.GitHubService>();

// 4. רישום ה-Decorator (המעטפת) שמטפל ב-Caching.
builder.Services.Decorate<IGitHubService, CachedGitHubService>();

// --- שירותי API סטנדרטיים ---
builder.Services.AddControllers();
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
