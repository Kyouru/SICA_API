namespace APISICA.Class
{
    public class GuardarClass
    {
        public int idinventario { get; set; } = -1;
        public int iddepartamento { get; set; } = -1;
        public int iddocumento { get; set; } = -1;
        public int iddetalle { get; set; } = -1;
        public int idcentrocosto { get; set; } = -1;
        public int idclasificacion { get; set; } = -1;
        public int idproducto { get; set; } = -1;
        public string numerocaja { get; set; } = string.Empty;
        public string numerosolicitud { get; set; } = string.Empty;
        public string codigosocio { get; set; } = string.Empty;
        public string nombresocio { get; set; } = string.Empty;
        public string fechadesde { get; set; } = string.Empty;
        public string fechahasta { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public string pendiente { get; set; } = string.Empty;
        public string detallepen { get; set; } = string.Empty;
        public string banca { get; set; } = string.Empty;
        //Para Pendiente
        public bool modificado { get; set; } = false;
        public bool pendienteok { get; set; } = false;
        public int idubicacionrecibe { get; set; } = -1;
    }
}
