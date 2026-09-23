using System;
using System.Security.Cryptography;
using NacionalSeguros.Application.Abstractions.Security;

namespace NacionalSeguros.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32;  // 256 bits
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithm,
            KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));
        if (hashedPassword == null) throw new ArgumentNullException(nameof(hashedPassword));

        string[] parts = hashedPassword.Split('.', 3);

        if (parts.Length != 3)
        {
            if (hashedPassword.Length == 64)
            {
                using var sha256 = SHA256.Create();
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                string hex = Convert.ToHexString(bytes);
                return hex.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        if (!int.TryParse(parts[0], out int iterations))
        {
            return false;
        }

        try
        {
            byte[] saltBytes = Convert.FromBase64String(parts[1]);
            byte[] hashBytes = Convert.FromBase64String(parts[2]);

            byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                iterations,
                HashAlgorithm,
                hashBytes.Length);

            return CryptographicOperations.FixedTimeEquals(hashBytes, inputHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
