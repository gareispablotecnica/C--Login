using System;
using System.Security.Cryptography;
using System.Text;

namespace CapaLogica
{
    public static class PasswordHasher
    {
        public const string PREFIJO_HASH = "$pbkdf2-sha512$";
        public const int ITERACIONES = 210000;

        private const int TAMANIO_SAL = 16;
        private const int TAMANIO_HASH = 64;
        private const int TOTAL_PARTES = 5;

        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La contraseña es obligatoria.", "password");

            byte[] sal = new byte[TAMANIO_SAL];

            using (RandomNumberGenerator generador = RandomNumberGenerator.Create())
            {
                generador.GetBytes(sal);
            }

            byte[] hash = Derivar(password, sal, ITERACIONES);

            StringBuilder resultado = new StringBuilder();
            resultado.Append(PREFIJO_HASH);
            resultado.Append(ITERACIONES);
            resultado.Append('$');
            resultado.Append(Convert.ToBase64String(sal));
            resultado.Append('$');
            resultado.Append(Convert.ToBase64String(hash));

            return resultado.ToString();
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            if (!EsHash(storedHash))
                return ComparacionSegura(password, storedHash);

            int iteraciones;
            byte[] sal;
            byte[] hashEsperado;

            if (!IntentarDescomponerHash(storedHash, out iteraciones, out sal, out hashEsperado))
                return false;

            byte[] hashCalculado = Derivar(password, sal, iteraciones);

            return ComparacionSegura(hashEsperado, hashCalculado);
        }

        public static bool EsHash(string valor)
        {
            int iteraciones;
            byte[] sal;
            byte[] hash;

            if (string.IsNullOrEmpty(valor) || !valor.StartsWith(PREFIJO_HASH, StringComparison.Ordinal))
                return false;

            return IntentarDescomponerHash(valor, out iteraciones, out sal, out hash);
        }

        private static bool IntentarDescomponerHash(string valor, out int iteraciones, out byte[] sal, out byte[] hash)
        {
            iteraciones = 0;
            sal = null;
            hash = null;

            string[] partes = valor.Split('$');

            if (partes.Length != TOTAL_PARTES)
                return false;

            if (!Int32.TryParse(partes[2], out iteraciones) || iteraciones <= 0)
                return false;

            try
            {
                sal = Convert.FromBase64String(partes[3]);
                hash = Convert.FromBase64String(partes[4]);
            }
            catch (FormatException)
            {
                return false;
            }

            return sal.Length > 0 && hash.Length > 0;
        }

        private static byte[] Derivar(string password, byte[] sal, int iteraciones)
        {
            using (Rfc2898DeriveBytes derivada = new Rfc2898DeriveBytes(password, sal, iteraciones, HashAlgorithmName.SHA512))
            {
                return derivada.GetBytes(TAMANIO_HASH);
            }
        }

        private static bool ComparacionSegura(string valorA, string valorB)
        {
            return ComparacionSegura(
                Encoding.UTF8.GetBytes(valorA ?? string.Empty),
                Encoding.UTF8.GetBytes(valorB ?? string.Empty));
        }

        private static bool ComparacionSegura(byte[] valorA, byte[] valorB)
        {
            if (valorA == null || valorB == null)
                return false;

            int diferencia = valorA.Length ^ valorB.Length;
            int limite = Math.Max(valorA.Length, valorB.Length);

            for (int indice = 0; indice < limite; indice++)
            {
                byte byteA = indice < valorA.Length ? valorA[indice] : (byte)0;
                byte byteB = indice < valorB.Length ? valorB[indice] : (byte)0;
                diferencia |= byteA ^ byteB;
            }

            return diferencia == 0;
        }
    }
}