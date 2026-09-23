using System;
using System.Security.Cryptography;
using System.Text;
using NacionalSeguros.Application.Abstractions.Security;

namespace NacionalSeguros.Infrastructure.Security;

public class MfaService : IMfaService
{
    private const int TimeStepSeconds = 30;

    public string GenerateSecretKey()
    {
        // 80 bits de entropía (10 bytes) es estándar y seguro para TOTP
        byte[] buffer = new byte[10];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(buffer);
        }
        return Base32Encode(buffer);
    }

    public bool ValidateCode(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        string cleanedCode = code.Trim();
        if (cleanedCode.Length != 6 || !int.TryParse(cleanedCode, out int otpCode))
        {
            return false;
        }

        byte[] secretBytes;
        try
        {
            secretBytes = Base32Decode(secret);
        }
        catch (Exception)
        {
            return false;
        }

        long currentTimeStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TimeStepSeconds;

        // Ventana de tolerancia de +/- 1 paso de tiempo (total 90 segundos de ventana de aceptación)
        for (long i = -1; i <= 1; i++)
        {
            if (CalculateTotp(secretBytes, currentTimeStep + i) == otpCode)
            {
                return true;
            }
        }

        return false;
    }

    public string GetQrCodeUri(string correo, string secret)
    {
        string issuer = Uri.EscapeDataString("NacionalSeguros.SIR");
        string label = Uri.EscapeDataString(correo);
        return $"otpauth://totp/{issuer}:{label}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";
    }

    private static int CalculateTotp(byte[] secret, long timeStep)
    {
        byte[] timeStepBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timeStepBytes);
        }

        using (var hmac = new HMACSHA1(secret))
        {
            byte[] hash = hmac.ComputeHash(timeStepBytes);

            int offset = hash[hash.Length - 1] & 0x0F;
            int binary = ((hash[offset] & 0x7F) << 24) |
                         ((hash[offset + 1] & 0xFF) << 16) |
                         ((hash[offset + 2] & 0xFF) << 8) |
                         (hash[offset + 3] & 0xFF);

            return binary % 1000000;
        }
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        if (data == null || data.Length == 0)
        {
            return string.Empty;
        }

        var result = new StringBuilder((data.Length + 7) * 8 / 5);
        int bitBuffer = 0;
        int bitCount = 0;

        foreach (byte b in data)
        {
            bitBuffer = (bitBuffer << 8) | b;
            bitCount += 8;

            while (bitCount >= 5)
            {
                int index = (bitBuffer >> (bitCount - 5)) & 0x1F;
                result.Append(alphabet[index]);
                bitCount -= 5;
            }
        }

        if (bitCount > 0)
        {
            int index = (bitBuffer << (5 - bitCount)) & 0x1F;
            result.Append(alphabet[index]);
        }

        return result.ToString();
    }

    private static byte[] Base32Decode(string base32)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        string cleaned = base32.ToUpperInvariant().TrimEnd('=');
        if (cleaned.Length == 0)
        {
            return Array.Empty<byte>();
        }

        int byteCount = cleaned.Length * 5 / 8;
        byte[] result = new byte[byteCount];
        int buffer = 0;
        int bitsLeft = 0;
        int resultIndex = 0;

        foreach (char c in cleaned)
        {
            int value = alphabet.IndexOf(c);
            if (value < 0)
            {
                throw new ArgumentException("Carácter base32 inválido.", nameof(base32));
            }

            buffer = (buffer << 5) | value;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                result[resultIndex++] = (byte)(buffer >> (bitsLeft - 8));
                bitsLeft -= 8;
            }
        }

        return result;
    }
}
