
using SQLHelper.Interfaces;
using SQLHelper.Readers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLHelper
{
    public class SQLH : BaseSQLHelper
    {
        public SQLH(string ConnectionString) : base(ConnectionString)
        {

        }
        public SQLH() : base()
        {

        }
        public async Task<Exception> PruebaConexion(string CadenaCon = null)
        {
            try
            {
                await Task.Yield();
                using SqlConnection con = string.IsNullOrEmpty(CadenaCon) ? Con() : new SqlConnection(CadenaCon);
                con.Open();
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }
        public List<Tuple<string, Type>> GetColumnsName(string consulta, params SqlParameter[] Parametros)
        {
            List<Tuple<string, Type>> listacolumnas = new List<Tuple<string, Type>>();
            DataTable dataTable = new DataTable();
            using (SqlConnection con = Con())
            {
                con.Open();
                using SqlCommand cmd = new SqlCommand(consulta, con);
                cmd.Parameters.AddRange(Parametros);
                using SqlDataAdapter dataContent = new SqlDataAdapter(cmd);
                dataContent.Fill(dataTable);

                foreach (DataColumn col in dataTable.Columns)
                {
                    listacolumnas.Add(new Tuple<string, Type>(col.ColumnName, col.DataType));
                }
            }
            return listacolumnas;
        }
        public string TipoDato(string Tabla, string Campo)
        {
            return Single<string>(@"SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TABLA AND COLUMN_NAME = @CAMPO", false, System.Data.CommandType.Text
                , new SqlParameter("TABLA", Tabla)
                , new SqlParameter("CAMPO", Campo)
                );
        }
        public void ChangeConnectionString(string CadenaCon)
        {
            this.ConnectionString = CadenaCon;
        }
        public SqlConnection Con()
        {
            return new SqlConnection(ConnectionString);
        }
        public void Querry(string sql, CommandType type = CommandType.StoredProcedure, bool Reportar = true)
        {
            using SqlConnection con = Con();
            con.Open();
            using (SqlCommand cmd = new SqlCommand(sql, con) { CommandType = type })
            {
                cmd.ExecuteNonQuery();
                if (Reportar)
                    ReportaTransaccion(cmd);
            }
            con.Close();
        }
        public T Single<T>(string sql, bool Reportar = true, CommandType type = CommandType.StoredProcedure)
        {
            T result = default;
            using (IReader reader = Leector(sql, type, Reportar))
            {
                if (reader.Read())
                {
                    if (reader[0] != DBNull.Value)
                    {
                        result =
                            typeof(T).IsEnum
                                ? (T)Enum.Parse(typeof(T), reader[0].ToString(), true)
                                : (T)Convert.ChangeType(reader[0], typeof(T));
                    }
                }
            }
            return result;
        }
        public T Single<T>(string sql, bool Reportar = true, CommandType type = CommandType.Text, params SqlParameter[] parametros)
        {
            T result = default;
            try
            {
                using (var reader = Leector(sql, type, false, parametros))
                {
                    if (reader.Read())
                    {
                        result = (T)Convert.ChangeType(reader[0], typeof(T));
                    }
                }
                if (Reportar)
                {
                    Log.LogMeSQL(sql);
                }
            }
            catch (Exception ex)
            {
                Log.LogMe(ex, "Consulta fallida");
                Log.LogMeSQL(sql);
                Log.AlertOnDBConnectionError(ex);
            }
            return result;
        }
        [Obsolete("No se deberia utilizar mas procedimientos alamcenados debido a su dificil versionamiento", false)]
        public T Single<T>(string sql, bool Reportar = true, params SqlParameter[] parametros)
        {
            T result = default;
            using (var reader = Leector(sql, CommandType.StoredProcedure, false, parametros))
            {
                if (reader.Read())
                {
                    result = (T)Convert.ChangeType(reader[0], typeof(T));
                }
            }
            return result;
        }
        [Obsolete("No se deberia utilizar mas procedimientos alamcenados debido a su dificil versionamiento", false)]
        public int EXEC(string procedimiento, bool Reportar = true, params SqlParameter[] parametros)
        {
            int Rows = -1;
            using (SqlConnection con = Con())
            {
                con.Open();
                using SqlCommand cmd = new SqlCommand(procedimiento, con)
                { CommandType = CommandType.StoredProcedure };
                if (parametros.Any(x => x.Value is null))
                {
                    foreach (SqlParameter t in parametros)
                    {
                        if (t.Value is null)
                        {
                            t.Value = DBNull.Value;
                        }
                        if (!parametros.Any(x => x.Value is null))
                            break;
                    }
                }
                cmd.Parameters.AddRange(parametros);
                if (Reportar)
                    ReportaTransaccion(cmd);
                Rows = cmd.ExecuteNonQuery();
                con.Close();

            }
            return Rows;
        }
        public int EXEC(string procedimiento, CommandType commandType = CommandType.StoredProcedure, bool Reportar = true, params SqlParameter[] parametros)
        {
            int Rows = -1;
            using (SqlConnection con = Con())
            {
                con.Open();
                using SqlCommand cmd = new SqlCommand(procedimiento, con)
                { CommandType = commandType };
                if (parametros.Any(x => x.Value is null))
                {
                    foreach (SqlParameter t in parametros)
                    {
                        if (t.Value is null)
                        {
                            t.Value = DBNull.Value;
                        }
                        if (!parametros.Any(x => x.Value is null))
                            break;
                    }
                }
                cmd.Parameters.AddRange(parametros);
                try
                {
                    Rows = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Log.LogMe(ex, "Transaccion fallida reportada");
                    ReportaTransaccion(cmd);
                    if (SQLHelper.Instance.Debugging)
                    {
                        throw;
                    }

                    Rows = -2;
                }

                if (Reportar)
                    ReportaTransaccion(cmd);
                con.Close();

            }
            return Rows;
        }
        public List<T> Lista<T>(string sql, CommandType type = CommandType.StoredProcedure, bool Reportar = true, int indice = 0, params SqlParameter[] parameters)
        {
            List<T> result = new List<T>();
            try
            {
                using (SqlConnection con = Con())
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, con) { CommandType = type })
                    {
                        cmd.Parameters.AddRange(parameters);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add((T)Convert.ChangeType(reader[indice], typeof(T)));
                            }
                        }
                        if (Reportar)
                            ReportaTransaccion(cmd);
                    }
                    con.Close();
                }
            }catch(Exception ex)
            {
                Log.AlertOnDBConnectionError(ex);
            }
            return result;
        }
        public List<T> Lista<T>(string sql, bool Reportar = true, int indice = 0, params SqlParameter[] parameters)
        {
            List<T> result = new List<T>();
            using (SqlConnection con = Con())
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddRange(parameters);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader[indice] == DBNull.Value)
                            {
                                result.Add(default);
                                continue;
                            }
                            result.Add((T)Convert.ChangeType(reader[indice], typeof(T)));
                        }
                    }
                    if (Reportar)
                        ReportaTransaccion(cmd);
                }
                con.Close();
            }
            return result;
        }
        //ListaTupla
        public List<Tuple<T, Q>> ListaTupla<T, Q>(string sql, CommandType type = CommandType.StoredProcedure, params SqlParameter[] parameters)
        {
            List<Tuple<T, Q>> result = new List<Tuple<T, Q>>();
            using (SqlConnection con = Con())
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con) { CommandType = type })
                {
                    cmd.Parameters.AddRange(parameters);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        result.Add(new Tuple<T, Q>
                            ((T)Convert.ChangeType(reader[0], typeof(T)),
                            (Q)Convert.ChangeType(reader[1], typeof(Q))));
                    }
                }
                con.Close();
            }
            return result;
        }
        public List<Tuple<T, Q>> ListaTupla<T, Q>(string sql, params SqlParameter[] parameters)
        {
            List<Tuple<T, Q>> result = new List<Tuple<T, Q>>();
            using (SqlConnection con = Con())
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddRange(parameters);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Tuple<T, Q>
                                ((T)Convert.ChangeType(reader[0], typeof(T)),
                                (Q)Convert.ChangeType(reader[1], typeof(Q))));
                        }
                    }
                    ReportaTransaccion(cmd);
                }
                con.Close();
            }
            return result;
        }
        public DataTable DataTable(string Querry, CommandType commandType = CommandType.StoredProcedure, string TableName = null, bool Reportar = true, params SqlParameter[] parameters)
        {
            DataTable result = new DataTable(TableName);
            using (SqlConnection con = Con())
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(Querry, con) { CommandType = commandType })
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    try
                    {
                        result.Load(cmd.ExecuteReader());
                    }
                    catch (Exception ex)
                    {
                        ReportaTransaccion(cmd);
                        Log.LogMe(ex);
                        throw ex;
                    }
                    if (Reportar)
                        ReportaTransaccion(cmd);
                }
                con.Close();
            }
            return result;
        }
        public DataTable DataTable(string Querry, string TableName = null, params SqlParameter[] parameters)
        {
            return DataTable(Querry, CommandType.Text, TableName, false, parameters);
        }
        public IReader Leector(string sql, CommandType commandType = CommandType.StoredProcedure, bool Reportar = true, params SqlParameter[] parameters)
        {
            if (SQLHelper.Instance.Debugging)
            {
                if (commandType == CommandType.StoredProcedure)
                {
                    Log.LogMe($"STRORED PROCEDURE=>{sql}");
                }
            }
            using (SqlCommand cmd = new SqlCommand(sql, Con()) { CommandType = commandType })
            {
                try
                {

                    cmd.Parameters.AddRange(parameters);
                    cmd.Connection.Open();
                    if (Reportar)
                        ReportaTransaccion(cmd);
                    return new Reader(cmd);
                }
                catch (Exception ex)
                {
                    Log.LogMeSQL("Transaccion fallida reportada");
                    Log.LogMeSQL(GetCommandText(cmd));
                    if (!Log.AlertOnDBConnectionError(ex) && SQLHelper.Instance.Debugging)
                    {
                        throw ex;
                    }
                    return new FakeReader();
                }
            }
        }
        public void RunBatch(string Batch, bool Reportar = false)
        {
            Batch += "\nGO";   // make sure last batch is executed.
            try
            {
                foreach (string line in Batch.Split(new string[2] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.ToUpperInvariant().Trim() == "GO")
                    {
                        if (!string.IsNullOrEmpty(Batch))
                            EXEC(Batch, CommandType.Text, Reportar);
                        Batch = string.Empty;
                    }
                    else
                    {
                        Batch += line + "\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogMeDemonio(ex.Message);
            }
        }
        public async Task<Reader> AsyncLeector(string sql, CommandType commandType = CommandType.StoredProcedure, bool Reportar = true, params SqlParameter[] parameters)
        {
            using SqlCommand cmd = new SqlCommand(sql, Con()) { CommandType = commandType };
            try
            {
                cmd.Parameters.AddRange(parameters);
                cmd.Connection.Open();
                if (Reportar)
                    ReportaTransaccion(cmd);
                return await new Reader().AsyncReader(cmd);
            }
            catch (Exception ex)
            {
                if (SQLHelper.Instance.Debugging)
                {
                    throw ex;
                }
                Log.LogMeSQL("Transaccion fallida reportada");
                Log.LogMeSQL(GetCommandText(cmd));
                return null;
            }
        }
        [Obsolete("No se deberia utilizar mas procedimientos alamcenados debido a su dificil versionamiento", false)]
        public Reader Leector(string sql, bool Reportar, params SqlParameter[] parameters)
        {
            try
            {
                using SqlCommand cmd = new SqlCommand(sql, Con()) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddRange(parameters);
                cmd.Connection.Open();
                return new Reader(cmd);
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
                return null;
            }
        }
        public bool Exists(string sql, bool Reportar = false, params SqlParameter[] parametros)
        {
            bool result = false;
            using (IReader reader = Leector(sql, CommandType.Text, Reportar, parametros))
            {
                if (reader != null)
                {
                    result = reader.Read();
                }
            }
            return result;
        }
        public bool ExisteCampo(string Tabla, string Campo)
        {
            return Exists($@"SELECT 1 FROM sys.columns WHERE name = N'{Campo}' AND Object_ID = Object_ID(N'{Tabla}')", false);
        }
        public bool ExisteTabla(string Tabla)
        {
            return Exists($"(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{Tabla}')", false);
        }
        public bool TieneIdentidad(string Tabla)
        {
            return Exists("SELECT * from syscolumns where id = Object_ID(@TABLE_NAME) and colstat & 1 = 1", false,
                new SqlParameter("TABLE_NAME", Tabla));
        }
        public void ReportaTransaccion(SqlCommand cmd)
        {
            string sql = GetCommandText(cmd);
            Log.LogMeSQL(sql);
            //if (Settings.Depurando)
            //{
            //    Debugger.Break();
            //}
        }
        private string GetCommandText(SqlCommand sqc)
        {
            StringBuilder sbCommandText = new StringBuilder();
            sbCommandText.AppendLine("GO");
            //sbCommandText.AppendLine("-- INICIA");

            // params
            for (int i = 0; i < sqc.Parameters.Count; i++)
                LogParameterToSqlBatch(sqc.Parameters[i], sbCommandText);
            //sbCommandText.AppendLine("-- END PARAMS");

            // command
            if (sqc.CommandType == CommandType.StoredProcedure)
            {
                sbCommandText.Append("EXEC ");

                bool hasReturnValue = false;
                for (int i = 0; i < sqc.Parameters.Count; i++)
                {
                    if (sqc.Parameters[i].Direction == ParameterDirection.ReturnValue)
                        hasReturnValue = true;
                }
                if (hasReturnValue)
                {
                    sbCommandText.Append("@returnValue = ");
                }

                sbCommandText.Append(sqc.CommandText + (sqc.Parameters.Count > 0 ? " " : ""));

                bool hasPrev = false;
                for (int i = 0; i < sqc.Parameters.Count; i++)
                {
                    var cParam = sqc.Parameters[i];
                    if (cParam.Direction != ParameterDirection.ReturnValue)
                    {
                        if (hasPrev)
                            sbCommandText.Append(", ");

                        sbCommandText.Append("@" + cParam.ParameterName);
                        sbCommandText.Append(" = ");
                        sbCommandText.Append("@" + cParam.ParameterName);

                        if (cParam.Direction.HasFlag(ParameterDirection.Output))
                            sbCommandText.Append(" OUTPUT");

                        hasPrev = true;
                    }
                }
            }
            else
            {
                sbCommandText.Append(sqc.CommandText);
            }

            //sbCommandText.AppendLine("-- RESULTS");
            //sbCommandText.Append("SELECT 1 as Executed");
            for (int i = 0; i < sqc.Parameters.Count; i++)
            {
                SqlParameter cParam = sqc.Parameters[i];

                if (cParam.Direction == ParameterDirection.ReturnValue)
                {
                    sbCommandText.AppendLine(", @returnValue as ReturnValue");
                }
                else if (cParam.Direction.HasFlag(ParameterDirection.Output))
                {
                    sbCommandText.AppendLine(", ");
                    sbCommandText.Append(cParam.ParameterName);
                    sbCommandText.Append(" as [");
                    sbCommandText.Append(cParam.ParameterName);
                    sbCommandText.Append(']');
                }
            }
            sbCommandText.Append(";");

            //sbCommandText.AppendLine("-- END COMMAND");
            return sbCommandText.ToString();
        }
        private void LogParameterToSqlBatch(SqlParameter param, StringBuilder sbCommandText)
        {
            sbCommandText.Append("DECLARE ");
            if (param.Direction == ParameterDirection.ReturnValue)
            {
                sbCommandText.AppendLine("@returnValue INT;");
            }
            else
            {
                sbCommandText.Append("@" + param.ParameterName);

                sbCommandText.Append(' ');
                try
                {
                    if (param.SqlDbType != SqlDbType.Structured)
                    {
                        LogParameterType(param, sbCommandText);
                        sbCommandText.Append(" = ");
                        LogQuotedParameterValue(param.Value, sbCommandText);

                        sbCommandText.AppendLine(";");
                    }
                    else
                    {
                        LogStructuredParameter(param, sbCommandText);
                    }
                }
                catch (Exception)
                {
                    sbCommandText.AppendLine($" sql_variant ={param.Value};");
                }
            }
        }
        private void LogStructuredParameter(SqlParameter param, StringBuilder sbCommandText)
        {
            sbCommandText.AppendLine(" {List Type};");
            DataTable dataTable = (DataTable)param.Value;

            for (int rowNo = 0; rowNo < dataTable.Rows.Count; rowNo++)
            {
                sbCommandText.Append("INSERT INTO ");
                sbCommandText.Append(param.ParameterName);
                sbCommandText.Append(" VALUES (");

                bool hasPrev = false;
                for (int colNo = 0; colNo < dataTable.Columns.Count; colNo++)
                {
                    if (hasPrev)
                    {
                        sbCommandText.Append(", ");
                    }
                    LogQuotedParameterValue(dataTable.Rows[rowNo].ItemArray[colNo], sbCommandText);
                    hasPrev = true;
                }
                sbCommandText.AppendLine(");");
            }
        }
        const string DATETIME_FORMAT_ROUNDTRIP = "o";
        private void LogQuotedParameterValue(object value, StringBuilder sbCommandText)
        {
            try
            {
                if (value == null)
                {
                    sbCommandText.Append("NULL");
                }
                else
                {
                    value = UnboxNullable(value);

                    if (value is string
                        || value is char
                        || value is char[]
                        || value is System.Xml.Linq.XElement
                        || value is System.Xml.Linq.XDocument)
                    {
                        sbCommandText.Append("N'");
                        sbCommandText.Append(value.ToString().Replace("'", "''"));
                        sbCommandText.Append('\'');
                    }
                    else if (value is bool)
                    {
                        // True -> 1, False -> 0
                        sbCommandText.Append(Convert.ToInt32(value));
                    }
                    else if (value is sbyte
                        || value is byte
                        || value is short
                        || value is ushort
                        || value is int
                        || value is uint
                        || value is long
                        || value is ulong
                        || value is float
                        || value is double
                        || value is decimal)
                    {
                        sbCommandText.Append(value.ToString());
                    }
                    else if (value is DateTime time)
                    {
                        // SQL Server only supports ISO8601 with 3 digit precision on datetime,
                        // datetime2 (>= SQL Server 2008) parses the .net format, and will 
                        // implicitly cast down to datetime.
                        // Alternatively, use the format string "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK"
                        // to match SQL server parsing
                        sbCommandText.Append("CAST('");
                        sbCommandText.Append(time.ToString(DATETIME_FORMAT_ROUNDTRIP));
                        sbCommandText.Append("' as datetime2)");
                    }
                    else if (value is DateTimeOffset offset)
                    {
                        sbCommandText.Append('\'');
                        sbCommandText.Append(offset.ToString(DATETIME_FORMAT_ROUNDTRIP));
                        sbCommandText.Append('\'');
                    }
                    else if (value is Guid guid)
                    {
                        sbCommandText.Append('\'');
                        sbCommandText.Append(guid.ToString());
                        sbCommandText.Append('\'');
                    }
                    else if (value is byte[] data)
                    {
                        if (data.Length == 0)
                        {
                            sbCommandText.Append("NULL");
                        }
                        else
                        {
                            sbCommandText.Append("0x");
                            foreach (byte t in data)
                            {
                                sbCommandText.Append(t.ToString("h2"));
                            }
                        }
                    }
                    else if (value == DBNull.Value)
                    {
                        sbCommandText.Append("NULL");
                    }
                    else
                    {
                        sbCommandText.Append("/* UNKNOWN DATATYPE: ");
                        sbCommandText.Append(value.GetType().ToString());
                        sbCommandText.Append(" *" + "/ N'");
                        sbCommandText.Append(value.ToString());
                        sbCommandText.Append('\'');
                    }
                }
            }

            catch (Exception ex)
            {
                sbCommandText.AppendLine("/* Exception occurred while converting parameter: ");
                sbCommandText.AppendLine(ex.ToString());
                sbCommandText.AppendLine("*/");
            }
        }
        private object UnboxNullable(object value)
        {
            Type typeOriginal = value.GetType();
            if (typeOriginal.IsGenericType
                && typeOriginal.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // generic value, unboxing needed
                return typeOriginal.InvokeMember("GetValueOrDefault",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.InvokeMethod,
                    null, value, null);
            }
            else
            {
                return value;
            }
        }
        private void LogParameterType(SqlParameter param, StringBuilder sbCommandText)
        {
            switch (param.SqlDbType)
            {
                // variable length
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.Binary:
                    {
                        sbCommandText.Append(param.SqlDbType.ToString().ToUpper());
                        sbCommandText.Append('(');
                        sbCommandText.Append(param.Size);
                        sbCommandText.Append(')');
                    }
                    break;
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:
                case SqlDbType.VarBinary:
                    {
                        sbCommandText.Append(param.SqlDbType.ToString().ToUpper());
                        sbCommandText.Append("(MAX");
                        //sbCommandText.Append("/* Specified as ");
                        //sbCommandText.Append(param.Size);
                        //sbCommandText.Append(" */");
                        sbCommandText.Append(")");
                    }
                    break;
                // fixed length
                case SqlDbType.Text:
                case SqlDbType.NText:
                case SqlDbType.Bit:
                case SqlDbType.TinyInt:
                case SqlDbType.SmallInt:
                case SqlDbType.Int:
                case SqlDbType.BigInt:
                case SqlDbType.SmallMoney:
                case SqlDbType.Money:
                case SqlDbType.Decimal:
                case SqlDbType.Real:
                case SqlDbType.Float:
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                case SqlDbType.UniqueIdentifier:
                case SqlDbType.Image:
                    {
                        sbCommandText.Append(param.SqlDbType.ToString().ToUpper());
                    }
                    break;
                // Unknown
                case SqlDbType.SmallDateTime:
                    break;
                case SqlDbType.Variant:
                    break;
                case SqlDbType.Xml:
                    break;
                case SqlDbType.Udt:
                    break;
                case SqlDbType.Structured:
                    break;
                case SqlDbType.Time:
                    break;
                case SqlDbType.Timestamp:
                default:
                    {
                        sbCommandText.Append("/* UNKNOWN DATATYPE: ");
                        sbCommandText.Append(param.SqlDbType.ToString().ToUpper());
                        sbCommandText.Append(" *" + "/ ");
                        sbCommandText.Append(param.SqlDbType.ToString().ToUpper());
                    }
                    break;
            }
        }
        public void CrearCampo(string Tabla, string Campo, string TipoDato, bool Nulleable)
        {
            Querry($"alter table [{Tabla}] add [{Campo}] {TipoDato} {(Nulleable ? "" : "NOT NULL")}", CommandType.Text, true);
        }
    }
}
