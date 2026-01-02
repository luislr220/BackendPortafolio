using System.Security.Cryptography;

namespace BackendPortafolio.Helpers;

public static class SeguridadesHelper
{
    public static string GenerarCodigo2Fa()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
    }
}