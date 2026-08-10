using Microsoft.JSInterop;

namespace Quizora.Web.Services;

public class ThemeService
{
    private readonly IJSRuntime _js;
    public bool IsDark { get; private set; } = true;
    public event Action? OnChange;

    public ThemeService(IJSRuntime js) => _js = js;

    public async Task InitializeAsync()
    {
        try
        {
            var saved = await _js.InvokeAsync<string?>("localStorage.getItem", "quizora-theme");
            IsDark = saved != "light";
            await ApplyAsync();
        }
        catch
        {
            IsDark = true;
        }
    }

    public async Task ToggleAsync()
    {
        IsDark = !IsDark;
        await ApplyAsync();
        OnChange?.Invoke();
    }

    private async Task ApplyAsync()
    {
        var theme = IsDark ? "dark" : "light";
        try
        {
            await _js.InvokeVoidAsync("eval",
                $"document.documentElement.setAttribute('data-theme','{theme}');" +
                $"localStorage.setItem('quizora-theme','{theme}');");
        }
        catch { /* prerender */ }
    }
}
