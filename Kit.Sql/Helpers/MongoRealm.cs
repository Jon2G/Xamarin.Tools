using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit.Sql.Interfaces;
using Realms;
using Realms.Sync;

namespace Kit.Sql.Helpers
{
    public class MongoRealm
    {
        private readonly RealmConfiguration Config;
        public Realm Instance => Realm.GetInstance(Config);
        public Task<Realm> AsyncInstance => Realm.GetInstanceAsync(Config);
        public MongoRealm(string DbName, ulong SchemaVersion,
            bool ShouldDeleteIfMigrationNeeded = false)
        {
            if (Sqlh.Instance is null)
            {
                throw new Exception("Please call SQLHelper.Init before using it");
            }
            DirectoryInfo db = new DirectoryInfo($"{Sqlh.Instance.LibraryPath}/Realms/{DbName}/{DbName}.realm");
            if (!db.Parent.Exists)
            {
                db.Parent.Create();
                db.Parent.Attributes |= FileAttributes.Directory | FileAttributes.Normal;
                db.Parent.Attributes &= ~FileAttributes.ReadOnly;
            }
            this.Config = new RealmConfiguration(db.FullName)
            {
                SchemaVersion = SchemaVersion,
                IsReadOnly = false,
                ShouldDeleteIfMigrationNeeded = ShouldDeleteIfMigrationNeeded
            };

            RealmConfiguration.DefaultConfiguration = Config;
            Realm.GetInstance(Config);
        }

        public FileInfo GetDbFile()
        {
            return new FileInfo(this.Config.DatabasePath);
        }
        public interface IPrimaryKeyId
        {
            [PrimaryKey]
            public long Id { get; set; }
        }


        private IPrimaryKeyId GetNextId<T>(T item) where T : RealmObject, IPrimaryKeyId
        {
            T NewItemModel()
            {
                IQueryable<T> q = Instance.All<T>();
                item.Id = (q.Any() ? q.Last().Id + 1 : 1);
                return item;
            }

            if (Instance.IsInTransaction)
            {
                return NewItemModel();
            }
            // Use a pseudo transaction to ensure multi-process safety on obtaining the last record 
            using (Transaction trans = Instance.BeginWrite())
            {
                return NewItemModel();
            }
        }

        public RealmObject Update(RealmObject item) 
        {
            RealmObject result = null;
            using (Transaction trans = Instance.BeginWrite())
            {
                result = this.Instance.Add(item, true);
                trans.Commit();
            }
            return result;
        }
        public T Add<T>(T item) where T : RealmObject, IPrimaryKeyId
        {
            item = (T)GetNextId(item);
            return (T)Add((RealmObject)item);
        }
        public RealmObject Add(RealmObject item)
        {
            RealmObject result = null;
            using (Transaction trans = Instance.BeginWrite())
            {
                result = this.Instance.Add(item, false);
                trans.Commit();
            }

            return result;
        }

        public T Detach<T>(T item) where T : RealmObject
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(
                Newtonsoft.Json.JsonConvert.SerializeObject(item)
                    .Replace(",\"IsManaged\":true", ",\"IsManaged\":false")
            );

        }
    }
}
