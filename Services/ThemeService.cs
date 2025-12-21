using Microsoft.JSInterop;

namespace JournalApp.Services
{
    public class ThemeService
    {
        private readonly IJSRuntime _js;

        public ThemeService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task ToggleTheme()
        {
            await _js.InvokeVoidAsync("theme.toggle");
        }
    }
}
