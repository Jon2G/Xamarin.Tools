using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Security.Encryption
{
    public abstract class Encryption
    {
        protected readonly Encoding Encoding;
        public Encryption(Encoding Encoding)
        {
            this.Encoding = Encoding;
        }
        public abstract byte[] Encrypt(string Value);
        public abstract byte[] UnEncrypt(byte[] Array);
        public string ToString(byte[] Array)
        {
            if(Array is null)
            {
                return string.Empty;
            }
            return Encoding.GetString(Array);
        }

    }
}
