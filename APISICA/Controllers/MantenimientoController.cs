using APISICA.Class;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;

namespace APISICA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MantenimientoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public MantenimientoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost("buscarcuenta")]
        public IActionResult ListaCuentas(Class.JsonToken jsontoken)
        {
            DataTable dt;
            Cuenta cuenta;
            try
            {
                cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), jsontoken.token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (!(cuenta.IdUser > 0))
            {
                return Unauthorized("Sesion no encontrada");
            }

            string strSQL = "SELECT U.ID_USUARIO, U.NOMBRE_USUARIO, U.EMAIL, U.ANULADO AS DESHABILITADO FROM ADMIN.USUARIO U ORDER BY ORDEN DESC";

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();
                dt = conn.llenarDataTable();
                conn.cerrar();

                string json = JsonConvert.SerializeObject(dt);
                return Ok(json);
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("crearusuario")]
        public IActionResult CrearUsuario(Class.JsonToken jsontoken)
        {
            Cuenta cuenta;
            string connstr = "";
            try
            {
                cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), jsontoken.token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (!(cuenta.IdUser > 0))
            {
                return Unauthorized("Sesion no encontrada");
            }

            if (jsontoken.idaux == Int32.Parse(_configuration.GetSection("Area:Custodia").Value))
            {
                connstr = "Custodia1";
            }
            else
            {
                connstr = "Default1";
            }

            string strSQL = "SELECT COUNT(*) FROM ADMIN.USUARIO WHERE NOMBRE_USUARIO = '" + jsontoken.descripcion1 + "'";

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                int cont = conn.ejecutarQueryEscalar();
                if (cont == 0)
                {
                    strSQL = "SELECT MAX(ORDEN) + 1 FROM USUARIO WHERE ID_AREA_FK = " + jsontoken.idaux;
                    conn.iniciaCommand(strSQL);
                    int orden = conn.ejecutarQueryEscalar();

                    strSQL = "INSERT INTO ADMIN.USUARIO (NOMBRE_USUARIO, ID_AREA_FK, REAL, CAMBIAR_PASSWORD, ORDEN, CERRAR_SESION, DATAMANAGER, PASSWORDHASH, PASSWORDSALT, EMAIL, CONNUSER, ANULADO, ACCESO_PERMITIDO)";
                    strSQL += " VALUES ('" + jsontoken.descripcion1 + "', " + jsontoken.idaux + ", 1, 1, " + orden + ", 0, 0, '" + _configuration.GetSection("DefaultPassword:hash").Value + "', '" + _configuration.GetSection("DefaultPassword:salt").Value + "', '" + jsontoken.descripcion2 + "', '" + connstr + "', 0, 0)";
                    strSQL += " RETURNING ID_USUARIO INTO :numero";
                    int id = conn.InsertReturnID(strSQL);
                    if (id <= 0)
                    {
                        return BadRequest("Error Creando Usuario");
                    }
                    strSQL = "INSERT INTO ADMIN.PERMISO (ID_USUARIO_FK, BUSQUEDA, BUSQUEDA_HISTORICO, BUSQUEDA_EDITAR, ENTREGAR, ENTREGAR_EXPEDIENTE, ENTREGAR_DOCUMENTO, RECIBIR, RECIBIR_NUEVO, RECIBIR_REINGRESO, RECIBIR_CONFIRMAR, RECIBIR_MANUAL, PAGARE, PAGARE_BUSCAR, PAGARE_RECIBIR, PAGARE_ENTREGAR, LETRA, LETRA_NUEVO, LETRA_ENTREGAR, LETRA_REINGRESO, LETRA_BUSCAR, IRONMOUNTAIN, IRONMOUNTAIN_SOLICITAR, IRONMOUNTAIN_RECIBIR, IRONMOUNTAIN_ARMAR, IRONMOUNTAIN_ENVIAR, IRONMOUNTAIN_ENTREGAR, IRONMOUNTAIN_CARGO, BOVEDA, BOVEDA_CAJA_RETIRAR, BOVEDA_CAJA_GUARDAR, BOVEDA_DOCUMENTO_RETIRAR, BOVEDA_DOCUMENTO_GUARDAR, MANTENIMIENTO, MANTENIMIENTO_CUENTA, MANTENIMIENTO_CREDITO, MANTENIMIENTO_SOCIO, IMPORTAR, IMPORTAR_ACTIVAS, IMPORTAR_PASIVAS, NIVEL)";
                    if (jsontoken.idaux == Int32.Parse(_configuration.GetSection("Area:Custodia").Value))
                    {
                        //ID, BUSQUEDA, BUSQUEDA_HISTORICO, BUSQUEDA_EDITAR
                        strSQL += " VALUES (" + id + ", 1, 1, 1,";
                        //ENTREGAR, ENTREGAR_EXPEDIENTE, ENTREGAR_DOCUMENTO
                        strSQL += " 1, 1, 1,";
                        //RECIBIR, RECIBIR_NUEVO, RECIBIR_REINGRESO, RECIBIR_CONFIRMAR, RECIBIR_MANUAL
                        strSQL += " 1, 1, 1, 1, 1,";
                        //PAGARE, PAGARE_BUSCAR, PAGARE_RECIBIR, PAGARE_ENTREGAR
                        strSQL += " 1, 1, 1, 1,";
                        //LETRA, LETRA_NUEVO, LETRA_ENTREGAR, LETRA_REINGRESO, LETRA_BUSCAR
                        strSQL += " 0, 0, 0, 0, 0,";
                        //IRONMOUNTAIN, IRONMOUNTAIN_SOLICITAR, IRONMOUNTAIN_RECIBIR, IRONMOUNTAIN_ARMAR, IRONMOUNTAIN_ENVIAR, IRONMOUNTAIN_ENTREGAR, IRONMOUNTAIN_CARGO
                        strSQL += " 0, 0, 0, 0, 0, 0, 0,";
                        //BOVEDA, BOVEDA_CAJA_RETIRAR, BOVEDA_CAJA_GUARDAR, BOVEDA_DOCUMENTO_RETIRAR, BOVEDA_DOCUMENTO_GUARDAR
                        strSQL += " 1, 1, 1, 1, 1,";
                        //MANTENIMIENTO, MANTENIMIENTO_CUENTA, MANTENIMIENTO_CREDITO, MANTENIMIENTO_SOCIO
                        strSQL += " 1, 1, 1, 1,";
                        //IMPORTAR, IMPORTAR_ACTIVAS, IMPORTAR_PASIVAS, NIVEL
                        strSQL += " 0, 0, 0, 2)";
                    }
                    else
                    {
                        //ID, BUSQUEDA, BUSQUEDA_HISTORICO, BUSQUEDA_EDITAR
                        strSQL += " VALUES (" + id + ", 1, 1, 0,";
                        //ENTREGAR, ENTREGAR_EXPEDIENTE, ENTREGAR_DOCUMENTO
                        strSQL += " 1, 1, 1,";
                        //RECIBIR, RECIBIR_NUEVO, RECIBIR_REINGRESO, RECIBIR_CONFIRMAR, RECIBIR_MANUAL
                        strSQL += " 1, 0, 0, 1, 0,";
                        //PAGARE, PAGARE_BUSCAR, PAGARE_RECIBIR, PAGARE_ENTREGAR
                        strSQL += " 1, 1, 0, 0,";
                        //LETRA, LETRA_NUEVO, LETRA_ENTREGAR, LETRA_REINGRESO, LETRA_BUSCAR
                        strSQL += " 0, 0, 0, 0, 0,";
                        //IRONMOUNTAIN, IRONMOUNTAIN_SOLICITAR, IRONMOUNTAIN_RECIBIR, IRONMOUNTAIN_ARMAR, IRONMOUNTAIN_ENVIAR, IRONMOUNTAIN_ENTREGAR, IRONMOUNTAIN_CARGO
                        strSQL += " 0, 0, 0, 0, 0, 0, 0,";
                        //BOVEDA, BOVEDA_CAJA_RETIRAR, BOVEDA_CAJA_GUARDAR, BOVEDA_DOCUMENTO_RETIRAR, BOVEDA_DOCUMENTO_GUARDAR
                        strSQL += " 0, 0, 0, 0, 0,";
                        //MANTENIMIENTO, MANTENIMIENTO_CUENTA, MANTENIMIENTO_CREDITO, MANTENIMIENTO_SOCIO
                        strSQL += " 0, 0, 0, 0,";
                        //IMPORTAR, IMPORTAR_ACTIVAS, IMPORTAR_PASIVAS, NIVEL
                        strSQL += " 0, 0, 0, 2)";
                    }
                    conn.ejecutarQuery();
                    conn.cerrar();

                    return Ok();
                }
                else
                {
                    conn.cerrar();
                    return BadRequest("Usuario Duplicado");
                }
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("datousuario")]
        public IActionResult DatoCuenta(Class.JsonToken jsontoken)
        {
            DataTable dt;
            Cuenta cuenta;
            try
            {
                cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), jsontoken.token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (!(cuenta.IdUser > 0))
            {
                return Unauthorized("Sesion no encontrada");
            }

            string strSQL = "SELECT * FROM ADMIN.USUARIO WHERE ID_USUARIO = " + jsontoken.idaux + "";

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();
                dt = conn.llenarDataTable();

                conn.cerrar();

                string json = JsonConvert.SerializeObject(dt);
                return Ok(json);
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }
    }
}
