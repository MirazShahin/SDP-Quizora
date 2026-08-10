using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Quizora.Application.Interfaces;
using Quizora.Web.Auth;
using Quizora.Web.Components;
using Quizora.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<AuthorizedHandler>();

builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthorizedHandler>();
    handler.InnerHandler = new HttpClientHandler();

    return new HttpClient(handler)
    {
        BaseAddress = new Uri("https://localhost:7102/") 
    };
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TestService>();
builder.Services.AddScoped<InvitationService>();
builder.Services.AddScoped<AttemptService>();
builder.Services.AddScoped<InterviewService>();
builder.Services.AddScoped<QuizApiClient>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<AiClient>(); 
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();