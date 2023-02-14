namespace APISICA.Class
{
    public class JsonBody
    {
        public int idaux { get; set; } = -1;
        public int idestado { get; set; } = -1;
        public int idarea { get; set; } = -1;
        public int idinventario { get; set; } = -1;
        public int iddepartamento { get; set; } = -1;
        public int iddocumento { get; set; } = -1;
        public int iddetalle { get; set; } = -1;
        public int idcentrocosto { get; set; } = -1;
        public int idclasificacion { get; set; } = -1;
        public int idproducto { get; set; } = -1;
        public int idubicacion { get; set; } = -1;
        public string numerocaja { get; set; } = string.Empty;
        public string busquedalibre { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;
        public string numerosolicitud { get; set; } = string.Empty;
        public string codigosocio { get; set; } = string.Empty;
        public string nombresocio { get; set; } = string.Empty;
        public string fechadesde { get; set; } = string.Empty;
        public string fechahasta { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public int anulado { get; set; } = -1;
        public string strdepartamento { get; set; } = string.Empty;
        public string strdocumento { get; set; } = string.Empty;
        public string strdetalle { get; set; } = string.Empty;
        public string strclasificacion { get; set; } = string.Empty;
        public string strproducto { get; set; } = string.Empty;
        public string strcentrocosto { get; set; } = string.Empty;
        public int idubicacionentrega { get; set; } = -1;
        public int idubicacionrecibe { get; set; } = -1;
        public bool pendienteok { get; set; } = false;
        public bool modificado { get; set; } = false;
        public string nombreusuario { get; set; } = string.Empty;
        public string correousuario { get; set; } = string.Empty;
        public int notificar { get; set; } = -1;
        public int ordendif { get; set; } = 0;
    }
}
