using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kit.Sql.Attributes;

namespace Kit.Sql.Tables
{
    public class DatabaseVersion
    {
        [Key, Index(IsClustered = true,IsUnique = true)]
        public int Id { get; set; }
        public int Version { get; set; }
        public DatabaseVersion() { }
    }
}
