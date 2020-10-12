using FFImageLoading;
using SQLHelper;
using SQLHelper.Abstractions;
using SQLHelper.Interfaces;
using SyncService.Daemon.Abstractions;
using SyncService.Daemon.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TouchEffect.Delegates;

namespace SyncService.Daemon.Abstractions
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
            this.TableDirection = TableDirection.DOWNLOAD;
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
        internal bool Execute(DaemonConfig config, Pendientes pendiente, DireccionDemonio direccion)
        {
            if (string.IsNullOrEmpty(pendiente.LLave?.ToString()))
            {
                Log.LogMeDemonio("WARNING->{SOLICITUD DE ACTUALIZACION SIN LLAVE IGNORADA}");
                return true;
            }
            try
            {
                DireccionDemonio origen = direccion;
                switch (direccion)
                {
                    case DireccionDemonio.TO_DESTINY:
                        origen = DireccionDemonio.TO_ORIGIN;
                        break;
                    case DireccionDemonio.TO_ORIGIN:
                        origen = DireccionDemonio.TO_DESTINY;
                        break;
                }
                if (pendiente.Accion == AccionDemonio.DELETE)
                {
                    IQuery query = ConsultaTabla(config, pendiente, pendiente.Accion, direccion);
                    query.Execute();
                }
                else
                {
                    List<ValoresOriginales> valoresOriginales = ValoresRegistroOriginal(config, pendiente, origen);

                    if (direccion == DireccionDemonio.TO_DESTINY)
                    {
                        foreach (ValoresOriginales valores in valoresOriginales)
                        {
                            IQuery query = ConsultaTabla(config, pendiente, pendiente.Accion, direccion, valores.Valores);
                            query.Execute();
                        }
                    }
                    else
                    {
                        Upload(config, pendiente, valoresOriginales);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.LogMeDemonio(ex, $"Al insertar y sincronizar los datos de =>{pendiente.Tabla}->({pendiente.LLave},{pendiente.Accion},{direccion})");
                return false;
            }

            //si no dio error entoces esta ok 
            pendiente.Sincronizado(config, direccion);
            return true;

        }
        private void Upload(DaemonConfig config, Pendientes pendiente, List<ValoresOriginales> valoresOriginales)
        {
            foreach (ValoresOriginales row in valoresOriginales)
            {
                object OldPk = row[0];
                object NewPk = OldPk;
                if (ShouldReserveNewId)
                {
                    if (config.Source is SQLH)
                    {
                        object NewValue = ReserveNewId(config);
                        if (NewValue!=null)
                        {
                            row[0] = NewPk = NewValue;
                        }
                    }
                }
                if (ForeignKeys.Any())
                {
                    foreach (var Fkey in ForeignKeys)
                    {
                        Update.BulidFrom(config.Destination, Fkey.Key)
                            .AddField(Fkey.Value, NewPk)
                            .Where(Fkey.Key, OldPk)
                            .NoReplaceOnSqlite()
                            .Execute();
                        //Sqlh.SQLHLite.EXEC($"UPDATE {Fkey.Key} SET {Fkey.Value}=? WHERE {Fkey.Value}=?", NewPk, OldPk);
                    }
                }
                if (CustomUploadAction != null)
                {
                    if (CustomUploadAction.Invoke(this, pendiente, row))
                    {
                        return;
                    }
                }

                Update query = Update.BulidFrom(config.Source, Name);
                for (int i = 1; i < Fields.Length; i++)
                {
                    query.AddField(Fields[i], row.Valores[i]);
                }
                query.Where(PrimaryKey, NewPk);
                if (query.Execute() == SQLH.Error)
                {
                    throw new Exception("Sincronización fallida");
                }
            }
        }
        private object ReserveNewId(DaemonConfig config)
        {
            if (config.Source is SQLH SQLH)
            {
                return SQLH.Single<object>($"INSERT INTO {Name} DEFAULT VALUES SELECT SCOPE_IDENTITY();", false, System.Data.CommandType.Text);
            }
            return null;
        }
        private List<ValoresOriginales> ValoresRegistroOriginal(DaemonConfig config, Pendientes pendiente, DireccionDemonio direccion)
        {
            //Recuperar valores de el registro original
            int fila = 0;
            List<ValoresOriginales> valores = new List<ValoresOriginales>();
            using (Select select = (Select)ConsultaTabla(config, pendiente, AccionDemonio.SELECT, direccion))
            {
                using (IReader reader = select.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        fila++;
                        ValoresOriginales Vfila = new ValoresOriginales(fila, Fields.Length);
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Vfila.Agregar(reader[i], i);
                        }
                        valores.Add(Vfila);
                    }
                }
            }
            return valores;
        }
        private IQuery ConsultaTabla(DaemonConfig config, Pendientes pendiente, AccionDemonio Accion, DireccionDemonio direccion, object[] valores = null)
        {
            IQuery query;
            BaseSQLHelper Connection = null;
            switch (direccion)
            {
                case DireccionDemonio.TO_DESTINY:
                    Connection = config.Destination;
                    break;
                case DireccionDemonio.TO_ORIGIN:
                    Connection = config.Source;
                    break;
            }
            switch (Accion)
            {
                case AccionDemonio.SELECT:
                    query = Select.BulidFrom(Connection, Name)
                        .AddFields(Fields)
                        .Where(PrimaryKey, pendiente.LLave);
                    break;
                case AccionDemonio.UPDATE:
                    query = SQLHelper.Abstractions.Update.BulidFrom(Connection, Name);
                    for (int i = 0; i < Fields.Length; i++)
                    {
                        ((Update)query).AddField(Fields[i], valores[i]);
                    }
                    ((Update)query).Where(PrimaryKey, pendiente.LLave);
                    break;
                case AccionDemonio.INSERT:
                    query = SQLHelper.Abstractions.Insert.BulidFrom(Connection, Name);
                    for (int i = 0; i < Fields.Length; i++)
                    {
                        ((Insert)query).AddField(Fields[i], valores[i]);
                    }
                    break;
                case AccionDemonio.DELETE:
                    query = SQLHelper.Abstractions.Delete.BulidFrom(Connection, Name);
                    ((Delete)query).Where(PrimaryKey, pendiente.LLave);
                    break;
                case AccionDemonio.INVALIDA:
                default:
                    Log.LogMeDemonio("Accion no conciderada =>'" + pendiente.Accion + "'");
                    return null;
            }
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
    }
}
