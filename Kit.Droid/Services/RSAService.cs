using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Security;
using Java.Security.Spec;
using Javax.Crypto;
using Kit.Forms.Services.Interfaces;
using RSAVault.Droid.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xamarin.Forms;
using Key = Kit.Forms.Security.RSA.Key;
using static Kit.Extensions.StringExtensions;
[assembly: Dependency(typeof(RSAService))]

namespace RSAVault.Droid.Services
{
    public class RSAService : IRSA
    {
        private const string Algorithm = "RSA";

        private byte[] Cipher(Key key, byte[] message, Javax.Crypto.CipherMode mode)
        {
            Cipher encryptCipher = Javax.Crypto.Cipher.GetInstance(Algorithm);

            encryptCipher.Init(mode, mode == Javax.Crypto.CipherMode.DecryptMode ? GetPrivateKey(key) : GetPublicKey(key)); ;
            byte[] encryptedMessageBytes = encryptCipher.DoFinal(message);
            return encryptedMessageBytes;
        }
        private IPublicKey GetPublicKey(Key key)
        {
            var factory = KeyFactory.GetInstance(Algorithm);
            IKeySpec keySpec = new X509EncodedKeySpec(Convert.FromBase64String(key.PublicKey));
            return factory.GeneratePublic(keySpec);
        }
        private IPrivateKey GetPrivateKey(Key key)
        {
            KeyFactory kf = KeyFactory.GetInstance(Algorithm);
            PKCS8EncodedKeySpec keySpec = new PKCS8EncodedKeySpec(Convert.FromBase64String(key.PrivateKey));
            return kf.GeneratePrivate(keySpec);
        }
        public Key MakeKey(string name)
        {
            KeyPairGenerator generator = KeyPairGenerator.GetInstance(Algorithm);
            generator.Initialize(2048);
            KeyPair pair = generator.GenerateKeyPair();
            IPrivateKey privateKey = pair.Private;
            IPublicKey publicKey = pair.Public;
            return
                new Key(
                name,
                Convert.ToBase64String(privateKey.GetEncoded()),
                Convert.ToBase64String(publicKey.GetEncoded())
                );

        }

        public byte[] Encrypt(Key key, byte[] message) =>
            Cipher(key, message, Javax.Crypto.CipherMode.EncryptMode);

        public byte[] Decrypt(Key key, byte[] message) =>
            Cipher(key, message, Javax.Crypto.CipherMode.DecryptMode);

        public string EncryptToString(Key key, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return string.Empty;
            }
            return System.Convert.ToBase64String(Cipher(key, System.Text.Encoding.UTF8.GetBytes(message), Javax.Crypto.CipherMode.EncryptMode));
        }


        public string DecryptToString(Key key, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return string.Empty;
            }
            return System.Text.Encoding.UTF8.GetString(Cipher(key, Convert.FromBase64String(message), Javax.Crypto.CipherMode.DecryptMode));
        }



        public bool TestKey(Key key, string TestString = "This is a test")
        {
            byte[] original = System.Text.Encoding.UTF8.GetBytes(TestString);
            byte[] crypted = Encrypt(key, original);



            string encrypted = EncryptToString(key, TestString);
            string s_crypted = System.Convert.ToBase64String(crypted);
            if (encrypted == s_crypted)
            {
                string decrypted = DecryptToString(key, encrypted);
                return TestString == decrypted;
            }
            return false;

        }
    }
}