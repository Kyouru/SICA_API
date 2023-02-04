namespace APISICA.Class
{
    public class UserData
    {
        public string Username { get; set; } = String.Empty;
        public int IdUser { get; set; } = 0;
        public int CambiarPassword { get; set; } = 0;
        public int AccesoPermitido { get; set; } = 0;
        public int CerrarSesion { get; set; } = 0;
        public int Busqueda { get; set; } = 0;
        public int BusquedaHistorico { get; set; } = 0;
        public int BusquedaEditar { get; set; } = 0;
        public int Mover { get; set; } = 0;
        public int MoverExpediente { get; set; } = 0;
        public int MoverDocumento { get; set; } = 0;
        public int MoverMasivo { get; set; } = 0;
        public int Valija { get; set; } = 0;
        public int ValijaNuevo { get; set; } = 0;
        public int ValijaValija { get; set; } = 0;
        public int ValijaTransicion { get; set; } = 0;
        public int Pagare { get; set; } = 0;
        public int PagareBuscar { get; set; } = 0;
        public int PagareRecibir { get; set; } = 0;
        public int PagareEntregar { get; set; } = 0;
        public int Letra { get; set; } = 0;
        public int LetraNuevo { get; set; } = 0;
        public int LetraEntregar { get; set; } = 0;
        public int LetraReingreso { get; set; } = 0;
        public int LetraBuscar { get; set; } = 0;
        public int Mantenimiento { get; set; } = 0;
        public int MantenimientoUsuarioExterno { get; set; } = 0;
        public int MantenimientoSocio { get; set; } = 0;
        public int MantenimientoCredito { get; set; } = 0;
        public int MantenimientoListas { get; set; } = 0;
        public int Pendiente { get; set; } = 0;
        public int PendienteRegularizar { get; set; } = 0;
        public int Reporte { get; set; } = 0;
        public int ReporteCajas { get; set; } = 0;
        public int Prestar { get; set; } = 0;
        public int PrestarPrestar { get; set; } = 0;
        public int PrestarRecibir { get; set; } = 0;
        public string Token { get; set; } = String.Empty;
    }
}
