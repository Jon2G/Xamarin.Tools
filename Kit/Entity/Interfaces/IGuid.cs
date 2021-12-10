using Kit.Sql.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Kit.Sql.Interfaces
{
    public interface IGuid
    {
#if !NET48
        [NotNull]
#endif
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Index(IsClustered = true, IsUnique = true)]
        public Guid Guid { get; set; }
    }
}
