using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Kit;
using Kit.Sql.Sqlite;
using Kit.Daemon;
using Kit.SetUpConnectionString;
using Kit.Sql.Readers;
using Kit.Daemon.Devices;

namespace BlumAPI.MyB.Versions
{
    public abstract class MybVersion
    {
        public VersionMyBusinessPos Version { get; }
        public DirectoryInfo VirtualDirectory { get; }
        public DirectoryInfo Directory { get; }
        public abstract string ConfigDbRealtivePath { get; }
        public FileInfo ConfigDb { get; private set; }
        public SQLiteConnection Con() => new SQLiteConnection(ConfigDb, 0);
        public MybVersion(VersionMyBusinessPos version, DirectoryInfo directory, DirectoryInfo virtualDirectory)
        {
            this.Version = version;
            this.Directory = directory;
            this.VirtualDirectory = virtualDirectory;
            LocateConfigDb();
            Log.Logger.Information("MyB VERSION  - {0}", version);
            Log.Logger.Information("MyB DIRECOTORIO  - {0}", Directory);
            Log.Logger.Information("MyB DIRECOTORIO VIRTUAL - {0}", VirtualDirectory);
            Log.Logger.Information("MyB DIRECOTORIO DB - {0}", ConfigDb);
        }
        protected void LocateConfigDb()
        {
            FileInfo db = new FileInfo($"{VirtualDirectory.FullName}\\{ConfigDbRealtivePath}");
            if (db.Exists)
            {
                ConfigDb = db;
                return;
            }
            db = new FileInfo($"{Directory.FullName}\\{ConfigDbRealtivePath}");
            ConfigDb = db;

        }
        private static DirectoryInfo FindVirtualDirectory(VersionMyBusinessPos version, string subdir)
        {
            DirectoryInfo virutalpath = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\..\\Local\\VirtualStore");
            if (version <= VersionMyBusinessPos.V2019)
            {
                DirectoryInfo programFiles = new DirectoryInfo(Environment.GetFolderPath((Environment.Is64BitOperatingSystem ? Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles)));
                DirectoryInfo programsVirutalpath = new DirectoryInfo($"{virutalpath.FullName}\\{programFiles.Name}\\{subdir}");
                return programsVirutalpath;
            }
            else
            {
                virutalpath = new DirectoryInfo($"{virutalpath.FullName}\\{subdir}");
            }
            return virutalpath;
        }
        private static DirectoryInfo FindProgramFilesDirectory()
        {
            DirectoryInfo directory = new DirectoryInfo(Environment.GetFolderPath((Environment.Is64BitOperatingSystem ? Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles)));
            return directory;
        }
        public static IEnumerable<MybVersion> Find()
        {
            List<MybVersion> versions = new List<MybVersion>();
            foreach (VersionMyBusinessPos version in Enum.GetValues(typeof(VersionMyBusinessPos)))
            {
                var myv = Find(version);
                if (myv is not null)
                {
                    versions.Add(myv);
                }
            }
            return versions;
        }
        public static MybVersion Find(VersionMyBusinessPos version)
        {
            string FolderPath = string.Empty;
            switch (version)
            {
                case VersionMyBusinessPos.V2011:
                    FolderPath = "MyBusiness POS 2011";
                    break;
                case VersionMyBusinessPos.V2012:
                    FolderPath = "MyBusiness POS\\MyBusiness POS 2012";
                    break;
                case VersionMyBusinessPos.V2017:
                    FolderPath = "MyBusiness POS\\MyBusiness POS 2017";
                    break;
                case VersionMyBusinessPos.V2019:
                    FolderPath = "MyBusinessPOS2019";
                    break;
                case VersionMyBusinessPos.V2020:
                    FolderPath = "MyBusinessPOS20";
                    break;
            }
            DirectoryInfo baseDirectory = version <= VersionMyBusinessPos.V2017 ? FindProgramFilesDirectory() : new DirectoryInfo(Environment.CurrentDirectory).Root;
            DirectoryInfo directory = new DirectoryInfo($"{baseDirectory.FullName}\\{FolderPath}");
            if (directory.Exists)
            {
                DirectoryInfo vDirectory = FindVirtualDirectory(version, FolderPath);
                Type type = Type.GetType($"BlumAPI.MyB.Versions.{version}");
                return (MybVersion)Activator.CreateInstance(type, version, directory, vDirectory);
            }
            return null;
        }



        /// <summary>
        /// Comprueba que los iconos de la barra de tareas en MyBusinessPos esten colocados correctamente
        /// </summary>
        public void EstableceImagenesBarras()
        {
            try
            {
                DirectoryInfo directory = new DirectoryInfo($@"{this.Directory}\interface\imagenes");
                if (!directory.Exists)
                {
                    directory.Create();
                }
                if (directory.Exists)
                {
                    string directorioImgs = $"{Tools.Instance.LibraryPath}\\Imgs\\";
                    string directorioDestino = $"{ directory.FullName}\\";
                    foreach (string img in new string[]
                        {"001-tray.png", "003-payment.png", "LogoNombre.png", "001-chef.png", "team.png"})
                    {
                        string destino = string.Concat(directorioDestino, img);
                        if (!File.Exists(destino))
                        {
                            File.Copy(string.Concat(directorioImgs, img), destino, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al establecer las imagenes de barras");
            }
        }

        /// <summary>
        /// Establece los scripts faltantes en los iconos de la barras de tareas
        /// </summary>
        public void EstableceScripts(Kit.Sql.SqlServer.SQLServerConnection con)
        {
            try
            {
                if (Daemon.OffLine)
                {
                    return;
                }
                if (this.Version == VersionMyBusinessPos.V2017)
                {
                    foreach (string modulo in new string[] { "MyGourmetPos.Administrador", "COMANDERA", "COCINA" })
                    {
                        if (con.Exists(
                            $@"SELECT CODIGO FROM formatosdelta WHERE DESCRIP='NUEVA_{modulo}' AND CODIGO NOT LIKE '%--MODULO%'"))
                        {
                            con.EXEC($"DELETE FROM formatosdelta WHERE DESCRIP='NUEVA_{modulo}'");
                        }
                        if (!con.Exists(
                            $"SELECT DESCRIP FROM formatosdelta WHERE DESCRIP='NUEVA_{modulo}'"))
                        {
                            StringBuilder codigo = new StringBuilder();
                            codigo.AppendLine("Imports System");
                            codigo.AppendLine("Imports System.Data");
                            codigo.AppendLine("Imports System.Data.SQLClient");
                            codigo.AppendLine("Imports System.Threading");
                            codigo.AppendLine("Imports Microsoft.VisualBasic");
                            codigo.AppendLine("Imports System.IO");
                            codigo.AppendLine("Imports System.Diagnostics");
                            codigo.AppendLine("Public Class MainClass");
                            codigo.AppendLine("Public Sub Main");
                            codigo.AppendLine("Dim p As ProcessStartInfo");
                            codigo.AppendLine(
                                $"\t\tp= New ProcessStartInfo(\"C:\\MyGourmetPOS\\Instalador.exe\",\"--MODULO \"\"{modulo}\"\"\")");
                            codigo.AppendLine("\t\tProcess.Start(p)");
                            codigo.AppendLine("End Sub");
                            codigo.AppendLine("End Class");
                            con.EXEC
                            (@"INSERT INTO formatosdelta (catalogo, codigo,  descrip, formato, grupo, observ,  tipo,  proyecto,  usuario,  usufecha,  usuhora,  referencias) VALUES (@CATALOGO,@CODIGO,@DESCRIP,@FORMATO,@GRUPO,@OBSERV,@TIPO,@PROYECTO,@USUARIO,@USUFECHA,@USUHORA,@REFERENCIAS)",

                                new System.Data.SqlClient.SqlParameter("CATALOGO", "RESTAURANTE"),
                                new System.Data.SqlClient.SqlParameter("DESCRIP", $"NUEVA_{modulo}"),
                                new System.Data.SqlClient.SqlParameter("FORMATO", $"NUEVA_{modulo}"),
                                new System.Data.SqlClient.SqlParameter("GRUPO", "RESTAURANTE"),
                                new System.Data.SqlClient.SqlParameter("OBSERV", "MYGOURMETPOS"),
                                new System.Data.SqlClient.SqlParameter("TIPO", "Programa .NET"),
                                new System.Data.SqlClient.SqlParameter("PROYECTO", "RESTAURANTE"),
                                new System.Data.SqlClient.SqlParameter("USUARIO", "SUP"),
                                new System.Data.SqlClient.SqlParameter("USUFECHA", DateTime.Today),
                                new System.Data.SqlClient.SqlParameter("USUHORA", DateTime.Now.ToString("HH:mm:ss")),
                                new System.Data.SqlClient.SqlParameter("REFERENCIAS",
                                    @"{app}\sqlcloud\system.diagnostics.debug.dll;"),
                                new System.Data.SqlClient.SqlParameter("CODIGO", codigo.ToString())
                            );
                        }
                    }
                    con.EXEC(
                        "UPDATE opcionesbarra SET  descripcion = 'Comandas', barra = 'restaurante', barratemporal = 'restaurante', activa = 1, imagen = '001-tray.png', tipodeobjeto = 'Programa .NET', modal = 0, objetoaejecutar = 'NUEVA_COMANDERA', usuusuario = 'SUP', usufecha = '20190114', usuhora = '11:11:58' WHERE opcion = 'Comandas';");
                    con.EXEC(
                        "UPDATE opcionesbarra SET descripcion = 'MyGourmetPos.Administrador', barra = 'restaurante', barratemporal = 'restaurante', activa = 1, imagen = 'team.png', tipodeobjeto = 'Programa .NET', modal = 0, objetoaejecutar = 'NUEVA_MyGourmetPos.Administrador', usuusuario = 'SUP', usufecha = '20190114', usuhora = '11:13:23' WHERE opcion = 'Menu';");
                    con.EXEC(
                        "UPDATE opcionesBarra SET imagen='003-payment.png' WHERE opcion='puntodeventa'");
                    con.EXEC(
                        "UPDATE formatosdelta set codigo=REPLACE(codigo,'C:\\Program Files (x86)\\GOURMETPOS\\Imgs\\logo.png','imagenes/LogoNombre.png') where formato='INICIO2016'");
                    con.EXEC(
                        "UPDATE formatosdelta set codigo=REPLACE(codigo,'MyBusiness POS','MyBusiness POS & MyGourmet POS') where formato='INICIO2016'");
                    if (!con.Exists("SELECT OPCION FROM opcionesbarra WHERE OPCION='Cocina'"))
                    {
                        con.EXEC(
                            "INSERT INTO opcionesbarra(opcion, usuario,  orden,ordentemporal,  descripcion,  barra,  barratemporal,  activa,  imagen,  tipodeobjeto,  modal,  objetoaejecutar,  usuusuario,  usufecha,  usuhora) VALUES('Cocina', 'SUP', 7, 7, 'Cocina', 'restaurante', 'restaurante',  1, '001-chef.png', 'Programa .NET', 0,  'NUEVA_COCINA', 'SUP', '20190114', '11:20:14');");
                    }
                    if (!con.Exists("SELECT OPCION FROM opcionesbarra WHERE OPCION='Comandas'"))
                    {
                        con.EXEC(
                            "INSERT INTO opcionesbarra(opcion, usuario,  orden,ordentemporal,  descripcion,  barra,  barratemporal,  activa,  imagen,  tipodeobjeto,  modal,  objetoaejecutar,  usuusuario,  usufecha,  usuhora) VALUES('Comandas', 'SUP', 7, 7, 'Comandas', 'restaurante', 'restaurante',  1, '001-tray.png', 'Programa .NET', 0,  'NUEVA_COMANDERA', 'SUP', '20190114', '11:20:14');");
                    }
                    if (!con.Exists("SELECT OPCION FROM opcionesbarra WHERE OPCION='Menu'"))
                    {
                        con.EXEC(
                            "INSERT INTO opcionesbarra(opcion, usuario,  orden,ordentemporal,  descripcion,  barra,  barratemporal,  activa,  imagen,  tipodeobjeto,  modal,  objetoaejecutar,  usuusuario,  usufecha,  usuhora) VALUES('Menu', 'SUP', 7, 7, 'MyGourmetPos.Administrador', 'restaurante', 'restaurante',  1, 'team.png', 'Programa .NET', 0,  'NUEVA_MyGourmetPos.Administrador', 'SUP', '20190114', '11:20:14');");
                    }
                    //COBRO RAPIDO
                    if (!con.Exists($"SELECT DESCRIP FROM formatosdelta WHERE DESCRIP='MGP_COBRONUEVO'"))
                    {
                        StringBuilder codigo = new StringBuilder();
                        codigo.AppendLine("Imports System");
                        codigo.AppendLine("Imports System.Data");
                        codigo.AppendLine("Imports System.Data.SQLClient");
                        codigo.AppendLine("Imports System.Threading");
                        codigo.AppendLine("Imports Microsoft.VisualBasic");
                        codigo.AppendLine("Imports System.IO");
                        codigo.AppendLine("Imports System.Diagnostics");
                        codigo.AppendLine("Imports System.Windows.Forms");
                        codigo.AppendLine("Imports System.Data.DataTable");
                        codigo.AppendLine("Public Class MainClass");
                        codigo.AppendLine("\tPublic GlobalFunctions As Object");
                        codigo.AppendLine("\tPublic ConnectionString As String");
                        codigo.AppendLine("\tPublic MyParameters As String = \"\"");
                        codigo.AppendLine("\tPublic AppPath As String = \"\"");
                        codigo.AppendLine("\tPublic Sub Main");
                        codigo.AppendLine("\tDim Usuario As String=\"\"");
                        codigo.AppendLine("\tDim Estacion As String=\"\"");
                        codigo.AppendLine("\tDim Venta As String=\"\"");
                        codigo.AppendLine("\tUsuario=ObtieneParametro(\"Usuario\")");
                        codigo.AppendLine("\tEstacion=ObtieneParametro(\"Estacion\")");
                        codigo.AppendLine("\tVenta=ObtieneParametro(\"Venta\")\t");
                        codigo.AppendLine("\tImpresora=ObtieneParametro(\"tImpresora\")\t");
                        codigo.AppendLine("\t'EjecutaNetScript(\"MENSAJE\", \"\", \"\", \"\", True, \"?Mensaje=Usuario\"+Usuario+\",Estacion=\"+Estacion+\",Venta=\"+Venta+\"?Imagen=4?Titulo=MyBusiness POS\")");
                        codigo.AppendLine("\tDim p As ProcessStartInfo");
                        codigo.AppendLine("	\t\tp= New ProcessStartInfo(\"C:\\MyGourmetPOS\\Instalador.exe\",\"--MODULO \"\"COBRO_RAPIDO\"\" --VENTA \"\"\"+Venta+\"\"\" --ESTACION \"\"\"+Estacion+\"\"\" --USUARIO \"\"\"+Usuario+\"\"\" --IMPRESORA \"\"\"+Impresora+\"\"\" \")");
                        codigo.AppendLine("\t\tProcess.Start(p)");
                        codigo.AppendLine("End Sub");
                        codigo.AppendLine("'Obtiene parametro");
                        codigo.AppendLine("Function ObtieneParametro(ByVal strParametro As String) As String");
                        codigo.AppendLine("        Dim iPosicionIni As Integer");
                        codigo.AppendLine("        Dim iPosicionFin As Integer");
                        codigo.AppendLine("        Dim sValor As String = MyParameters");
                        codigo.AppendLine("        Dim sParametro As String");
                        codigo.AppendLine("        Dim bContinua As Boolean = True");
                        codigo.AppendLine("\t\tIf MyParameters = \"\" Then");
                        codigo.AppendLine("        \tReturn \"\"");
                        codigo.AppendLine("\t\tEnd If      ");
                        codigo.AppendLine("        While bContinua");
                        codigo.AppendLine("             iPosicionIni = sValor.IndexOf(\"?\")");
                        codigo.AppendLine("             iPosicionFin = sValor.IndexOf(\"=\")");
                        codigo.AppendLine("            If iPosicionIni >= 0 And iPosicionFin >= 0 Then");
                        codigo.AppendLine("                sParametro = Mid(sValor, (iPosicionIni + 2), (iPosicionFin - 1))");
                        codigo.AppendLine("                If UCase(sParametro) = UCase(strParametro) Then");
                        codigo.AppendLine("                    bContinua = False");
                        codigo.AppendLine("                    iPosicionIni = iPosicionFin + 2");
                        codigo.AppendLine("                    sValor = Mid(sValor, iPosicionIni)");
                        codigo.AppendLine("                     iPosicionFin = sValor.IndexOf(\"?\")");
                        codigo.AppendLine("                    If iPosicionFin >= 0 Then");
                        codigo.AppendLine("                        sValor = Mid(sValor, 1, iPosicionFin)");
                        codigo.AppendLine("                    End If");
                        codigo.AppendLine("                Else");
                        codigo.AppendLine("                    sValor = Mid(sValor, iPosicionFin + 2)");
                        codigo.AppendLine("                     iPosicionIni = sValor.IndexOf(\"?\")");
                        codigo.AppendLine("                     iPosicionFin = sValor.IndexOf(\"=\")");
                        codigo.AppendLine("                    If iPosicionIni >= 0 And iPosicionFin >= 0 Then");
                        codigo.AppendLine("                        sValor = Mid(sValor, iPosicionIni + 1)");
                        codigo.AppendLine("                    Else");
                        codigo.AppendLine("                        bContinua = False");
                        codigo.AppendLine("                        sValor = \"\"");
                        codigo.AppendLine("                    End If");
                        codigo.AppendLine("                End If");
                        codigo.AppendLine("            End If");
                        codigo.AppendLine("        End While");
                        codigo.AppendLine("        Return sValor");
                        codigo.AppendLine("    End Function");
                        codigo.AppendLine("\tFunction EjecutaNetScript(ByVal Procedimiento As String, ByVal Referencias As String, ByVal Uid As String, ByVal Estacion As String, ByVal CompileOnMemory As Boolean, ByVal MyParameters As String, Optional ByVal NetObject As Object = Nothing) As String");
                        codigo.AppendLine("        Try");
                        codigo.AppendLine("            Dim oSQLCld As SQLCloud.SQLCloudTools = New SQLCloud.SQLCloudTools");
                        codigo.AppendLine("\t\t\tDim dtFormatos As DataTable");
                        codigo.AppendLine("\t\t\tdtFormatos = GlobalFunctions.SQLTable(\"SELECT codigo, usufecha FROM formatosdelta WHERE formato = '\" & Procedimiento & \"'\", Me.ConnectionString)");
                        codigo.AppendLine("\t\t\tIf dtFormatos Is Nothing Or dtFormatos.Rows.Count = 0 Then");
                        codigo.AppendLine("        \t\tReturn \"\"\t");
                        codigo.AppendLine("\t\t\tEnd If");
                        codigo.AppendLine("\t\t\toSQLCld.RunAssembly(dtFormatos.Rows(0)(\"codigo\").ToString(), Procedimiento, dtFormatos.Rows(0)(\"usufecha\").ToString(), Referencias, Uid, Estacion, AppPath, Me.ConnectionString, CompileOnMemory, MyParameters)");
                        codigo.AppendLine("\t\t\tReturn oSQLCld.getReturnValue");
                        codigo.AppendLine("        Catch ex As Exception");
                        codigo.AppendLine("            Return \"Error: \" & ex.Message & vbCrLf & vbCrlf & \"Verifique que cuenta con la versión correcta de SQLCloud\"");
                        codigo.AppendLine("        End Try");
                        codigo.AppendLine("    End Function");
                        codigo.AppendLine("End Class");

                        con.EXEC
                        (@"INSERT INTO formatosdelta (catalogo, codigo,  descrip, formato, grupo, observ,  tipo,  proyecto,  usuario,  usufecha,  usuhora,  referencias) VALUES (@CATALOGO,@CODIGO,@DESCRIP,@FORMATO,@GRUPO,@OBSERV,@TIPO,@PROYECTO,@USUARIO,@USUFECHA,@USUHORA,@REFERENCIAS)",

                            new System.Data.SqlClient.SqlParameter("CATALOGO", "RESTAURANTE"),
                            new System.Data.SqlClient.SqlParameter("DESCRIP", "MGP_COBRONUEVO"),
                            new System.Data.SqlClient.SqlParameter("FORMATO", "MGP_COBRONUEVO"),
                            new System.Data.SqlClient.SqlParameter("GRUPO", "RESTAURANTE"),
                            new System.Data.SqlClient.SqlParameter("OBSERV", "MYGOURMETPOS"),
                            new System.Data.SqlClient.SqlParameter("TIPO", "Programa .NET"),
                            new System.Data.SqlClient.SqlParameter("PROYECTO", "RESTAURANTE"),
                            new System.Data.SqlClient.SqlParameter("USUARIO", "SUP"),
                            new System.Data.SqlClient.SqlParameter("USUFECHA", DateTime.Today),
                            new System.Data.SqlClient.SqlParameter("USUHORA", DateTime.Now.ToString("HH:mm:ss")),
                            new System.Data.SqlClient.SqlParameter("REFERENCIAS", @"{app}\sqlcloud\system.diagnostics.debug.dll;{app}\sqlcloud\sqlcloud.dll;"),
                            new System.Data.SqlClient.SqlParameter("CODIGO", codigo.ToString())
                        );
                    }
                    if (!con.Exists("SELECT *FROM formatosdelta WHERE formato = 'PUNTOV080' and codigo like '%MGP_COBRONUEVO%'"))
                    {
                        StringBuilder codigo = new StringBuilder();
                        codigo.AppendLine("Sub Main() ");
                        codigo.AppendLine("    Me.usuarioRequerido = 0");
                        codigo.AppendLine("\tCancelaProceso = True\t");
                        codigo.AppendLine("\t\tIf Ambiente.Tag = 116 Then  ");
                        codigo.AppendLine("\tIf Venta>0 Then ");
                        codigo.AppendLine("\t\t\tScript.RunNetScript \"MGP_COBRONUEVO\", \"\", Ambiente,(\"?Usuario=\" & Ambiente.Uid & \"?Estacion=\" & Ambiente.Estacion&\"?Venta=\"&Venta\"?Impresora=\" & Ambiente.Impresora)");
                        codigo.AppendLine("\t\t\t'MyMessage(Ambiente.Tag&\" - VENTA [\"&Venta&\"]\")      ");
                        codigo.AppendLine("\t\tEnd if");
                        codigo.AppendLine("    End If  ");
                        codigo.AppendLine("    If Parent.KeyControl = 1 Then");
                        codigo.AppendLine("       Exit Sub");
                        codigo.AppendLine("    End If ");
                        codigo.AppendLine("\tIf ArticuloTiempoAireOEspecial = False Then");
                        codigo.AppendLine("    \tIf Ambiente.Tag = 118 Then");
                        codigo.AppendLine("       \t\tIf Question(\"Esta seguro de querer dejar esta venta como pendiente\") Then");
                        codigo.AppendLine("          \t\tMe.BorraVenta = True");
                        codigo.AppendLine("          \t\tMe.FinalizaOperacion        ");
                        codigo.AppendLine("          \t\tExit Sub");
                        codigo.AppendLine("       \t\tEnd If");
                        codigo.AppendLine("\t\tEnd If");
                        codigo.AppendLine("\tElse");
                        codigo.AppendLine("\t\tIf Ambiente.Tag = 119 Then");
                        codigo.AppendLine("\t\t\tMe.CancelaProceso = True");
                        codigo.AppendLine("    \tEnd If      ");
                        codigo.AppendLine("\tEnd If");
                        codigo.AppendLine("End Sub     ");
                        codigo.AppendLine("Function ArticuloTiempoAireOEspecial()");
                        codigo.AppendLine("\tDim rstPV");
                        codigo.AppendLine("	\tSet rstPV = Rst(\"SELECT articulo FROM partvta WHERE venta = \" & Me.Venta & \" AND (articulo = 'RMOVISTAR' OR \" & _");
                        codigo.AppendLine("\t\t\t\t\t\"articulo = 'RTELCEL' OR articulo = 'RIUSACELL' OR articulo = 'RUNEFON' OR articulo = 'RNEXTEL' OR \" & _");
                        codigo.AppendLine("\t\t\t\t\t\"articulo = 'RSATFEMYB' OR articulo = 'RSERVICIOS' OR articulo = 'RVIRGIN' OR ARTICULO = 'RTELCELINT' OR \" & _");
                        codigo.AppendLine("\t\t\t\t\t\"articulo = 'RTELCELPAQ' OR articulo = 'RALO')\", Ambiente.Connection)");
                        codigo.AppendLine("\tIf Not rstPV.EOF Then");
                        codigo.AppendLine("    \tArticuloTiempoAireOEspecial = True");
                        codigo.AppendLine("\tElse");
                        codigo.AppendLine("\t\tArticuloTiempoAireOEspecial = False");
                        codigo.AppendLine("\tEnd If");
                        codigo.AppendLine("\trstPV.Close");
                        codigo.AppendLine("\tSet rstPV = Nothing\t");
                        codigo.AppendLine("End Function");

                        con.EXEC
                        (@"UPDATE formatosdelta SET CODIGO=@CODIGO WHERE formato = 'PUNTOV080'",

                            new System.Data.SqlClient.SqlParameter("CODIGO", codigo.ToString())
                        );
                    }

                    if (con.Exists(
                        "SELECT * FROM formatosdelta WHERE formato = 'SERVICIO' AND codigo LIKE '%Imports%'"))
                    {
                        StringBuilder codigo = new StringBuilder();
                        codigo.AppendLine("Public Class MainClass");
                        codigo.AppendLine("\tPublic Sub Main");
                        codigo.AppendLine("End Sub");
                        codigo.AppendLine("End Class");
                        con.EXEC
                        (@"UPDATE formatosdelta SET CODIGO=@CODIGO WHERE formato = 'SERVICIO' AND codigo LIKE '%Imports%'",

                            new System.Data.SqlClient.SqlParameter("CODIGO", codigo.ToString())
                        );
                    }
                }
                else
                {
                    throw new NotImplementedException("Version!=2017");
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al establecer los scripts en la barra de tareas");
            }
        }
        public Configuracion DatosCadenaCon(string empresa)
        {
            Configuracion cadena = null;
            using (SQLiteConnection conectado = this.Con())
            {
                using (IReader reader = conectado.Read($"SELECT cadenadeconexion FROM configuracion where nombre='{empresa}'"))
                {
                    if (reader.Read())
                    {
                        cadena = Configuracion.FromMyB(reader[0].ToString());
                        cadena.Empresa = empresa;
                        cadena.IdentificadorDispositivo = Device.Current.DeviceId;
                        cadena.Activa = true;
                    }

                }
                conectado.Close();
            }
            return cadena;

        }
        public List<string> Empresas()
        {
            List<string> empresas = new List<string>();
            using (SQLiteConnection conectado = this.Con())
            {
                using (IReader reader = conectado.Read($"SELECT Nombre FROM configuracion"))
                {
                    while (reader.Read())
                    {
                        empresas.Add(reader[0].ToString());
                    }
                }
                conectado.Close();
            }

            return empresas;
        }
    }
}
