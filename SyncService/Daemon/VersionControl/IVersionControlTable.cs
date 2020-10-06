using SQLHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SyncService.Daemon.VersionControl
{
    public interface IVersionControlTable
    {
        string TableName { get; }
        void CreateTable(SQLH SQLH);
    }
}
