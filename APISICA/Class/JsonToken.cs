namespace APISICA.Class
{
    public class JsonToken
    {
        public string token { get; set; } = string.Empty;
        public int idaux { get; set; } = -1;
        public int idinventario { get; set; } = -1;
        public int iddepartamento { get; set; } = -1;
        public int iddocumento { get; set; } = -1;
        public int iddetalle { get; set; } = -1;
        public int idcentrocosto { get; set; } = -1;
        public int idclasificacion { get; set; } = -1;
        public int idproducto { get; set; } = -1;
        public int idubicacion { get; set; } = -1;
        public string tipocarrito { get; set; } = string.Empty;
        public string numerocaja { get; set; } = string.Empty;
        public string busquedalibre { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;
        public string numerosolicitud { get; set; } = string.Empty;
        public string codigosocio { get; set; } = string.Empty;
        public string nombresocio { get; set; } = string.Empty;
        public string descripcion1 { get; set; } = string.Empty;
        public string descripcion2 { get; set; } = string.Empty;
        public string descripcion3 { get; set; } = string.Empty;
        public string descripcion4 { get; set; } = string.Empty;
        public string descripcion5 { get; set; } = string.Empty;
        public string nombredepartamento { get; set; } = string.Empty;
        public string nombredocumento { get; set; } = string.Empty;
        public string fechadesde { get; set; } = string.Empty;
        public string fechahasta { get; set; } = string.Empty;
        public string fechamodifica { get; set; } = string.Empty;
        public int expediente { get; set; } = -1;
        public string observacion { get; set; } = string.Empty;
        public bool confirmar { get; set; } = false;
        public int idareaentrega { get; set; } = -1;
        public int idarearecibe { get; set; } = -1;
        public int pagare { get; set; } = -1;
        public string nomdocumento { get; set; } = string.Empty;
        public string nomdepartamento { get; set; } = string.Empty;
        public string concat { get; set; } = string.Empty;
        public int entrega { get; set; } = -1;
        public int tiposeleccionarusuario { get; set; } = -1;
        public int entransito { get; set; } = -1;

        public bool anulado = false;
    }
}
