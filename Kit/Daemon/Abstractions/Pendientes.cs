using Kit.Sql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Kit.Daemon.Devices;
using Kit.Enums;
using Kit.Daemon.Enums;
using Kit.Daemon.Helpers;
using Kit.Sql.Interfaces;
using Kit.Sql.Helpers;
namespace Kit.Daemon.Abstractions
{
    public class Pendientes
    {
        public AccionDemonio Accion { get; set; }
        public object LLave { get; private set; }
        //public string Campo { get; private set; }
        public string Tabla { get; private set; }
        public int Id { get; private set; }
        public Pendientes(AccionDemonio Accion, object LLave, string Tabla, int Id)
        {
            this.Accion = Accion;
            this.LLave = LLave;
            //this.Campo = Campo.ToUpper();
            this.Tabla = Tabla.ToUpper();
            this.Id = Id;
        }

  
    }
}
