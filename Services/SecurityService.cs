using Microsoft.Maui.Storage;
using System.Security.Cryptography;
using System.Text;

namespace JournalApp.Services
{
    public class SecurityService
    {
        private const string PinKey = "journal_pin_hash";
        private bool _isAuthenticated = false;

        public bool IsAuthenticated => _isAuthenticated;

        public async Task<bool> IsPinSetAsync()
        {
            var pinHash = await SecureStorage.GetAsync(PinKey);
            return !string.IsNullOrEmpty(pinHash);
        }

        public async Task SetPinAsync(string pin)
        {
            if (string.IsNullOrEmpty(pin))
            {
                SecureStorage.Remove(PinKey);
                _isAuthenticated = false;
                return;
            }

            var hash = ComputeHash(pin);
            await SecureStorage.SetAsync(PinKey, hash);
            _isAuthenticated = true; // Automatically authenticate on set
        }

        public async Task<bool> VerifyPinAsync(string pin)
        {
            var storedHash = await SecureStorage.GetAsync(PinKey);
            if (string.IsNullOrEmpty(storedHash)) return true; // No PIN set

            var inputHash = ComputeHash(pin);
            if (storedHash == inputHash)
            {
                _isAuthenticated = true;
                return true;
            }

            return false;
        }

        public void Lock()
        {
            _isAuthenticated = false;
        }

        private string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}
