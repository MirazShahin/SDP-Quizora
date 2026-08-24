using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Quizora.Web.Auth;
using Quizora.Web.Components;
using Quizora.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Render PORT
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<AuthorizedHandler>();

// API URL
var apiBase = builder.Configuration["ApiBaseUrl"]
              ?? Environment.GetEnvironmentVariable("ApiBaseUrl")
              ?? "https://localhost:7102/";
if (!apiBase.EndsWith("/"))
    apiBase += "/";

// ✅ শুধু এটা — AuthorizedHandler সহ
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthorizedHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler)
    {
        BaseAddress = new Uri(apiBase)
    };
});

// ❌ এই ব্লক মুছে দাও — এটাই 401-এর কারণ
// builder.Services.AddScoped(sp => new HttpClient
// {
//     BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://quizora-api-o7gy.onrender.com/")
// });

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TestService>();
builder.Services.AddScoped<InvitationService>();
builder.Services.AddScoped<AttemptService>();
builder.Services.AddScoped<InterviewService>();
builder.Services.AddScoped<QuizApiClient>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<AiClient>();
builder.Services.AddScoped<CodeClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();
app.UseStaticFiles();

app.MapGet("/health", () => Results.Ok("OK"));

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();