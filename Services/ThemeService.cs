using Microsoft.JSInterop;

namespace JournalApp.Services;

// Service responsible for managing application theme states (Dark/Light mode).
public class ThemeService
{
    private readonly IJSRuntime _js;

    // Gets a value indicating whether Dark Mode is currently active.
    public bool IsDarkMode { get; private set; }

    // Event triggered when the application theme changes.
    public event Action? ThemeChanged;

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    // Toggles the application theme between Dark and Light modes.
    public async Task ToggleThemeAsync()
    {
        // Toggle state and notify subscribers (Blazor components)
        IsDarkMode = !IsDarkMode;
        ThemeChanged?.Invoke();

        try
        {
            // Sync with JavaScript to update document body classes
            await _js.InvokeVoidAsync("theme.set", IsDarkMode);
        }
        catch (Exception)
        {
            // Ignore JS errors during theme toggle (e.g., if page is disposing)
        }
    }
}
