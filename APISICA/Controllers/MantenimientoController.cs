﻿using APISICA.Class;
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
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    conn.Conectar();

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
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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
        public IActionResult DepartamentoOrden(MantenimientoDepartamentoOrdenClass deporden)
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

                string strSQL = "UPDATE ADMIN.LDEPARTAMENTO SET ORDEN = (SELECT ORDEN FROM ADMIN.LDEPARTAMENTO WHERE ID_DEPARTAMENTO = " + deporden.iddepartamento + ") WHERE ORDEN = (SELECT ORDEN FROM ADMIN.LDEPARTAMENTO WHERE ID_DEPARTAMENTO = " + deporden.iddepartamento + ") + (" + deporden.ordendif + ")";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    conn.Conectar();
                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.LDEPARTAMENTO SET ORDEN = (SELECT ORDEN FROM ADMIN.LDEPARTAMENTO WHERE ID_DEPARTAMENTO = " + deporden.iddepartamento + ") + (" + deporden.ordendif + ") WHERE ID_DEPARTAMENTO = " + deporden.iddepartamento;
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
        public IActionResult DepartamentoAnular(MantenimientoDepartamentoAnularClass depanular)
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

                string strSQL = "UPDATE ADMIN.LDEPARTAMENTO SET ANULADO = (SELECT ABS(ANULADO - 1) FROM ADMIN.LDEPARTAMENTO WHERE ID_DEPARTAMENTO = " + depanular.iddepartamento + ") WHERE ID_DEPARTAMENTO = " + depanular.iddepartamento;
                Conexion conn = new Conexion();

                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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
        public IActionResult DocumentoAgregar(MantenimientoDocumentoAgregarClass docagregar)
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

                string strSQL = "INSERT INTO ADMIN.LDOCUMENTO (ID_DOCUMENTO, NOMBRE_DOCUMENTO, ORDEN, ID_DEPARTAMENTO_FK, ANULADO) VALUES ((SELECT NVL(MAX(ID_DOCUMENTO),0)+1 FROM ADMIN.LDOCUMENTO), '" + docagregar.strdocumento + "', (SELECT NVL(MAX(ORDEN),0)+1 FROM ADMIN.LDOCUMENTO), " + docagregar.iddepartamento + ", 0)";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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
        public IActionResult DocumentoOrden(MantenimientoDocumentoOrdenClass docorden)
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

                string strSQL = "UPDATE ADMIN.LDOCUMENTO SET ORDEN = (SELECT ORDEN FROM ADMIN.LDOCUMENTO WHERE ID_DOCUMENTO = " + docorden.iddocumento + ") WHERE ORDEN = (SELECT ORDEN FROM ADMIN.LDOCUMENTO WHERE ID_DOCUMENTO = " + docorden.iddocumento + ") + (" + docorden.ordendif + ")";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    conn.Conectar();
                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.LDOCUMENTO SET ORDEN = (SELECT ORDEN FROM ADMIN.LDOCUMENTO WHERE ID_DOCUMENTO = " + docorden.iddocumento + ") + (" + docorden.ordendif + ") WHERE ID_DOCUMENTO = " + docorden.iddocumento;
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
        public IActionResult DocumentoAnular(MantenimientoDocumentoAnularClass docanular)
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

                string strSQL = "UPDATE ADMIN.LDOCUMENTO SET ANULADO = (SELECT ABS(ANULADO - 1) FROM ADMIN.LDOCUMENTO WHERE ID_DOCUMENTO = " + docanular.iddocumento + ") WHERE ID_DOCUMENTO = " + docanular.iddocumento;
                Conexion conn = new Conexion();

                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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
        public IActionResult DetalleAgregar(MantenimientoDetalleAgregarClass detagregar)
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

                string strSQL = "INSERT INTO ADMIN.LDETALLE (ID_DETALLE, NOMBRE_DETALLE, ORDEN, ID_DOCUMENTO_FK, ANULADO) VALUES ((SELECT NVL(MAX(ID_DETALLE),0)+1 FROM ADMIN.LDETALLE), '" + detagregar.strdetalle + "', (SELECT NVL(MAX(ORDEN),0)+1 FROM ADMIN.LDETALLE), " + detagregar.iddocumento + ", 0)";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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
        public IActionResult DetalleOrden(MantenimientoDetalleOrdenClass detorden)
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

                string strSQL = "UPDATE ADMIN.LDETALLE SET ORDEN = (SELECT ORDEN FROM ADMIN.LDETALLE WHERE ID_DETALLE = " + detorden.iddetalle + ") WHERE ORDEN = (SELECT ORDEN FROM ADMIN.LDETALLE WHERE ID_DETALLE = " + detorden.iddetalle + ") + (" + detorden.ordendif + ")";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    conn.Conectar();
                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.LDETALLE SET ORDEN = (SELECT ORDEN FROM ADMIN.LDETALLE WHERE ID_DETALLE = " + detorden.iddetalle + ") + (" + detorden.ordendif + ") WHERE ID_DETALLE = " + detorden.iddetalle;
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
        public IActionResult DetalleAnular(MantenimientoDetalleAnularClass detanular)
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

                string strSQL = "UPDATE ADMIN.LDETALLE SET ANULADO = (SELECT ABS(ANULADO - 1) FROM ADMIN.LDETALLE WHERE ID_DETALLE = " + detanular.iddetalle + ") WHERE ID_DETALLE = " + detanular.iddetalle;
                Conexion conn = new Conexion();

                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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


        [HttpPost("areaagregar")]
        public IActionResult AreaAgregar(MantenimientoAreaAgregarClass areaagregar)
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

                string strSQL = "INSERT INTO ADMIN.LAREA (ID_AREA, NOMBRE_AREA, ORDEN, ANULADO) VALUES ((SELECT NVL(MAX(ID_AREA),0)+1 FROM ADMIN.LAREA), '" + areaagregar.strarea + "', (SELECT NVL(MAX(ORDEN),0)+1 FROM ADMIN.LAREA), 0)";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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


        [HttpPost("areaorden")]
        public IActionResult AreaOrden(MantenimientoAreaOrdenClass areaorden)
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

                string strSQL = "UPDATE ADMIN.LAREA SET ORDEN = (SELECT ORDEN FROM ADMIN.LAREA WHERE ID_AREA = " + areaorden.idarea + ") WHERE ORDEN = (SELECT ORDEN FROM ADMIN.LAREA WHERE ID_AREA = " + areaorden.idarea + ") + (" + areaorden.ordendif + ")";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    conn.Conectar();
                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.LAREA SET ORDEN = (SELECT ORDEN FROM ADMIN.LAREA WHERE ID_AREA = " + areaorden.idarea + ") + (" + areaorden.ordendif + ") WHERE ID_AREA = " + areaorden.idarea;
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

        [HttpPost("areaanular")]
        public IActionResult AreaAnular(MantenimientoAreaAnularClass areaanular)
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

                string strSQL = "UPDATE ADMIN.LAREA SET ANULADO = (SELECT ABS(ANULADO - 1) FROM ADMIN.LAREA WHERE ID_AREA = " + areaanular.idarea + ") WHERE ID_AREA = " + areaanular.idarea;
                Conexion conn = new Conexion();

                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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


        [HttpPost("productoagregar")]
        public IActionResult ProductoAgregar(MantenimientoProductoAgregarClass prodagregar)
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

                string strSQL = "INSERT INTO ADMIN.LPRODUCTO (ID_PRODUCTO, NOMBRE_PRODUCTO, ORDEN, ANULADO) VALUES ((SELECT NVL(MAX(ID_PRODUCTO),0)+1 FROM ADMIN.LPRODUCTO), '" + prodagregar.strproducto + "', (SELECT NVL(MAX(ORDEN),0)+1 FROM ADMIN.LPRODUCTO), 0)";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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


        [HttpPost("productoorden")]
        public IActionResult ProductoOrden(MantenimientoProductoOrdenClass prodorden)
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

                string strSQL = "UPDATE ADMIN.LPRODUCTO SET ORDEN = (SELECT ORDEN FROM ADMIN.LPRODUCTO WHERE ID_PRODUCTO = " + prodorden.idproducto + ") WHERE ORDEN = (SELECT ORDEN FROM ADMIN.LPRODUCTO WHERE ID_PRODUCTO = " + prodorden.idproducto + ") + (" + prodorden.ordendif + ")";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    conn.Conectar();
                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.LPRODUCTO SET ORDEN = (SELECT ORDEN FROM ADMIN.LPRODUCTO WHERE ID_PRODUCTO = " + prodorden.idproducto + ") + (" + prodorden.ordendif + ") WHERE ID_PRODUCTO = " + prodorden.idproducto;
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

        [HttpPost("productoanular")]
        public IActionResult ProductoAnular(MantenimientoProductoAnularClass prodanular)
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

                string strSQL = "UPDATE ADMIN.LPRODUCTO SET ANULADO = (SELECT ABS(ANULADO - 1) FROM ADMIN.LPRODUCTO WHERE ID_PRODUCTO = " + prodanular.idproducto + ") WHERE ID_PRODUCTO = " + prodanular.idproducto;
                Conexion conn = new Conexion();

                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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

        [HttpPost("pendienteagregar")]
        public IActionResult PendienteAgregar(MantenimientoPendienteAgregarClass penagregar)
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

                string strSQL = "INSERT INTO ADMIN.LPENDIENTE (ID_PENDIENTE, NOMBRE_PENDIENTE, TIPO, ORDEN, ANULADO) VALUES ((SELECT NVL(MAX(ID_PENDIENTE),0)+1 FROM ADMIN.LPENDIENTE), '" + penagregar.strpendiente + "', " + penagregar.tipo + " (SELECT NVL(MAX(ORDEN),0)+1 FROM ADMIN.LPENDIENTE WHERE TIPO = " + penagregar.tipo + "), 0)";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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


        [HttpPost("pendienteorden")]
        public IActionResult PendienteOrden(MantenimientoPendienteOrdenClass penorden)
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

                string strSQL = "UPDATE ADMIN.LPENDIENTE SET ORDEN = (SELECT ORDEN FROM ADMIN.LPENDIENTE WHERE ID_PENDIENTE = " + penorden.idpendiente + ") WHERE ORDEN = (SELECT ORDEN FROM ADMIN.LPENDIENTE WHERE ID_PENDIENTE = " + penorden.idpendiente + ") + (" + penorden.ordendif + ") AND TIPO = " + penorden.tipo;

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    conn.Conectar();
                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.LPENDIENTE SET ORDEN = (SELECT ORDEN FROM ADMIN.LPENDIENTE WHERE ID_PENDIENTE = " + penorden.idpendiente + ") + (" + penorden.ordendif + ") WHERE ID_PENDIENTE = " + penorden.idpendiente;
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

        [HttpPost("pendienteanular")]
        public IActionResult PendienteAnular(MantenimientoPendienteAnularClass penanular)
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

                string strSQL = "UPDATE ADMIN.LPENDIENTE SET ANULADO = (SELECT ABS(ANULADO - 1) FROM ADMIN.LPENDIENTE WHERE ID_PENDIENTE = " + penanular.idpendiente + ") WHERE ID_PENDIENTE = " + penanular.idpendiente;
                Conexion conn = new Conexion();

                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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
        [HttpPost("centrocostoagregar")]
        public IActionResult CentroCostoAgregar(MantenimientoCentroCostoAgregarClass ccagregar)
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

                string strSQL = "INSERT INTO ADMIN.CENTRO_COSTO (ID_CENTRO_COSTO, NOMBRE_CENTRO_COSTO, CODIGO_CENTRO_COSTO, ORDEN, ANULADO) VALUES ((SELECT NVL(MAX(ID_CENTRO_COSTO),0)+1 FROM ADMIN.CENTRO_COSTO), '" + ccagregar.strnombrecentrocosto + "', '" + ccagregar.strcodigocentrocosto + "', (SELECT NVL(MAX(ORDEN),0)+1 FROM ADMIN.CENTRO_COSTO), 0)";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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


        [HttpPost("centrocostoorden")]
        public IActionResult CentroCostoOrden(MantenimientoCentroCostoOrdenClass prodorden)
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

                string strSQL = "UPDATE ADMIN.CENTRO_COSTO SET ORDEN = (SELECT ORDEN FROM ADMIN.CENTRO_COSTO WHERE ID_CENTRO_COSTO = " + prodorden.idcentrocosto + ") WHERE ORDEN = (SELECT ORDEN FROM ADMIN.CENTRO_COSTO WHERE ID_CENTRO_COSTO = " + prodorden.idcentrocosto + ") + (" + prodorden.ordendif + ")";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    conn.Conectar();
                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.CENTRO_COSTO SET ORDEN = (SELECT ORDEN FROM ADMIN.CENTRO_COSTO WHERE ID_CENTRO_COSTO = " + prodorden.idcentrocosto + ") + (" + prodorden.ordendif + ") WHERE ID_CENTRO_COSTO = " + prodorden.idcentrocosto;
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

        [HttpPost("centrocostoanular")]
        public IActionResult CentroCostoAnular(MantenimientoCentroCostoAnularClass prodanular)
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

                string strSQL = "UPDATE ADMIN.CENTRO_COSTO SET ANULADO = (SELECT ABS(ANULADO - 1) FROM ADMIN.CENTRO_COSTO WHERE ID_CENTRO_COSTO = " + prodanular.idcentrocosto + ") WHERE ID_CENTRO_COSTO = " + prodanular.idcentrocosto;
                Conexion conn = new Conexion();

                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
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
