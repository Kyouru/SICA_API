using APISICA.Class;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Text.Json.Nodes;

namespace APISICA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CommonController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet("listaconcat")]
        public IActionResult ListaConcat()
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

                string strSQL = @"SELECT LDEP.NOMBRE_DEPARTAMENTO || LDOC.NOMBRE_DOCUMENTO || LDET.NOMBRE_DETALLE AS CONCAT,
                                LDEP.ID_DEPARTAMENTO || ';' || LDOC.ID_DOCUMENTO || ';' || LDET.ID_DETALLE AS ID,
                                FROM ADMIN.LDEPARTAMENTO LDEP
                                LEFT JOIN ADMIN.LDOCUMENTO LDOC ON LDEP.ID_DEPARTAMENTO = LDOC.ID_DEPARTAMENTO_FK
                                LEFT JOIN ADMIN.LDETALLE LDET ON LDOC.ID_DOCUMENTO = LDET.ID_DOCUMENTO_FK
                                WHERE LDEP.ANULADO = 0
                                    AND LDOC.ANULADO = 0
                                    AND LDET.ANULADO = 0";

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

        [HttpPost("listaarea")]
        public IActionResult ListaArea(Class.JsonBody jsonbody)
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
                string strSQL;
                if (jsonbody.anulado == 1)
                {
                    strSQL = "SELECT * FROM ADMIN.LAREA ORDER BY ORDEN ASC";
                }
                else
                {
                    strSQL = "SELECT * FROM ADMIN.LAREA WHERE ANULADO = 0 ORDER BY ORDEN ASC";
                }
                

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

        [HttpPost("listadepartamento")]
        public IActionResult ListaDepartamento(Class.JsonBody jsonbody)
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

                string strSQL;
                if (jsonbody.anulado == 1)
                {
                    strSQL = "SELECT * FROM ADMIN.LDEPARTAMENTO ORDER BY ORDEN ASC";
                }
                else
                {
                    strSQL = "SELECT * FROM ADMIN.LDEPARTAMENTO WHERE ANULADO = 0 ORDER BY ORDEN ASC";
                }
                

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

        [HttpPost("listadocumento")]
        public IActionResult ListaDocumento(Class.JsonBody jsonbody)
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

                string strSQL;
                if (jsonbody.anulado == 1)
                {
                    strSQL = "SELECT * FROM ADMIN.LDOCUMENTO WHERE ID_DEPARTAMENTO_FK = " + jsonbody.iddepartamento + " ORDER BY ORDEN ASC";
                }
                else
                {
                    strSQL = "SELECT * FROM ADMIN.LDOCUMENTO WHERE ANULADO = 0 AND ID_DEPARTAMENTO_FK = " + jsonbody.iddepartamento + " ORDER BY ORDEN ASC";
                }

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

        [HttpPost("listadetalle")]
        public IActionResult ListaDetalle(Class.JsonBody jsonbody)
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

                string strSQL;
                if (jsonbody.anulado == 1)
                {
                    strSQL = "SELECT * FROM ADMIN.LDETALLE WHERE ID_DOCUMENTO_FK = " + jsonbody.iddocumento + " ORDER BY ORDEN ASC";
                }
                else
                {
                    strSQL = "SELECT * FROM ADMIN.LDETALLE WHERE ANULADO = 0 AND ID_DOCUMENTO_FK = " + jsonbody.iddocumento + " ORDER BY ORDEN ASC";
                }
                

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

        [HttpPost("listaclasificacion")]
        public IActionResult ListaClasificacion(Class.JsonBody jsonbody)
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

                string strSQL;
                if (jsonbody.anulado == 1)
                {
                    strSQL = "SELECT ID_CLASIFICACION, NOMBRE_CLASIFICACION FROM ADMIN.LCLASIFICACION ORDER BY ORDEN ASC";
                }
                else
                {
                    strSQL = "SELECT ID_CLASIFICACION, NOMBRE_CLASIFICACION FROM ADMIN.LCLASIFICACION WHERE ANULADO = 0 ORDER BY ORDEN ASC";
                }
                
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
                    return BadRequest(ex.Message + strSQL);
                }
            }
            else
            {
                return Unauthorized("No se recibió bearer token");
            }
            
        }

        [HttpPost("listaproducto")]
        public IActionResult ListaProducto(Class.JsonBody jsonbody)
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

                string strSQL;
                if (jsonbody.anulado == 1)
                {
                    strSQL = "SELECT ID_PRODUCTO, NOMBRE_PRODUCTO FROM ADMIN.LPRODUCTO ORDER BY ORDEN ASC";
                }
                else
                {
                    strSQL = "SELECT ID_PRODUCTO, NOMBRE_PRODUCTO FROM ADMIN.LPRODUCTO WHERE ANULADO = 0 ORDER BY ORDEN ASC";
                }

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
                    return BadRequest(ex.Message + strSQL);
                }
            }
            else
            {
                return Unauthorized("No se recibió bearer token");
            }
            
        }
        [HttpGet("listapendientes")]
        public IActionResult ListaPendientes()
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

                string strSQL = "SELECT NOMBRE, TIPO FROM ADMIN.LPENDIENTE WHERE ANULADO = 0 ORDER BY TIPO ASC, ORDEN ASC";

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

        [HttpPost("listacentrocosto")]
        public IActionResult ListaCentroCosto(Class.JsonBody jsonbody)
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

                string strSQL;
                if (jsonbody.anulado == 1)
                {
                    strSQL = "SELECT ID_CENTRO_COSTO, NOMBRE_CENTRO_COSTO FROM ADMIN.CENTRO_COSTO ORDER BY ORDEN ASC";
                }
                else
                {
                    strSQL = "SELECT ID_CENTRO_COSTO, NOMBRE_CENTRO_COSTO FROM ADMIN.CENTRO_COSTO WHERE ANULADO = 0 ORDER BY ORDEN ASC";
                }

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

        [HttpPost("listausuarioexterno")]
        public IActionResult ListaUsuariosExternos(Class.JsonBody jsonbody)
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

                string strSQL = ""; ;
                if (jsonbody.tiposeleccionarusuario == 1)
                {
                    strSQL = "SELECT UX.ID_USUARIO_EXTERNO AS ID, UX.NOMBRE_USUARIO_EXTERNO, LA.NOMBRE_AREA, UX.EMAIL, UX.NOTIFICAR FROM ADMIN.USUARIO_EXTERNO UX ";
                    strSQL += "LEFT JOIN ADMIN.LAREA LA ON UX.ID_AREA_FK = LA.ID_AREA ";
                    strSQL += "WHERE UX.NOMBRE_USUARIO_EXTERNO LIKE '%" + jsonbody.busquedalibre + "%'";
                    if (jsonbody.anulado == 0)
                    {
                        strSQL += " AND UX.ANULADO = 0";
                    }
                    strSQL += " ORDER BY UX.ORDEN ASC";
                }
                else
                {
                    strSQL += " AND U.ANULADO = 0 AND U.ID_USUARIO <> " + cuenta.IdUser + " AND A.REAL = 1 AND U.REAL = 1";
                }

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

        [HttpPost("listaubicacion")]
        public IActionResult ListaUbicacion(Class.JsonBody jsonbody)
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

                string strSQL = ""; ;
                if (jsonbody.tiposeleccionarubicacion == 1)
                {
                    strSQL = "SELECT UBI.ID_UBICACION, UBI.NOMBRE_UBICACION, UBI.ORDEN, UBI.ANULADO, UBI.PRESTAR FROM ADMIN.UBICACION UBI ";
                    strSQL += "WHERE PRESTAR = 1";
                    if (jsonbody.anulado == 0)
                    {
                        strSQL += " AND UBI.ANULADO = 0";
                    }
                    strSQL += " ORDER BY UBI.ORDEN ASC";
                }
                else
                {
                    strSQL += " AND U.ANULADO = 0 AND U.ID_USUARIO <> " + cuenta.IdUser + " AND A.REAL = 1 AND U.REAL = 1";
                }

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


        [HttpGet("pendienteconfirmarrecepcion")]
        public IActionResult PendienteConfirmarRecepcion()
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                Cuenta cuenta;
                try
                {
                    cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("Usercheck"), bearerToken);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                if (!(cuenta.IdUser > 0))
                {
                    return Unauthorized("Sesion no encontrada");
                }

                string strSQL = "SELECT COUNT(*) FROM ADMIN.INVENTARIO_HISTORICO WHERE RECIBIDO = 0 AND ANULADO = 0 AND ID_USUARIO_RECIBE_FK = " + cuenta.IdUser;

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    int i = conn.EjecutarQueryEscalar(strSQL);
                    conn.Cerrar();
                    return Ok(i.ToString());
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

        [HttpPost("validarubicacion")]
        public IActionResult ValidarUbicacion(Class.JsonBody jsonbody)
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                Cuenta cuenta;
                try
                {
                    cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("Usercheck"), bearerToken);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                if (!(cuenta.IdUser > 0))
                {
                    return Unauthorized("Sesión no encontrada");
                }

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    string strSQL = "SELECT NVL(ID_UBICACION, 0) FROM ADMIN.UBICACION UBI WHERE UPPER(NOMBRE_UBICACION) = UPPER('" + jsonbody.strubicacion + "') AND ANULADO = 0";
                    int i = conn.EjecutarQueryEscalar(strSQL);
                    conn.Cerrar();

                    if (i > 0)
                    {
                        return Ok(i.ToString());
                    }
                    else
                    {
                        return BadRequest("Ubicación Inválida");
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

        [HttpPost("obtenercaja")]
        public IActionResult ObtenerCaja(Class.JsonBody jsonbody)
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
                if (cuenta.IdUser <= 0)
                {
                    return Unauthorized("Sesion no encontrada");
                }

                string strSQL;
                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();

                    strSQL = @"SELECT ID_INVENTARIO_GENERAL AS ID, ID_UBICACION, EST.NOMBRE_ESTADO AS ESTADO,
                        CASE WHEN UBI.ID_UBICACION = 1 THEN USU.NOMBRE_USUARIO
                             WHEN UBI.ID_UBICACION = 2 THEN USUEX.NOMBRE_USUARIO_EXTERNO
                             ELSE UBI.NOMBRE_UBICACION
                        END AS UBICACION,
                        IG.NUMERO_DE_CAJA AS CAJA, LDEP.NOMBRE_DEPARTAMENTO AS DEPARTAMENTO, 
                        TO_CHAR(IG.FECHA_DESDE, 'DD/MM/YYYY') AS DESDE, TO_CHAR(IG.FECHA_HASTA, 'DD/MM/YYYY') AS HASTA, LDOC.NOMBRE_DOCUMENTO AS DOCUMENTO, LDET.NOMBRE_DETALLE AS DETALLE,
                        NUMEROSOLICITUD, CODIGO_SOCIO AS CODIGO, NOMBRE_SOCIO AS NOMBRE, LCLA.NOMBRE_CLASIFICACION AS CLASIFICACION, LPRO.NOMBRE_PRODUCTO AS PRODUCTO
                        FROM ADMIN.INVENTARIO_GENERAL IG
                            LEFT JOIN ADMIN.LDEPARTAMENTO LDEP
                                ON IG.ID_DEPARTAMENTO_FK = LDEP.ID_DEPARTAMENTO
                            LEFT JOIN ADMIN.LDOCUMENTO LDOC
                                ON IG.ID_DOCUMENTO_FK = LDOC.ID_DOCUMENTO
                            LEFT JOIN ADMIN.LDETALLE LDET
                                ON IG.ID_DETALLE_FK = LDET.ID_DETALLE
                            LEFT JOIN ADMIN.LCLASIFICACION LCLA
                                ON IG.ID_CLASIFICACION_FK = LCLA.ID_CLASIFICACION
                            LEFT JOIN ADMIN.LPRODUCTO LPRO
                                ON IG.ID_PRODUCTO_FK = LPRO.ID_PRODUCTO
                            LEFT JOIN ADMIN.UBICACION UBI
                                ON IG.ID_UBICACION_FK = UBI.ID_UBICACION
                            LEFT JOIN ADMIN.USUARIO USU
                                ON IG.ID_USUARIO_POSEE_FK = USU.ID_USUARIO
                            LEFT JOIN ADMIN.USUARIO_EXTERNO USUEX
                                ON IG.ID_USUARIO_POSEE_FK = USUEX.ID_USUARIO_EXTERNO
                            LEFT JOIN ADMIN.CENTRO_COSTO CC
                                ON IG.ID_CENTRO_COSTO_FK = CC.ID_CENTRO_COSTO
                            LEFT JOIN ADMIN.LESTADO EST
                                ON IG.ID_ESTADO_FK = EST.ID_ESTADO";
                    strSQL += " WHERE NUMERO_DE_CAJA = '" + jsonbody.numerocaja + "'";

                    dt = conn.LlenarDataTable(strSQL);
                    conn.Cerrar();

                    if (dt.Rows.Count > 0)
                    {
                        string json = JsonConvert.SerializeObject(dt);
                        return Ok(json);
                    }
                    else
                    {
                        return BadRequest("Caja no Encontrada");
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

        [HttpPost("iddepartamento")]
        public IActionResult IdDepartamento(Class.JsonBody jsonbody)
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

                string strSQL = "SELECT ID_DEPARTAMENTO FROM ADMIN.LDEPARTAMENTO WHERE ANULADO = 0 AND NOMBRE_DEPARTAMENTO = " + jsonbody.strdepartamento;

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    int id = conn.EjecutarQueryEscalar(strSQL);
                    conn.Cerrar();
                    return Ok(id);
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

        [HttpGet("prueba")]
        public IActionResult Prueba()
        {
            return Ok("Phonk");
        }
    }
}
