namespace Auth.Identity.Security
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;

    public static class Password
    {
        private static readonly RNGCryptoServiceProvider RngCryptoServiceProvider = new();
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string Punctuations = "!@#$%&*-+=?";

        private static int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (int) Math.Floor((minValue + ((double) maxValue - minValue) * NextDouble()));
        }

        private static double NextDouble()
        {
            var data = new byte[sizeof(uint)];
            RngCryptoServiceProvider.GetBytes(data);
            var randUint = BitConverter.ToUInt32(data, 0);
            return randUint / (uint.MaxValue + 1.0);
        }

        public static string Generate(int length)
        {
            string[] randomChars =
            {
                Uppercase,
                Lowercase,
                Digits,
                Punctuations,
            };
            if (length < randomChars.Length)
            {
                throw new ArgumentException(string.Empty, nameof(length));
            }


            var chars = new List<char>();
            foreach (var randomCharGroup in randomChars)
            {
                chars.Insert(Next(0, chars.Count), randomCharGroup[Next(0, randomCharGroup.Length)]);
            }

            for (var i = chars.Count; i < length; ++i)
            {
                var rcs = randomChars[Next(0, randomChars.Length)];
                chars.Insert(Next(0, chars.Count), rcs[Next(0, rcs.Length)]);
            }

            chars.Sort((_, _) => Next(-1, 1));
            return new string(chars.ToArray());
        }

        public static string GenerateLoginHash()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < 64; i++)
            {
                sb.Append(Next(0, 9));
            }

            return sb.ToString();
        }
    }
}