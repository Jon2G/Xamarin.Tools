using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kit.WPF.Reportes
{
    public struct Variable
    {
        public object Data { get; private set; }
        public string Nombre { get; private set; }
        public Variable(string Nombre, object Data)
        {
            this.Data = Data;
            this.Nombre = Nombre;
        }
        public Variable(DataTable data)
        {
            this.Data = data;
            this.Nombre = data.TableName;
        }
        public Variable(DataSet data)
        {
            this.Data = data;
            this.Nombre = data.DataSetName;
        }
    }
}
