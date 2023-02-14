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



        [HttpPost("crearusuarioexterno")]
        public IActionResult UsuarioExternoCrear(UsuarioExternoCrearClass usuext)
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

                string strSQL = "SELECT COUNT(*) FROM ADMIN.USUARIO_EXTERNO WHERE NOMBRE_USUARIO_EXTERNO = '" + usuext.nombreusuario + "'";

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

                        strSQL = "INSERT INTO ADMIN.USUARIO_EXTERNO (NOMBRE_USUARIO_EXTERNO, EMAIL, NOTIFICAR, ORDEN, ID_AREA_FK, ANULADO)";
                        strSQL += " VALUES ('" + usuext.nombreusuario + "', '" + usuext.correousuario + "', " + usuext.notificar + ", " + orden + ", " + usuext.idarea + ", 0)";
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
        public IActionResult ModificarUsuarioExterno(UsuarioExternoModificarClass usuext)
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

                string strSQL = "SELECT COUNT(*) FROM ADMIN.USUARIO_EXTERNO WHERE NOMBRE_USUARIO_EXTERNO = '" + usuext.nombreusuario + "'";

                Conexion conn = new Conexion();
                try
                {
                    //conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    /*conn.iniciaCommand(strSQL);
                    int cont = conn.ejecutarQueryEscalar();
                    if (cont == 0)
                    {*/

                    strSQL = "UPDATE ADMIN.USUARIO_EXTERNO SET NOMBRE_USUARIO_EXTERNO = '" + usuext.nombreusuario + "', EMAIL = '" + usuext.correousuario + "', NOTIFICAR = " + usuext.notificar + ", ID_AREA_FK = " + usuext.idarea + " WHERE ID_USUARIO_EXTERNO = " + usuext.idaux;

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

        [HttpPost("usuarioexternoorden")]
        public IActionResult UsuarioExternoOrden(UsuarioExternoOrdenClass usuext)
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

                string strSQL = "UPDATE ADMIN.USUARIO_EXTERNO SET ORDEN = (SELECT ORDEN FROM ADMIN.USUARIO_EXTERNO WHERE ID_USUARIO_EXTERNO = " + usuext.idusuarioexterno + ") WHERE ORDEN = (SELECT ORDEN FROM ADMIN.USUARIO_EXTERNO WHERE ID_USUARIO_EXTERNO = " + usuext.idusuarioexterno + ") + (" + usuext.ordendif + ")";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.USUARIO_EXTERNO SET ORDEN = (SELECT ORDEN FROM ADMIN.USUARIO_EXTERNO WHERE ID_USUARIO_EXTERNO = " + usuext.idusuarioexterno + ") + (" + usuext.ordendif + ") WHERE ID_USUARIO_EXTERNO = " + usuext.idusuarioexterno;
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

        [HttpPost("departamentoagregar")]
        public IActionResult DepartamentoAgregar(MantenimientoDepartamentoAgregarClass depagregar)
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

                string strSQL = "INSERT INTO ADMIN.LDEPARTAMENTO (ID_DEPARTAMENTO, NOMBRE_DEPARTAMENTO, ORDEN, ANULADO) VALUES ((SELECT NVL(MAX(ID_DEPARTAMENTO),0)+1 FROM ADMIN.LDEPARTAMENTO), '" + depagregar.strdepartamento + "', (SELECT NVL(MAX(ORDEN),0)+1 FROM ADMIN.LDEPARTAMENTO), 0)";
                
                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
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


        [HttpPost("departamentoorden")]
        public IActionResult DepartamentoOrden(Class.JsonBody jsonbody)
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

                string strSQL = "UPDATE ADMIN.LDEPARTAMENTO SET ORDEN = (SELECT ORDEN FROM ADMIN.LDEPARTAMENTO WHERE ID_DEPARTAMENTO = " + jsonbody.iddepartamento + ") WHERE ORDEN = (SELECT ORDEN FROM ADMIN.LDEPARTAMENTO WHERE ID_DEPARTAMENTO = " + jsonbody.iddepartamento + ") + (" + jsonbody.ordendif + ")";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.LDEPARTAMENTO SET ORDEN = (SELECT ORDEN FROM ADMIN.LDEPARTAMENTO WHERE ID_DEPARTAMENTO = " + jsonbody.iddepartamento + ") + (" + jsonbody.ordendif + ") WHERE ID_DEPARTAMENTO = " + jsonbody.iddepartamento;
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

        [HttpPost("departamentoanular")]
        public IActionResult DepartamentoAnular(Class.JsonBody jsonbody)
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

                string strSQL = "UPDATE ADMIN.LDEPARTAMENTO SET ANULADO = (SELECT ABS(ANULADO - 1) FROM ADMIN.LDEPARTAMENTO WHERE ID_DEPARTAMENTO = " + jsonbody.iddepartamento + ") WHERE ID_DEPARTAMENTO = " + jsonbody.iddepartamento;
                Conexion conn = new Conexion();

                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
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


        [HttpPost("documentoagregar")]
        public IActionResult DocumentoAgregar(Class.JsonBody jsonbody)
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

                string strSQL = "INSERT INTO ADMIN.LDOCUMENTO (ID_DOCUMENTO, NOMBRE_DOCUMENTO, ORDEN, ID_DEPARTAMENTO_FK, ANULADO) VALUES ((SELECT NVL(MAX(ID_DOCUMENTO),0)+1 FROM ADMIN.LDOCUMENTO), '" + jsonbody.strdocumento + "', (SELECT NVL(MAX(ORDEN),0)+1 FROM ADMIN.LDOCUMENTO), " + jsonbody.iddepartamento + ", 0)";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
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

        [HttpPost("documentoorden")]
        public IActionResult DocumentoOrden(Class.JsonBody jsonbody)
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

                string strSQL = "UPDATE ADMIN.LDOCUMENTO SET ORDEN = (SELECT ORDEN FROM ADMIN.LDOCUMENTO WHERE ID_DOCUMENTO = " + jsonbody.iddocumento + ") WHERE ORDEN = (SELECT ORDEN FROM ADMIN.LDOCUMENTO WHERE ID_DOCUMENTO = " + jsonbody.iddocumento + ") + (" + jsonbody.ordendif + ")";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.LDOCUMENTO SET ORDEN = (SELECT ORDEN FROM ADMIN.LDOCUMENTO WHERE ID_DOCUMENTO = " + jsonbody.iddocumento + ") + (" + jsonbody.ordendif + ") WHERE ID_DOCUMENTO = " + jsonbody.iddocumento;
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

        [HttpPost("documentoanular")]
        public IActionResult DocumentoAnular(Class.JsonBody jsonbody)
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

                string strSQL = "UPDATE ADMIN.LDOCUMENTO SET ANULADO = (SELECT ABS(ANULADO - 1) FROM ADMIN.LDOCUMENTO WHERE ID_DOCUMENTO = " + jsonbody.iddocumento + ") WHERE ID_DOCUMENTO = " + jsonbody.iddocumento;
                Conexion conn = new Conexion();

                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
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

        [HttpPost("detalleagregar")]
        public IActionResult DetalleAgregar(Class.JsonBody jsonbody)
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

                string strSQL = "INSERT INTO ADMIN.LDETALLE (ID_DETALLE, NOMBRE_DETALLE, ORDEN, ID_DOCUMENTO_FK, ANULADO) VALUES ((SELECT NVL(MAX(ID_DETALLE),0)+1 FROM ADMIN.LDETALLE), '" + jsonbody.strdetalle + "', (SELECT NVL(MAX(ORDEN),0)+1 FROM ADMIN.LDETALLE), " + jsonbody.iddocumento + ", 0)";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
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

        [HttpPost("detalleorden")]
        public IActionResult DetalleOrden(Class.JsonBody jsonbody)
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

                string strSQL = "UPDATE ADMIN.LDETALLE SET ORDEN = (SELECT ORDEN FROM ADMIN.LDETALLE WHERE ID_DETALLE = " + jsonbody.iddetalle + ") WHERE ORDEN = (SELECT ORDEN FROM ADMIN.LDETALLE WHERE ID_DETALLE = " + jsonbody.iddetalle + ") + (" + jsonbody.ordendif + ")";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.LDETALLE SET ORDEN = (SELECT ORDEN FROM ADMIN.LDETALLE WHERE ID_DETALLE = " + jsonbody.iddetalle + ") + (" + jsonbody.ordendif + ") WHERE ID_DETALLE = " + jsonbody.iddetalle;
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

        [HttpPost("detalleanular")]
        public IActionResult DetalleAnular(Class.JsonBody jsonbody)
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

                string strSQL = "UPDATE ADMIN.LDETALLE SET ANULADO = (SELECT ABS(ANULADO - 1) FROM ADMIN.LDETALLE WHERE ID_DETALLE = " + jsonbody.iddetalle + ") WHERE ID_DETALLE = " + jsonbody.iddetalle;
                Conexion conn = new Conexion();

                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
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
