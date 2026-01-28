using Microsoft.JSInterop;

namespace JournalApp.Services;

/// <summary>
/// Service responsible for managing application theme states (Dark/Light mode).
/// </summary>
public class ThemeService
{
    private readonly IJSRuntime _js;

    /// <summary>
    /// Gets a value indicating whether Dark Mode is currently active.
    /// </summary>
    public bool IsDarkMode { get; private set; }

    /// <summary>
    /// Event triggered when the application theme changes.
    /// </summary>
    public event Action? ThemeChanged;

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>
    /// Toggles the application theme between Dark and Light modes.
    /// </summary>
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
