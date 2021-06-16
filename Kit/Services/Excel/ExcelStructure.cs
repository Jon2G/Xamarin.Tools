using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Kit.Services.Excel
{
    public abstract class ExcelStructure
    {
        public string HeaderColor { get; set; }
        public string HeaderForeground { get; set; }

    }

    public class ExcelStructureDataTable: ExcelStructure
    {
        public DataTable Table { get; set; }
        public bool MakeHeader { get; internal set; }

        public ExcelStructureDataTable(DataTable Table)
        {
            this.MakeHeader = true;
            this.Table = Table;
        }
    }
}