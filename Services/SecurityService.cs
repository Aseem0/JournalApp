using Microsoft.Maui.Storage;
using System.Security.Cryptography;
using System.Text;

namespace JournalApp.Services;

/// <summary>
/// Service responsible for handling application security, including PIN protection.
/// </summary>
public class SecurityService
{
    private const string PinKey = "journal_pin_hash";
    private bool _isAuthenticated;

    /// <summary>
    /// Gets a value indicating whether the user is currently authenticated.
    /// </summary>
    public bool IsAuthenticated => _isAuthenticated;

    /// <summary>
    /// Checks if a PIN has been set in secure storage.
    /// </summary>
    public async Task<bool> IsPinSetAsync()
    {
        var pinHash = await SecureStorage.GetAsync(PinKey);
        return !string.IsNullOrEmpty(pinHash);
    }

    /// <summary>
    /// Sets or removes the application PIN.
    /// </summary>
    /// <param name="pin">The PIN to set. If empty, the PIN is removed.</param>
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

    /// <summary>
    /// Verifies the provided PIN against the stored hash.
    /// </summary>
    /// <param name="pin">The PIN to verify.</param>
    /// <returns>True if the PIN is correct or no PIN is set; otherwise, false.</returns>
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

    /// <summary>
    /// Manually locks the application.
    /// </summary>
    public void Lock()
    {
        _isAuthenticated = false;
    }

    /// <summary>
    /// Computes a SHA256 hash of the input string.
    /// </summary>
    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
