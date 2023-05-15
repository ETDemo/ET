using System;
using System.Security.Cryptography;
using System.Text;

namespace ET.GameDemo.Server
{
    public class RsaPrivate: Singleton<RsaPrivate>
    {
        private const int defaultConfigID = 1001;

        private RSA rsa;
        private RSAEncryptionPadding padding;
        private Encoding encoding;

        public RsaPrivate()
        {
            rsa = RSA.Create();
            padding = RSAEncryptionPadding.Pkcs1;
            encoding = Encoding.UTF8;
        }

        public override void Dispose()
        {
            rsa.Dispose();
            padding = null;
            encoding = null;
        }

        public byte[] Decrypt(string value, int configID = defaultConfigID)
        {
            if (value == null) return Array.Empty<byte>();
            return this.Decrypt(StringToBytes(value), configID);
        }

        public byte[] Decrypt(byte[] value, int configID = defaultConfigID)
        {
            if (value == null) return Array.Empty<byte>();
            var cfg = RsaPrivateConfigCategory.Instance.Get(configID);
            this.rsa.FromXmlString(cfg.Key);
            return this.rsa.Decrypt(value, padding);
        }

        public string BytesToString(byte[] value)
        {
            if (value == null) return string.Empty;

            return encoding.GetString(value);
        }

        public byte[] StringToBytes(string value)
        {
            if (value == null) return Array.Empty<byte>();

            return encoding.GetBytes(value);
        }
    }
}