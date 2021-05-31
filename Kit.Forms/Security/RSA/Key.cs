using System;
using System.Globalization;
using System.Management;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Kit.Forms.Services.Interfaces;
using Kit.Model;
using Kit.Security.Encryption;
using Kit.Sql.Attributes;
using Kit.Sql.Interfaces;
using Xamarin.Forms;

namespace Kit.Forms.Security.RSA
{
    [Preserve]
    public sealed class Key : Encryption, IGuid
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique]
        public Guid Guid { get; set; }
        private string _Name;
        [Unique]
        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
            }
        }

        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }


        public Key() : base(Encoding.UTF8) { }
        public Key(string Name, string PrivateKey, string PublicKey) : base(Encoding.UTF8)
        {
            this.Name = Name;
            this.PrivateKey = PrivateKey;
            this.PublicKey = PublicKey;
        }

        public string EncryptToString(string Value)
        {
            IRSA rsa = DependencyService.Get<IRSA>(DependencyFetchTarget.GlobalInstance);
            return rsa.EncryptToString(this, Value);
        }
        public string DecryptToString(string Value)
        {
            IRSA rsa = DependencyService.Get<IRSA>(DependencyFetchTarget.GlobalInstance);
            return rsa.DecryptToString(this, Value);
        }
        public override byte[] Encrypt(string Value) => Encrypt(Encoding.UTF8.GetBytes(Value));
        public byte[] Encrypt(byte[] Value)
        {
            IRSA rsa = DependencyService.Get<IRSA>(DependencyFetchTarget.GlobalInstance);
            return rsa.Encrypt(this, Value);
        }

        public override byte[] Decrypt(byte[] Array)
        {
            IRSA rsa = DependencyService.Get<IRSA>(DependencyFetchTarget.GlobalInstance);
            return rsa.Decrypt(this,Array);
        }
    }
}
