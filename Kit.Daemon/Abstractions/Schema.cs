using SQLHelper.Linker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kit.Daemon.Enums;

namespace Kit.Daemon.Abstractions
{
    [Preserve(AllMembers = true)]
    public class Schema
    {
        public readonly HashSet<Table> Tables;
        public readonly IEnumerable<Table> UploadTables;
        public readonly IEnumerable<Table> DownloadTables;
        private HashSet<string> DeniedTables;
        public readonly bool HasDownloadTables;
        public readonly bool HasUploadTables;
        public Schema(params Table[] tables)
        {
            this.Tables = new HashSet<Table>(tables);
            this.UploadTables = this.Tables.Where(x => x.TableDirection == TableDirection.UPLOAD || x.TableDirection == TableDirection.TWO_WAY).OrderByDescending(x => x.Priority);
            this.DownloadTables = this.Tables.Where(x => x.TableDirection == TableDirection.DOWNLOAD || x.TableDirection == TableDirection.TWO_WAY).OrderByDescending(x => x.Priority);
            this.HasDownloadTables = this.DownloadTables.Any();
            this.HasUploadTables = this.UploadTables.Any();
        }
        public Table this[string TableName, SyncDirecction direcction]
        {
            get
            {
                if (DeniedTables != null && DeniedTables.Contains(TableName))
                {
                    return null;
                }
                Table table = this.Tables.FirstOrDefault(x => string.Compare(x.Name, TableName, true) == 0);
                if (table is null)
                {
                    if (this.DeniedTables is null)
                    {
                        this.DeniedTables = new HashSet<string>();
                    }
                    DeniedTables.Add(TableName);
                }
                if (!IsValidDirection(table.TableDirection, direcction))
                {
                    return null;
                }
                return table;
            }
        }
        private bool IsValidDirection(TableDirection TableDirection, SyncDirecction UseDirection)
        {
            if (TableDirection == TableDirection.TWO_WAY)
            {
                return true;
            }
            if (TableDirection == TableDirection.UPLOAD && UseDirection == SyncDirecction.Remote)
            {
                return true;
            }
            if (TableDirection == TableDirection.DOWNLOAD && UseDirection == SyncDirecction.Local)
            {
                return true;
            }
            return false;
        }


    }
}
