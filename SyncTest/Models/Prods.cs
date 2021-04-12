using System;
using System.Collections.Generic;
using System.Text;
using Kit.Daemon.Sync;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;

namespace SyncTest.Models
{
    [StoreAsText]
    public enum TipoProducto
    {
        Platillo,
        Ingrediente,
        ProductoTerminado,
        Promocion
    }
    [SyncMode(SyncDirection.Download)]
    public class Prods : ISync
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [MaxLength(30)]
        public string Articulo { get; set; }
        public string Descrip { get; set; }
        public string Linea { get; set; }
        public string Familia { get; set; }
        public string Marca { get; set; }
        [Column("Precio1")]
        public string Precio { get; set; }
        public string Unidad { get; set; }
        public string Impuesto { get; set; }
        [Column("Observ")]
        public string Observaciones { get; set; }
        public bool ParaVenta { get; set; }
        public string PreparaEn { get; set; }
        [Column("TIPO_PRODUCTO")]
        public string TipoProducto { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        [Column("TIEMPO_PREPARACION")]
        public float TiempoPreparacion { get; set; }
        public bool Combo { get; set; }
        [Column("EXTRA_PERSONALIZADO")]
        public bool ExtraPersonalizado { get; set; }
        [Column("PORCION_PERSONALIZADO")]
        public bool PorcionPersonalizado { get; set; }
        [Column("HABILITADO_PARA_PERSONALIZADO")]
        public bool HabilitadoPersonalizado { get; set; }
        public string Color { get; set; }
        [Column("PORCIONES_PERSONALIZADO")]
        public float PorcionesPersonalizado { get; set; }
        [Column("PARA_LLEVAR")]
        public bool ParaLlevar { get; set; }
        [Column("COSTO_U")]
        public float Costo { get; set; }
        public float Merma { get; set; }
        public float Maximo { get; set; }
        public float Minimo { get; set; }
        public Prods() { }
    }
}
