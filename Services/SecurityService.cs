using Microsoft.Maui.Storage;
using System.Security.Cryptography;
using System.Text;

namespace JournalApp.Services;

// Service responsible for handling application security, including PIN protection.
public class SecurityService
{
    private const string PinKey = "journal_pin_hash";
    private bool _isAuthenticated;

    // Gets a value indicating whether the user is currently authenticated.
    public bool IsAuthenticated => _isAuthenticated;

    // Checks if a PIN has been set in secure storage.
    public async Task<bool> IsPinSetAsync()
    {
        var pinHash = await SecureStorage.GetAsync(PinKey);
        return !string.IsNullOrEmpty(pinHash);
    }

    // Sets or removes the application PIN.
    public async Task SetPinAsync(string pin)
    {
        // Remove PIN if empty string is provided
        if (string.IsNullOrEmpty(pin))
        {
            SecureStorage.Remove(PinKey);
            _isAuthenticated = false;
            return;
        }

        // Store hashed PIN for security
        var hash = ComputeHash(pin);
        await SecureStorage.SetAsync(PinKey, hash);
        _isAuthenticated = true; // Automatically authenticate when a new PIN is set
    }

    // Verifies the provided PIN against the stored hash.
    public async Task<bool> VerifyPinAsync(string pin)
    {
        var storedHash = await SecureStorage.GetAsync(PinKey);
        
        // If no PIN is set, consider it "verified" (app is open)
        if (string.IsNullOrEmpty(storedHash)) 
        {
            return true;
        }

        var inputHash = ComputeHash(pin);
        if (storedHash == inputHash)
        {
            _isAuthenticated = true;
            return true;
        }

        return false;
    }

    // Manually locks the application.
    public void Lock()
    {
        _isAuthenticated = false;
    }

    // Computes a SHA256 hash of the input string.
    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
