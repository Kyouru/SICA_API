namespace APISICA.Class
{
    public class RecibirAgregarClass
    {
        public int idusuarioentrega { get; set; } = -1;
        public int idubicacionentrega { get; set; } = -1;
        //public int idubicacionrecibe { get; set; } = -1;
        public string numerocaja { get; set; } = string.Empty;
        public int iddepartamento { get; set; } = -1;
        public int iddocumento { get; set; } = -1;
        public int iddetalle { get; set; } = -1;
        public int idcentrocosto { get; set; } = -1;
        public int idclasificacion { get; set; } = -1;
        public int idproducto { get; set; } = -1;
        public string codigosocio { get; set; } = string.Empty;
        public string nombresocio { get; set; } = string.Empty;
        public string numerosolicitud { get; set; } = string.Empty;
        public string fechadesde { get; set; } = string.Empty;
        public string fechahasta { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;
    }
}
