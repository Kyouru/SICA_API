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
        public IActionResult ListaCuentas(Class.JsonBody jsonbody)
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                DataTable dt;
                Cuenta cuenta;
                try
                {
                    cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), bearerToken);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                if (!(cuenta.IdUser > 0))
                {
                    return Unauthorized("Sesion no encontrada");
                }

                string strSQL = "SELECT U.ID_USUARIO, U.NOMBRE_USUARIO, U.EMAIL, U.ANULADO AS DESHABILITADO FROM ADMIN.USUARIO U ORDER BY ORDEN ASC";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    dt = conn.LlenarDataTable(strSQL);
                    conn.Cerrar();

                    string json = JsonConvert.SerializeObject(dt);
                    return Ok(json);
                }
                catch (Exception ex)
                {
                    conn.Cerrar();
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return Unauthorized("No se recibió bearer token");
            }
            
        }


        [HttpPost("crearusuarioexterno")]
        public IActionResult CrearUsuarioExterno(Class.JsonBody jsonbody)
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                Cuenta cuenta;
                try
                {
                    cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), bearerToken);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                if (!(cuenta.IdUser > 0))
                {
                    return Unauthorized("Sesion no encontrada");
                }

                string strSQL = "SELECT COUNT(*) FROM ADMIN.USUARIO_EXTERNO WHERE NOMBRE_USUARIO_EXTERNO = '" + jsonbody.nombreusuario + "'";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    int cont = conn.EjecutarQueryEscalar(strSQL);
                    if (cont == 0)
                    {
                        strSQL = "SELECT MAX(ORDEN) + 1 FROM ADMIN.USUARIO_EXTERNO";
                        int orden = conn.EjecutarQueryEscalar(strSQL);

                        strSQL = "INSERT INTO ADMIN.USUARIO_EXTERNO (NOMBRE_USUARIO_EXTERNO, EMAIL, NOTIFICAR, ORDEN, ANULADO)";
                        strSQL += " VALUES ('" + jsonbody.nombreusuario + "', '" + jsonbody.correousuario + "', " + jsonbody.notificar + ", " + orden + ", 0)";
                        strSQL += " RETURNING ID_USUARIO_EXTERNO INTO :numero";
                        int id = conn.InsertReturnID(strSQL);

                        conn.Cerrar();

                        if (id <= 0)
                        {
                            return BadRequest("Error Creando Usuario" + strSQL);
                        }

                        return Ok();
                    }
                    else
                    {
                        conn.Cerrar();
                        return BadRequest("Usuario Duplicado" + strSQL);
                    }
                }
                catch (Exception ex)
                {
                    conn.Cerrar();
                    return BadRequest(ex.Message + strSQL);
                }
            }
            else
            {
                return Unauthorized("No se recibió bearer token");
            }
            
        }

        [HttpPost("modificarusuarioexterno")]
        public IActionResult ModificarUsuarioExterno(Class.JsonBody jsonbody)
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                Cuenta cuenta;
                try
                {
                    cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), bearerToken);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                if (!(cuenta.IdUser > 0))
                {
                    return Unauthorized("Sesion no encontrada");
                }

                string strSQL = "SELECT COUNT(*) FROM ADMIN.USUARIO_EXTERNO WHERE NOMBRE_USUARIO_EXTERNO = '" + jsonbody.nombreusuario + "'";

                Conexion conn = new Conexion();
                try
                {
                    //conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    /*conn.iniciaCommand(strSQL);
                    int cont = conn.ejecutarQueryEscalar();
                    if (cont == 0)
                    {*/

                    strSQL = "UPDATE ADMIN.USUARIO_EXTERNO SET NOMBRE_USUARIO_EXTERNO = '" + jsonbody.nombreusuario + "', EMAIL = '" + jsonbody.correousuario + "', NOTIFICAR = " + jsonbody.notificar + " WHERE ID_USUARIO_EXTERNO = " + jsonbody.idaux;

                    conn.EjecutarQuery(strSQL);

                    conn.Cerrar();

                    return Ok();
                    /*}
                    else
                    {
                        conn.Cerrar();
                        return BadRequest("Usuario Duplicado" + strSQL);
                    }*/
                }
                catch (Exception ex)
                {
                    conn.Cerrar();
                    return BadRequest(ex.Message + strSQL);
                }
            }
            else
            {
                return Unauthorized("No se recibió bearer token");
            }
            
        }

        [HttpPost("crearusuario")]
        public IActionResult CrearUsuario(Class.JsonBody jsonbody)
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                Cuenta cuenta;
                string connstr = "";
                try
                {
                    cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), bearerToken);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                if (!(cuenta.IdUser > 0))
                {
                    return Unauthorized("Sesion no encontrada");
                }

                if (jsonbody.idaux == Int32.Parse(_configuration.GetSection("Area:Custodia").Value))
                {
                    connstr = "Custodia1";
                }
                else
                {
                    connstr = "Default1";
                }

                string strSQL = "SELECT COUNT(*) FROM ADMIN.USUARIO WHERE NOMBRE_USUARIO = '" + jsonbody.nombreusuario + "'";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    int cont = conn.EjecutarQueryEscalar(strSQL);
                    if (cont == 0)
                    {
                        strSQL = "SELECT MAX(ORDEN) + 1 FROM ADMIN.USUARIO";
                        int orden = conn.EjecutarQueryEscalar(strSQL);

                        strSQL = "INSERT INTO ADMIN.USUARIO (NOMBRE_USUARIO, REAL, CAMBIAR_PASSWORD, ORDEN, CERRAR_SESION, DATAMANAGER, PASSWORDHASH, PASSWORDSALT, EMAIL, CONNUSER, ANULADO, ACCESO_PERMITIDO)";
                        strSQL += " VALUES ('" + jsonbody.nombreusuario + "', " + "1, 1, " + orden + ", 0, 0, '" + _configuration.GetSection("DefaultPassword:hash").Value + "', '" + _configuration.GetSection("DefaultPassword:salt").Value + "', '" + jsonbody.correousuario + "', '" + connstr + "', 0, 0)";
                        strSQL += " RETURNING ID_USUARIO INTO :numero";
                        int id = conn.InsertReturnID(strSQL);
                        if (id <= 0)
                        {
                            return BadRequest("Error Creando Usuario");
                        }
                        strSQL = "INSERT INTO ADMIN.PERMISO (ID_USUARIO_FK, BUSQUEDA, BUSQUEDA_HISTORICO, BUSQUEDA_EDITAR, ENTREGAR, ENTREGAR_EXPEDIENTE, ENTREGAR_DOCUMENTO, RECIBIR, RECIBIR_NUEVO, RECIBIR_REINGRESO, RECIBIR_CONFIRMAR, RECIBIR_MANUAL, PAGARE, PAGARE_BUSCAR, PAGARE_RECIBIR, PAGARE_ENTREGAR, LETRA, LETRA_NUEVO, LETRA_ENTREGAR, LETRA_REINGRESO, LETRA_BUSCAR, IRONMOUNTAIN, IRONMOUNTAIN_SOLICITAR, IRONMOUNTAIN_RECIBIR, IRONMOUNTAIN_ARMAR, IRONMOUNTAIN_ENVIAR, IRONMOUNTAIN_ENTREGAR, IRONMOUNTAIN_CARGO, BOVEDA, BOVEDA_CAJA_RETIRAR, BOVEDA_CAJA_GUARDAR, BOVEDA_DOCUMENTO_RETIRAR, BOVEDA_DOCUMENTO_GUARDAR, MANTENIMIENTO, MANTENIMIENTO_CUENTA, MANTENIMIENTO_CREDITO, MANTENIMIENTO_SOCIO, IMPORTAR, IMPORTAR_ACTIVAS, IMPORTAR_PASIVAS, NIVEL)";
                        if (jsonbody.idaux == Int32.Parse(_configuration.GetSection("Area:Custodia").Value))
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
                        conn.EjecutarQuery(strSQL);
                        conn.Cerrar();

                        return Ok();
                    }
                    else
                    {
                        conn.Cerrar();
                        return BadRequest("Usuario Duplicado");
                    }
                }
                catch (Exception ex)
                {
                    conn.Cerrar();
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return Unauthorized("No se recibió bearer token");
            }
            
        }


        [HttpPost("datousuario")]
        public IActionResult DatoCuenta(Class.JsonBody jsonbody)
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                DataTable dt;
                Cuenta cuenta;
                try
                {
                    cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), bearerToken);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                if (!(cuenta.IdUser > 0))
                {
                    return Unauthorized("Sesion no encontrada");
                }

                string strSQL = "SELECT * FROM ADMIN.USUARIO WHERE ID_USUARIO = " + jsonbody.idaux + "";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    dt = conn.LlenarDataTable(strSQL);

                    conn.Cerrar();

                    string json = JsonConvert.SerializeObject(dt);
                    return Ok(json);
                }
                catch (Exception ex)
                {
                    conn.Cerrar();
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return Unauthorized("No se recibió bearer token");
            }
            
        }


        [HttpPost("usuarioexternoorden")]
        public IActionResult UsuarioExternoOrden(Class.JsonBody jsonbody)
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                Cuenta cuenta;
                try
                {
                    cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), bearerToken);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                if (!(cuenta.IdUser > 0))
                {
                    return Unauthorized("Sesion no encontrada");
                }

                string strSQL = "UPDATE ADMIN.USUARIO_EXTERNO SET ORDEN = (SELECT ORDEN FROM ADMIN.USUARIO_EXTERNO WHERE ID_USUARIO_EXTERNO = " + jsonbody.idaux + ") WHERE ORDEN = (SELECT ORDEN FROM ADMIN.USUARIO_EXTERNO WHERE ID_USUARIO_EXTERNO = " + jsonbody.idaux + ") - (" + jsonbody.ordendif + ")";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.USUARIO_EXTERNO SET ORDEN = (SELECT ORDEN FROM ADMIN.USUARIO_EXTERNO WHERE ID_USUARIO_EXTERNO = " + jsonbody.idaux + ") + (" + jsonbody.ordendif + ") WHERE ID_USUARIO_EXTERNO = " + jsonbody.idaux;
                    conn.EjecutarQuery(strSQL);
                    conn.Cerrar();

                    return Ok();
                }
                catch (Exception ex)
                {
                    conn.Cerrar();
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return Unauthorized("No se recibió bearer token");
            }
            
        }
    }
}
