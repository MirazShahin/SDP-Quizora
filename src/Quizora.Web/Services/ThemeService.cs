using Microsoft.JSInterop;

namespace Quizora.Web.Services;

public class ThemeService
{
    private readonly IJSRuntime _js;
    // Default = Light mode (user requirement)
    public bool IsDark { get; private set; } = false;
    public event Action? OnChange;

    public ThemeService(IJSRuntime js) => _js = js;

    public async Task InitializeAsync()
    {
        try
        {
            var saved = await _js.InvokeAsync<string?>("localStorage.getItem", "quizora-theme");
            // Only dark when explicitly saved as "dark". Everything else → light.
            IsDark = string.Equals(saved, "dark", StringComparison.OrdinalIgnoreCase);
            await ApplyAsync();
        }
        catch
        {
            // On first load / prerender → Light
            IsDark = false;
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
