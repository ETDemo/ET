using System;
using System.Security.Cryptography;
using System.Text;

namespace ET.GameDemo
{
    [ComponentOf(typeof (Scene))]
    public class RsaPublic: Singleton<RsaPublic>
    {
        private const int defaultConfigID = 1001;

        private RSA rsa;
        private RSAEncryptionPadding padding;
        private Encoding encoding;

        public RsaPublic()
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

        public byte[] Encrypt(string value, int configID = defaultConfigID)
        {
            if (value == null) return Array.Empty<byte>();
            return this.Encrypt(StringToBytes(value), configID);
        }

        public byte[] Encrypt(byte[] value, int configID = defaultConfigID)
        {
            if (value == null) return Array.Empty<byte>();
            var cfg = RsaPublicConfigCategory.Instance.Get(configID);
            this.rsa.FromXmlString(cfg.Key);
            return this.rsa.Encrypt(value, padding);
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