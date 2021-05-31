using Kit.Forms.Security.RSA;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Forms.Services.Interfaces
{
    public interface IRSA
    {
        public bool TestKey(Key key, string TestString="This is a test");
        public Key MakeKey(string name);
        public byte[] Encrypt(Key key, byte[] message);
        public byte[] Decrypt(Key key, byte[] message);
        public string EncryptToString(Key key, string message);
        public string DecryptToString(Key key, string message);
    }
}
