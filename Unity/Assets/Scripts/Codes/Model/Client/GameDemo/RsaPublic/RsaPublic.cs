using System;
using System.Security.Cryptography;
using System.Text;

namespace ET.GameDemo
{
    [ComponentOf(typeof (Scene))]
    public class RsaPublic: Singleton<RsaPublic>
    {
        public RSACryptoServiceProvider Provider;

        public byte[] Encrypt(string value)
        {
            if (value == null) return Array.Empty<byte>();
            return Provider.EncryptValue(Encoding.UTF8.GetBytes(value));
        }

        public byte[] Decrypt(string value)
        {
            if (value == null) return Array.Empty<byte>();
            return Provider.DecryptValue(Encoding.UTF8.GetBytes(value));
        }

        public byte[] Encrypt(byte[] value)
        {
            if (value == null) return Array.Empty<byte>();

            var all = RsaPublicConfigCategory.Instance.GetAll();
            var index = (int)(RandomGenerator.RandUInt32() % all.Count);
            var key = all[index].Key;
            Provider.FromXmlString(key);
            return Provider.EncryptValue(value);
        }

        public byte[] Decrypt(byte[] value)
        {
            if (value == null) return Array.Empty<byte>();

            var all = RsaPublicConfigCategory.Instance.GetAll();
            var index = (int)(RandomGenerator.RandUInt32() % all.Count);
            var key = all[index].Key;
            Provider.FromXmlString(key);
            return Provider.DecryptValue(value);
        }
    }
}