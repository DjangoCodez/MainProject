using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SoftOne.Soe.Common.Security
{
    public class StringEncryption
    {
        private readonly Random random;
        private readonly byte[] key;
        private readonly Aes aes;
        private readonly UTF8Encoding encoder;

        public StringEncryption(string key)
        {
            this.random = new Random();
            this.aes = Aes.Create();
            this.encoder = new UTF8Encoding();
            this.key = PadKey(key);
        }

        private byte[] PadKey(string key)
        {
            string paddedKey;
            if (key.Length < 16)
                paddedKey = key.PadRight(16, '0');
            else if (key.Length < 24)
                paddedKey = key.PadRight(24, '0');
            else if (key.Length < 32)
                paddedKey = key.PadRight(32, '0');
            else
                paddedKey = key.Substring(0, 32);

            return encoder.GetBytes(paddedKey);
        }

        public string Encrypt(string unencrypted)
        {
            var vector = new byte[16];
            this.random.NextBytes(vector);
            var cryptogram = vector.Concat(this.Encrypt(this.encoder.GetBytes(unencrypted), vector));
            return Convert.ToBase64String(cryptogram.ToArray());
        }

        public string Decrypt(string encrypted)
        {
            var cryptogram = Convert.FromBase64String(encrypted);
            if (cryptogram.Length < 17)
            {
                throw new ArgumentException("Not a valid encrypted string", "encrypted");
            }

            var vector = cryptogram.Take(16).ToArray();
            var buffer = cryptogram.Skip(16).ToArray();
            var decrypted = this.Decrypt(buffer, vector);
            return this.encoder.GetString(decrypted, 0, decrypted.Length);
        }

        private byte[] Encrypt(byte[] buffer, byte[] vector)
        {
            var encryptor = this.aes.CreateEncryptor(this.key, vector);
            return this.Transform(buffer, encryptor);
        }

        private byte[] Decrypt(byte[] buffer, byte[] vector)
        {
            var decryptor = this.aes.CreateDecryptor(this.key, vector);
            return this.Transform(buffer, decryptor);
        }

        private byte[] Transform(byte[] buffer, ICryptoTransform transform)
        {
            var stream = new MemoryStream();
            using (var cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                cs.Write(buffer, 0, buffer.Length);
            }

            return stream.ToArray();
        }
    }
}
