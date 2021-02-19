using Kit.Sql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Kit.Enums;
using Kit.Daemon.Enums;
using Kit.Sql.Base;
using static Kit.Daemon.Helpers.Helper;
using Kit.Sql.Helpers;
using Kit.Sql.Interfaces;
using SQLServer;

namespace Kit.Daemon.Abstractions
{
    public class Table
    {
        public TableDirection TableDirection;
        public readonly string Name;
        public readonly string PrimaryKey;
        public readonly string[] Fields;
        private bool ShouldReserveNewId;
        public int Priority { get; private set; }
        private readonly Dictionary<string, string> ForeignKeys;
        public Func<Table, Pendientes, ValoresOriginales, bool> CustomUploadAction;
        public Table(string Name, string PrimaryKey, params string[] Fields)
        {
            TableDirection = TableDirection.DOWNLOAD;
            this.Name = Name;
            this.PrimaryKey = PrimaryKey;
            this.Fields = (new string[] { PrimaryKey }).Concat(Fields).ToArray();
            ForeignKeys = new Dictionary<string, string>();
        }
        public Table SetTableDirection(TableDirection TableDirection)
        {
            this.TableDirection = TableDirection;
            return this;
        }
        public Table SetPriority(int Priority)
        {
            this.Priority = Priority;
            return this;
        }
        public Table SetCustomUploadAction(Func<Table, Pendientes, ValoresOriginales, bool> CustomUploadAction)
        {
            this.CustomUploadAction = CustomUploadAction;
            return this;
        }
        internal bool Execute(Pendientes pendiente, SyncDirecction direccion)
        {
            if (string.IsNullOrEmpty(pendiente.LLave?.ToString()))
            {
                Log.Logger.Error("WARNING->{SOLICITUD DE ACTUALIZACION SIN LLAVE IGNORADA}");
                return true;
            }
            try
            {
                SyncDirecction origen = direccion.InvertDirection();
                if (pendiente.Accion == AccionDemonio.DELETE)
                {
                    IQuery query = ConsultaTabla(pendiente, pendiente.Accion, direccion);
                    //return query.Execute() != SqlServer.Error;
                }
                else
                {
                    List<ValoresOriginales> valoresOriginales = ValoresRegistroOriginal(pendiente, origen);


                    if (direccion == SyncDirecction.Local)
                    {
                        bool correcto = true;
                        foreach (ValoresOriginales valores in valoresOriginales)
                        {
                            IQuery query = ConsultaTabla(pendiente, pendiente.Accion, direccion, valores.Valores);
                            correcto = query.Execute() > 0;
                            //si no dio error entoces esta ok 
                        }
                        if (correcto)
                            pendiente.Sincronizado(direccion);
                        return correcto;
                    }
                    else
                    {
                        //if (Upload(Daemon.Current.DaemonConfig[SyncDirecction.Remote], pendiente, valoresOriginales))
                        //{
                        //    //si no dio error entoces esta ok 
                        //    pendiente.Sincronizado(direccion);
                        //    return true;
                        //}
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Al insertar y sincronizar los datos de =>{pendiente.Tabla}->({pendiente.LLave},{pendiente.Accion},{direccion})");
                return false;
            }
            return false;
        }
        private bool Upload(SqlBase Destination, Pendientes pendiente, List<ValoresOriginales> valoresOriginales)
        {
            foreach (ValoresOriginales row in valoresOriginales)
            {
                object OldPk = row[0];
                object NewPk = OldPk;
                if (ShouldReserveNewId)
                {
                    if (Destination is SQLServerConnection)
                    {
                        object NewValue = ReserveNewId(Destination);
                        if (NewValue != null)
                        {
                            row[0] = NewPk = NewValue;
                        }
                    }
                }
                if (ForeignKeys.Any())
                {
                    //foreach (KeyValuePair<string, string> Fkey in ForeignKeys)
                    //{
                    //    if (Daemon.Current.DaemonConfig.GetSqlLiteConnection()
                    //        .EXEC($"UPDATE {Fkey.Key} SET {Fkey.Value}=? WHERE {Fkey.Value}=?;", NewPk, OldPk) == SqlServer.Error ||
                    //        Daemon.Current.DaemonConfig.GetSqlLiteConnection()
                    //        .EXEC($"UPDATE {pendiente.Tabla} SET {this.PrimaryKey}=? WHERE {this.PrimaryKey}=?;", NewPk, OldPk) == SqlServer.Error
                    //       //Update.BulidFrom(Daemon.Current.DaemonConfig.Local, Fkey.Key)
                    //       //.AddField(Fkey.Value, NewPk)
                    //       //.Where(Fkey.Value, OldPk)
                    //       //.NoReplaceOnSqlite()
                    //       //.Execute()
                    //       )
                    //    {
                    //        return false;
                    //    }
                    //    //Sqlh.SQLHLite.EXEC($"UPDATE {Fkey.Key} SET {Fkey.Value}=? WHERE {Fkey.Value}=?", NewPk, OldPk);
                    //}
                }
                if (CustomUploadAction != null)
                {
                    return CustomUploadAction.Invoke(this, pendiente, row);
                }

                //Update query = Update.BulidFrom(Destination, Name);
                //for (int i = 1; i < Fields.Length; i++)
                //{
                //    query.AddField(Fields[i], row.Valores[i]);
                //}
                //query.Where(PrimaryKey, NewPk);
                //if (query.Execute() == BaseSQLHelper.Error)
                //{
                //    throw new Exception("Sincronización fallida");
                //}
                //else
                //{
                //    return true;
                //}
            }
            return false;
        }
        private object ReserveNewId(SqlBase sql)
        {
            //if (sql is SQLServerConnection SQLH)
            //{
            //    return sql.Single($"INSERT INTO {Name} DEFAULT VALUES SELECT SCOPE_IDENTITY();");
            //}
            return null;
        }
        private List<ValoresOriginales> ValoresRegistroOriginal(Pendientes pendiente, SyncDirecction direccion)
        {
            //Recuperar valores de el registro original
            int fila = 0;
            List<ValoresOriginales> valores = new List<ValoresOriginales>();
            //using (Select select = (Select)ConsultaTabla(pendiente, AccionDemonio.SELECT, direccion))
            //{
            //    using (IReader reader = select.ExecuteReader())
            //    {
            //        while (reader.Read())
            //        {
            //            fila++;
            //            ValoresOriginales Vfila = new ValoresOriginales(fila, Fields.Length);
            //            for (int i = 0; i < reader.FieldCount; i++)
            //            {
            //                Vfila.Agregar(reader[i], i);
            //            }
            //            valores.Add(Vfila);
            //        }
            //    }
            //}
            return valores;
        }
        private IQuery ConsultaTabla(Pendientes pendiente, AccionDemonio Accion, SyncDirecction direccion, object[] valores = null)
        {
            IQuery query=null;
            //BaseSQLHelper Connection = Daemon.Current.DaemonConfig[direccion];


            //switch (Accion)
            //{
            //    case AccionDemonio.SELECT:
            //        query = Select.BulidFrom(Connection, Name)
            //            .AddFields(Fields)
            //            .Where(PrimaryKey, pendiente.LLave);
            //        break;
            //    case AccionDemonio.UPDATE:
            //        query = Update.BulidFrom(Connection, Name);
            //        for (int i = 0; i < Fields.Length; i++)
            //        {
            //            ((Update)query).AddField(Fields[i], valores[i]);
            //        }
            //        ((Update)query).Where(PrimaryKey, pendiente.LLave);
            //        break;
            //    case AccionDemonio.INSERT:
            //        query = Insert.BulidFrom(Connection, Name);
            //        for (int i = 0; i < Fields.Length; i++)
            //        {
            //            ((Insert)query).AddField(Fields[i], valores[i]);
            //        }
            //        break;
            //    case AccionDemonio.DELETE:
            //        query = Delete.BulidFrom(Connection, Name);
            //        ((Delete)query).Where(PrimaryKey, pendiente.LLave);
            //        break;
            //    case AccionDemonio.INVALIDA:
            //    default:
            //        Log.LogMeDemonio("Accion no conciderada =>'" + pendiente.Accion + "'");
            //        return null;
            //}
            return query;
        }
        public Table ReserveNewId(bool ReserveNewId)
        {
            ShouldReserveNewId = ReserveNewId;
            return this;
        }
        public Table Affects(string TableName, string ForeignKey)
        {
            ForeignKeys.Add(TableName, ForeignKey);
            return this;
        }
        public override string ToString()
        {
            return new StringBuilder(Name).Append('-').Append(PrimaryKey).ToString();
        }

        public static void RemoveFromVersionControl(string TableName, SqlBase sql)
        {
            //switch (sql)
            //{
            //    case SqLite lite:
            //        RemoveFromVersionControl(TableName, lite);
            //        break;
            //    case SqlServer server:
            //        RemoveFromVersionControl(TableName, server);
            //        break;
            //}
        }
        //private static void RemoveFromVersionControl(string TableName, SqLite sql)
        //{

        //}
        //private static void RemoveFromVersionControl(string TableName, SqlServer sql)
        //{
        //    try
        //    {
        //        sql.EXEC("DELETE FROM VERSION_CONTROL WHERE TABLA=@TABLE", new SqlParameter("TABLE", TableName));
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.LogMeDemonio(ex, $"Al eliminar la tabla [{TableName}] del control de versiones de sql server");
        //    }
        //}
    }
}
