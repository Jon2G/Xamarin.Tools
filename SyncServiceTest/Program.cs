using SyncService.Daemon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit.Daemon;
using Kit.Daemon.Abstractions;
using Kit.WPF.Services;

namespace SyncServiceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            SQLHelper.SQLHelper.Init(Environment.CurrentDirectory, Debugger.IsAttached);
         //   Plugin.Xamarin.Tools.WPF.Tools.Init().SetLibraryPath(Environment.CurrentDirectory).InitAll($"{Environment.CurrentDirectory}\\Log", false);

            SQLHelper.SQLHelper.Init(Environment.CurrentDirectory, Debugger.IsAttached);
            ConnectionsManager connections =
            ConnectionsManager.Init()
                .AddConnection(Daemon.OriginName, new SQLHelper.SQLH(""))
                .AddConnection(Daemon.DestinationName, new SQLHelper.SQLHLite(new FileInfo("a.db")));

            Daemon.Init(new Tools.WPF.Services.DeviceInfo(),"1.0.0")
                  .SetSchema(
                new Table("ESTACIONES", "ESTACION", "MONEDA", "ALMACEN", "VENTA", "IMPFACT", "IMPREM"
                , "TANTOSREM", "IMPDEV", "PTICKET", "IMPSALIDAS", "TIPO_ESTACION", "COCINA_IMPRIME_COMANDAS"
                , "COCINA", "VISTA_COCINA", "ESTILO_TICKET", "COMANDERA_IMPRIME_COCINA", "COBRO_EN_COMANDERA"
                , "ESTILO_TICKET_COMANDERA", "TICKET_PARA_COBRO_CAJA", "MESERO_LIMPIA_MESA", "NO_LOGIN"
                , "NO_AUTORIZACION", "NO_GARROTERO", "PROPINA_1", "PROPINA_2", "PROPINA_3", "PROPINA_4"
                , "CAJA_COMANDERA", "VECES_TICKET_CAJA", "VECES_PRETICKET", "PRODUCTO_AUXILIAR", "NO_ARQUEO"
                , "MOSTRAR_VENTAS_X_ARTICULO_CORTEX", "PUEDE_HACER_CORTE", "USA_MONDEDA_EXTRANJERA"
                , "VENDER_SIN_EXISTENCIA", "NUMERO_TERMINAL", "CLAVE_TERMINAL", "URL_PROSEPAGO_ENVIO"
                , "URL_PROSEPAGO_RESPUESTA", "TERMINAL_HABILITADA", "SERIE_COMPROBANTE_PAGO ", "SERIE_NOTA_CREDITO"
                , "SERIE_FACTURA", "FORMATO_FACTURA", "TICKETS_REMOTOS", "DESGLOSAR_EXTRAS_TK"),
                new Table("USUARIOS", "USUARIO", "CLAVE", "NOMBRE", "AREA", "ACTIVO", "TIPO_USUARIO", "SUPERVISOR_DIRECTO"),
                new Table("USUARIOSHUELLA", "USUARIO", "huella", "DEDO"),
                new Table("SECCIONES", "ID", "SECCION", "PARENT_HEIGHT", "PARENT_WIDTH"),
                new Table("ELEMENTOS_SECCION", "ID", "ID_SECCION", "X", "Y", "FORMA", "HEIGHT", "WIDTH", "ID_MESA", "ID_VISTA_MESA", "IMAGEN", "ZINDEX"),
                new Table("PRODS", "ARTICULO", "DESCRIP", "LINEA", "FAMILIA", "MARCA", "PRECIO1", "UNIDAD", "IMPUESTO",
                "OBSERV", "PARAVENTA", "PREPARA_EN", "TIPO_PRODUCTO", "INICIO", "FIN", "TIEMPO_PREPARACION", "COMBO", "EXTRA_PERSONALIZADO",
                "PORCION_PERSONALIZADO", "HABILITADO_PARA_PERSONALIZADO", "COLOR", "PORCIONES_PERSONALIZADO", "PARA_LLEVAR", "COSTO_U",
                "MERMA", "MAXIMO", "MINIMO"),
                new Table("UNIONES", "IDENTIDAD", "ID", "MESA_ID"),
                new Table("NIVELES", "ID_NIVEL", "NOMBRE", "PADRE", "COLOR"),
                new Table("IMPUESTOS", "Impuesto", "Descrip", "Valor"),
                new Table("SERVICIOS_PARA_LLEVAR", "ID", "DESCRIP", "OBSERVACIONES", "LOGOTIPO", "AUTO_CONCEPTO", "USAR_LISTA_PRECIOS"),
                new Table("TAMANIOS", "ID", "ID_PLATILLO", "NOMBRE", "PREPARA_EN", "TIEMPO_PREPARACION", "ES_BASE", "PRECIO", "EXTRA", "TIENE_OPCIONES", "ES_COMPUESTO"),
                new Table("COMPONENTES", "ID", "ID_PLATILLO", "ID_TAMANIO", "ID_COMPONENTE", "CANTIDAD", "ES_OPCIONAL", "ID_MODIFICADOR"),
                new Table("COMPONENTES_OPCIONES", "ID", "ID_PLATILLO", "ID_TAMANIO", "ID_OPCION", "CANTIDAD"),
                new Table("ESTANDARES", "ID", "ID_PLATILLO", "ID_TAMANIO"),
                new Table("OPCIONES_ESTANDAR", "ID", "ID_ESTANDAR", "ID_PLATILLO", "ID_OPCION", "ID_COMPONENTE_OPCION"),
                new Table("CABECERA_OPCIONES", "ID", "TITULO"),
                new Table("COMPONENTES_OPCION", "ID", "ID_OPCION", "ID_INGREDIENTE", "ES_COMPUESTA", "CANTIDAD", "TITULO", "COSTO_EXTRA"),
                new Table("RECETA_OPCION", "ID", "ID_OPCION", "ID_INGREDIENTE", "ID_COMPONENTE_OPCION", "CANTIDAD"),
                new Table("MODIFICADORES", "ID", "PLATILLO_BASE", "INGREDIENTE_MODIFICADO"),
                new Table("INGREDIENTES_MODIFICADORES", "ID", "ID_MODIFICADOR", "ARTICULO", "CANTIDAD", "EXTRA"),
                new Table("CONCXC", "CLAVE_CON", "DESCRIP", "HABILITADO", "TIPO_CXC"),
                new Table("VENDS", "VEND", "NOMBRE", "COMISION", "USUARIO", "USUFECHA", "USUHORA", "ACTIVO", "FORANEO"),
                new Table("AREAS_MESEROS", "ID", "USUARIO", "ID_SECCION", "TODAS_LAS_MESAS"),
                new Table("MESAS_ASIGNADAS", "ID", "USUARIO", "ID_MESA", "ID_SECCION"),
                new Table("GRADIENTES", "ID", "GRADIENTE", "COLOR", "COLOR_OSCURO", "OFFSET", "RADIAL"),

                new Table("R_MESAS", "ID", "estado", "comensales", "mesero", "impresora", "operadaPorComandera", "activa", "HABILITADA", "INICIO", "UNIONMESA", "ID_SERVICIO", "ID_DIVISION", "ID_DIVISION_VISTA")
                .SetCustomUploadAction(UploadMesas),

                new Table("R_COMANDAS", "ID", "mesa", "articulo", "comensal", "precio", "confirmado", "encaja", "eliminado", "HORA", "NOTAS", "ESTADO", "CANTIDAD", "IMPRESO", "MESERO", "FECHA", "GUID", "BIS_MESA")
                .ReserveNewId(true)
                .Affects("TAMANIOS_COMANDAS", "ID_COMANDA")
                .Affects("OPCIONES_COMANDAS", "ID_COMANDA")
                .Affects("MODIFICADORES_COMANDAS", "ID_COMANDA")
                .Affects("PERSONALIZADOS_COMANDAS", "ID_COMANDA")
                .Affects("COMANDAS_PART", "ID_COMANDA")
                ,
                new Table("TAMANIOS_COMANDAS", "ID", "ID_COMANDA", "ID_PLATILLO", "ID_TAMANIO")
                .ReserveNewId(true)
                .Affects("OPCIONES_COMANDAS", "ID_TAMANIOS_COMANDAS")
                .Affects("MODIFICADORES_COMANDAS", "ID_TAMANIOS_COMANDAS"),

                new Table("OPCIONES_COMANDAS", "ID", "ID_PLATILLO", "ID_COMANDA", "ID_TAMANIOS_COMANDAS", "ID_OPCION", "ID_COMPONENTES_OPCION")
                .ReserveNewId(true),
                new Table("MODIFICADORES_COMANDAS", "ID", "ID_COMANDA", "ID_TAMANIOS_COMANDAS", "ID_MOD", "INGREDIENTE_MODIFICADO", "ES_OPCIONAL", "ID_REEMPLAZO")
                .ReserveNewId(true),
                new Table("PERSONALIZADOS_COMANDAS", "ID", "ID_COMANDA", "ID_INGREDIENTE_PERSONALIZADO", "CANTIDAD_PERSONALIZADA", "PORCIONES")
                .ReserveNewId(true),
                new Table("COMANDAS_PART", "ID_COMPART", "ID_COMPART", "ID_COMANDA", "ID_PLATILLO", "ID_OPCIONES_SELECCIONADAS", "ID_OPCION", "ATENDIDO", "GUID", "CANTIDAD_PERSONALIZADA")
                .ReserveNewId(true)
                );
            Daemon.Current.Awake();

            Console.ReadKey();
        }

        private static bool UploadMesas(Table arg1, Pendientes arg2, ValoresOriginales arg3)
        {
            throw new NotImplementedException();
        }
    }
}
