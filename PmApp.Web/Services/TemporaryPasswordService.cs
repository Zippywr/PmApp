using System.Security.Cryptography;

namespace PmApp.Web.Services;

public static class TemporaryPasswordService
{
    // Password acak per pengguna; ditampilkan satu kali pada hasil tindakan Admin.
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789@#%!?";

    public static string Generate(int length = 16)
    {
        var result = new char[length];
        for (var i = 0; i < length; i++)
            result[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        return new string(result);
    }
}
