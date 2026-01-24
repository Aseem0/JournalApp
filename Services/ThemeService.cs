using Microsoft.JSInterop;

namespace JournalApp.Services
{
    public class ThemeService
    {
        private readonly IJSRuntime _js;
        public bool IsDarkMode { get; private set; }
        public event Action? OnThemeChanged;

        public ThemeService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
            OnThemeChanged?.Invoke();
            try
            {
                await _js.InvokeVoidAsync("theme.set", IsDarkMode);
            }
            catch (Exception)
            {
                // Ignore JS errors during theme toggle
            }
        }
    }
}
