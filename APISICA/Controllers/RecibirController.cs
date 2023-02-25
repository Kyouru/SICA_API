using APISICA.Class;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Net;

namespace APISICA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecibirController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RecibirController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("agregar")]
        public IActionResult AgregarRecibir(RecibirAgregarClass recagregar)
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

                string strSQL = @"INSERT INTO ADMIN.INVENTARIO_GENERAL (NUMERO_DE_CAJA, ID_DEPARTAMENTO_FK, ID_DOCUMENTO_FK, ID_DETALLE_FK, ID_CLASIFICACION_FK,
                            ID_PRODUCTO_FK, ID_CENTRO_COSTO_FK, ID_USUARIO_REGISTRA_FK, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD, OBSERVACION,
                            FECHA_DESDE, FECHA_HASTA, ID_UBICACION_FK, ID_USUARIO_POSEE_FK, ID_ESTADO_FK, FECHA_REGISTRO)";
                strSQL += " VALUES (";
                if (recagregar.numerocaja != "")
                {
                    strSQL += "'" + recagregar.numerocaja + "', ";
                }
                else
                {
                    strSQL += "NULL, ";
                }

                if (recagregar.iddepartamento != -1)
                {
                    strSQL += recagregar.iddepartamento + ", ";
                }
                else
                {
                    strSQL += "NULL, ";
                }

                if (recagregar.iddocumento != -1)
                {
                    strSQL += "" + recagregar.iddocumento + ", ";
                }
                else
                {
                    strSQL += "NULL, ";
                }

                if (recagregar.iddetalle != -1)
                {
                    strSQL += "" + recagregar.iddetalle + ", ";
                }
                else
                {
                    strSQL += "NULL, ";
                }

                if (recagregar.idclasificacion != -1)
                {
                    strSQL += "" + recagregar.idclasificacion + ", ";
                }
                else
                {
                    strSQL += "NULL, ";
                }

                if (recagregar.idproducto != -1)
                {
                    strSQL += "" + recagregar.idproducto + ", ";
                }
                else
                {
                    strSQL += "NULL, ";
                }

                if (recagregar.idcentrocosto != -1)
                {
                    strSQL += "" + recagregar.idcentrocosto + ", ";
                }
                else
                {
                    strSQL += "NULL, ";
                }

                if (cuenta.IdUser != -1)
                {
                    strSQL += "" + cuenta.IdUser + ", ";
                }
                else
                {
                    strSQL += "NULL, ";
                }

                strSQL += "'" + recagregar.codigosocio + "', ";
                strSQL += "'" + recagregar.nombresocio + "', ";
                strSQL += "'" + recagregar.numerosolicitud + "', ";
                strSQL += "'" + recagregar.observacion + "', ";

                if (recagregar.fechadesde != "")
                {
                    strSQL += "TO_DATE('" + recagregar.fechadesde + "', 'YYYY-MM-DD HH24:MI:SS'), ";
                }
                else
                {
                    strSQL += "NULL, ";
                }
                if (recagregar.fechahasta != "")
                {
                    strSQL += "TO_DATE('" + recagregar.fechahasta + "', 'YYYY-MM-DD HH24:MI:SS'), ";
                }
                else
                {
                    strSQL += "NULL, ";
                }

                //UBICACION
                /*
                if (recagregar.idubicacionrecibe != -1)
                {
                    strSQL += "" + recagregar.idubicacionrecibe + ", ";
                }
                else
                {
                    strSQL += "NULL, ";
                }
                */
                strSQL += "" + _configuration.GetSection("Ubicacion:Valija").Value + ", ";

                strSQL += "" + cuenta.IdUser + ", ";
                //estado
                strSQL += _configuration.GetSection("Estados:Custodiado").Value + ", ";
                //fecharegistra
                strSQL += "TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS'))";

                strSQL += " RETURNING ID_INVENTARIO_GENERAL INTO :numero";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    conn.Conectar();
                    int lastinsertid = conn.InsertReturnID(strSQL);
                    if (lastinsertid < 0)
                    {
                        return BadRequest("Error Insert Inventario General\n" + strSQL);
                    }
                    strSQL = Functions.obtenerDescConcatSQL(lastinsertid);

                    conn.EjecutarQuery(strSQL);

                    strSQL = "INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_INVENTARIO_GENERAL_FK, ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, FECHA_INICIO, FECHA_FIN, OBSERVACION_RECIBE, RECIBIDO, ANULADO, ID_UBICACION_ENTREGA_FK, ID_UBICACION_RECIBE_FK, USUARIO, FECHA)";
                    strSQL += " VALUES (" + lastinsertid + ", " + recagregar.idusuarioentrega + ", " + cuenta.IdUser + ", TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS'), '" + recagregar.observacion + "', 1, 0, " + recagregar.idubicacionentrega + ", " + _configuration.GetSection("Ubicacion:Valija").Value + ", " + cuenta.IdUser + ", TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS'))";

                    conn.EjecutarQuery(strSQL);

                    conn.Cerrar();

                    return Ok(strSQL);
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

        [HttpPost("validar")]
        public IActionResult RecibirValidar(RecibirValidarClass recvalidar)
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
                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    conn.Conectar();
                    int cont, iddepartamento, iddocumento, iddetalle, idclasificacion, idproducto, idcentrocosto;
                    string concat = "";
                    string strSQL = "SELECT ID_DEPARTAMENTO FROM ADMIN.LDEPARTAMENTO WHERE NOMBRE_DEPARTAMENTO = '" + recvalidar.strdepartamento + "'";
                    iddepartamento = conn.EjecutarQueryEscalar(strSQL);
                    if (iddepartamento > 0)
                    {
                        concat += iddepartamento.ToString() + ";";
                    }
                    else
                    {
                        return BadRequest("Error Departamento");
                    }

                    strSQL = "SELECT ID_DOCUMENTO FROM ADMIN.LDOCUMENTO WHERE NOMBRE_DOCUMENTO = '" + recvalidar.strdocumento + "' AND ID_DEPARTAMENTO_FK = " + iddepartamento;
                    //conn.Conectar();
                    iddocumento = conn.EjecutarQueryEscalar(strSQL);
                    if (iddocumento > 0)
                    {
                        concat += iddocumento.ToString() + ";";
                    }
                    else
                    {
                        return BadRequest("Error Documento\n" + strSQL);
                    }

                    strSQL = "SELECT ID_DETALLE FROM ADMIN.LDETALLE WHERE NOMBRE_DETALLE = '" + recvalidar.strdetalle + "' AND ID_DOCUMENTO_FK = " + iddocumento;
                    //conn.Conectar();
                    iddetalle = conn.EjecutarQueryEscalar(strSQL);
                    if (iddetalle > 0)
                    {
                        concat += iddetalle.ToString() + ";";
                    }
                    else
                    {
                        return Ok("Error Detalle");
                    }

                    strSQL = "SELECT ID_CLASIFICACION FROM ADMIN.LCLASIFICACION WHERE NOMBRE_CLASIFICACION = '" + recvalidar.strclasificacion + "'";
                    // conn.Conectar();
                    idclasificacion = conn.EjecutarQueryEscalar(strSQL);
                    if (idclasificacion > 0)
                    {
                        concat += idclasificacion.ToString() + ";";
                    }
                    else
                    {
                        return Ok("Error Clasificacion");
                    }

                    strSQL = "SELECT ID_PRODUCTO FROM ADMIN.LPRODUCTO WHERE NOMBRE_PRODUCTO = '" + recvalidar.strproducto + "'";
                    //conn.Conectar();
                    idproducto = conn.EjecutarQueryEscalar(strSQL);
                    if (idproducto > 0)
                    {
                        concat += idproducto.ToString() + ";";
                    }
                    else
                    {
                        return Ok("Error Producto");
                    }

                    strSQL = "SELECT ID_CENTRO_COSTO FROM ADMIN.CENTRO_COSTO WHERE NOMBRE_CENTRO_COSTO = '" + recvalidar.strcentrocosto + "'";
                    //conn.Conectar();
                    idcentrocosto = conn.EjecutarQueryEscalar(strSQL);
                    if (idcentrocosto > 0)
                    {
                        concat += idcentrocosto.ToString() + ";";
                    }
                    else
                    {
                        return Ok("Error Centro Costo");
                    }

                    strSQL = "SELECT COUNT(*) FROM ADMIN.INVENTARIO_GENERAL WHERE ID_DEPARTAMENTO_FK = " + iddepartamento + "";
                    strSQL += "AND ID_DOCUMENTO_FK = " + iddocumento;
                    strSQL += "AND ID_DETALLE_FK = " + iddetalle;
                    strSQL += "AND ID_CLASIFICACION_FK = " + idclasificacion;
                    strSQL += "AND ID_PRODUCTO_FK = " + idproducto;
                    strSQL += "AND ID_CENTRO_COSTO_FK = " + idcentrocosto;
                    strSQL += "AND TRIM(CODIGO_SOCIO) = TRIM('" + recvalidar.codigosocio + "')";
                    strSQL += "AND TRIM(NOMBRE_SOCIO) = TRIM('" + recvalidar.nombresocio + "')";
                    strSQL += "AND TRIM(NUMEROSOLICITUD) = TRIM('" + recvalidar.numerosolicitud + "')";
                    if (recvalidar.fechadesde != "")
                    {
                        strSQL += "AND FECHA_DESDE = TO_DATE('" + recvalidar.fechadesde + "', 'YYYY-MM-DD HH24:MI:SS')";
                    }
                    if (recvalidar.fechahasta != "")
                    {
                        strSQL += "AND FECHA_HASTA = TO_DATE('" + recvalidar.fechahasta + "', 'YYYY-MM-DD HH24:MI:SS')";
                    }


                    //conn.Conectar();
                    cont = conn.EjecutarQueryEscalar(strSQL);
                    conn.Cerrar();

                    if (cont > 0)
                    {
                        return Ok("Error Duplicado en BD");
                    }
                    else
                    {
                        return Ok(concat);
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

        /*
        [HttpPost("buscarreingreso")]
        public IActionResult BuscarReingreso(Class.JsonBody jsonbody)
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                DataTable dt = new DataTable("Reingresos");
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
                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    string strSQL = @"SELECT IG.ID_INVENTARIO_GENERAL AS ID, IG.NUMERO_DE_CAJA AS CAJA,
                                CASE WHEN UBI.ID_UBICACION = 1 THEN USU.NOMBRE_USUARIO
                                     WHEN UBI.ID_UBICACION = 2 THEN USUEX.NOMBRE_USUARIO_EXTERNO
                                     ELSE UBI.NOMBRE_UBICACION
                                END AS UBICACION,
                                LDEP.NOMBRE_DEPARTAMENTO AS DEP,
                                LDOC.NOMBRE_DOCUMENTO AS DOC, LDET.NOMBRE_DETALLE AS DET, TO_CHAR(IG.FECHA_DESDE, 'dd/MM/yyyy') AS DESDE,
                                TO_CHAR(IG.FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, LPROD.NOMBRE_PRODUCTO AS PRODUCTO, LE.NOMBRE_ESTADO AS CUSTODIADO,
                                USU.NOMBRE_USUARIO AS POSEE, TO_CHAR(FECHA_POSEE, 'dd/MM/yyyy hh:mm:ss') AS FECHA";
                    strSQL += " FROM ADMIN.INVENTARIO_GENERAL IG LEFT JOIN ADMIN.TMP_CARRITO TC ON IG.ID_INVENTARIO_GENERAL = TC.ID_INVENTARIO_GENERAL_FK";
                    strSQL += " LEFT JOIN ADMIN.LESTADO LE ON LE.ID_ESTADO = IG.ID_ESTADO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDOCUMENTO LDOC ON LDOC.ID_DOCUMENTO = IG.ID_DOCUMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO LDEP ON LDEP.ID_DEPARTAMENTO = IG.ID_DEPARTAMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDETALLE LDET ON LDET.ID_DETALLE = IG.ID_DETALLE_FK";
                    strSQL += " LEFT JOIN ADMIN.LPRODUCTO LPROD ON LPROD.ID_PRODUCTO = IG.ID_PRODUCTO_FK";
                    strSQL += " LEFT JOIN ADMIN.UBICACION UBI ON UBI.ID_UBICACION = IG.ID_UBICACION_FK";
                    strSQL += " LEFT JOIN ADMIN.USUARIO USU ON USU.ID_USUARIO = IG.ID_USUARIO_POSEE_FK";
                    strSQL += " LEFT JOIN ADMIN.USUARIO_EXTERNO USUEX ON USUEX.ID_USUARIO_EXTERNO = IG.ID_USUARIO_POSEE_FK";
                    strSQL += " WHERE TC.ID_TMP_CARRITO IS NULL AND (IG.ID_ESTADO_FK = " + _configuration.GetSection("Estados:Prestado").Value + " OR IG.ID_ESTADO_FK = " + _configuration.GetSection("Estados:Transito").Value + ") AND USU.ID_USUARIO <> " + cuenta.IdUser + "";

                    if (jsonbody.busquedalibre != "")
                    {
                        strSQL += " AND DESC_CONCAT LIKE '%" + jsonbody.busquedalibre + "%'";
                    }

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
        */

        /*
        [HttpPost("buscarconfirmacionpendiente")]
        public IActionResult BuscarConfirmacionPendiente(Class.JsonBody jsonbody)
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                DataTable dt = new DataTable("ConfirmacionesPendientes");
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
                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    string strSQL = @"SELECT ID_INVENTARIO_GENERAL AS ID,
                                CASE WHEN UBIENT.ID_UBICACION = 1 THEN USUENT.NOMBRE_USUARIO
                                     WHEN UBIENT.ID_UBICACION = 2 THEN USUEXENT.NOMBRE_USUARIO_EXTERNO
                                     ELSE UBIENT.NOMBRE_UBICACION
                                END AS ENTREGA,
                                CASE WHEN UBIREC.ID_UBICACION = 1 THEN USUREC.NOMBRE_USUARIO
                                     WHEN UBIREC.ID_UBICACION = 2 THEN USUEXREC.NOMBRE_USUARIO_EXTERNO
                                     ELSE UBIREC.NOMBRE_UBICACION
                                END AS RECIBE,
                                NUMERO_DE_CAJA AS CAJA, TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA,
                                LPROD.NOMBRE_PRODUCTO AS PRODUCTO, LDEP.NOMBRE_DEPARTAMENTO AS DEP,
                                LDOC.NOMBRE_DOCUMENTO AS DOC, LDET.NOMBRE_DETALLE AS DET, LE.NOMBRE_ESTADO AS ESTADO, TO_CHAR(IH.FECHA_INICIO, 'DD/MM/YYYY HH24:MI:SS') AS INICIO";
                    strSQL += " FROM ADMIN.INVENTARIO_GENERAL IG LEFT JOIN (SELECT * FROM ADMIN.TMP_CARRITO WHERE TIPO = '" + _configuration.GetSection("TipoCarrito:ValijaConfirmar").Value + "') TC ON IG.ID_INVENTARIO_GENERAL = TC.ID_INVENTARIO_GENERAL_FK";
                    strSQL += " LEFT JOIN ADMIN.INVENTARIO_HISTORICO IH ON IH.ID_INVENTARIO_GENERAL_FK = IG.ID_INVENTARIO_GENERAL";
                    strSQL += " LEFT JOIN ADMIN.LESTADO LE ON LE.ID_ESTADO = IG.ID_ESTADO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDOCUMENTO DOC ON DOC.ID_DOCUMENTO = IG.ID_DOCUMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON DEP.ID_DEPARTAMENTO = IG.ID_DEPARTAMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.UBICACION UBIREC ON UBIREC.ID_UBICACION = IH.ID_UBICACION_RECIBE_FK";
                    strSQL += " LEFT JOIN ADMIN.USUARIO USUREC ON USUREC.ID_USUARIO = IH.ID_USUARIO_RECIBE_FK";
                    strSQL += " LEFT JOIN ADMIN.USUARIO_EXTERNO USUEXREC ON USUEXREC.ID_USUARIO_EXTERNO = IH.ID_USUARIO_RECIBE_FK";
                    strSQL += " LEFT JOIN ADMIN.UBICACION UBIENT ON UBIENT.ID_UBICACION = IH.ID_UBICACION_ENTREGA_FK";
                    strSQL += " LEFT JOIN ADMIN.USUARIO USUENT ON USUENT.ID_USUARIO = IH.ID_USUARIO_ENTREGA_FK";
                    strSQL += " LEFT JOIN ADMIN.USUARIO_EXTERNO USUEXENT ON USUEXENT.ID_USUARIO_EXTERNO = IH.ID_USUARIO_ENTREGA_FK";
                    strSQL += " LEFT JOIN ADMIN.LDOCUMENTO LDOC ON LDOC.ID_DOCUMENTO = IG.ID_DOCUMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO LDEP ON LDEP.ID_DEPARTAMENTO = IG.ID_DEPARTAMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDETALLE LDET ON LDET.ID_DETALLE = IG.ID_DETALLE_FK";
                    strSQL += " LEFT JOIN ADMIN.LPRODUCTO LPROD ON LPROD.ID_PRODUCTO = IG.ID_PRODUCTO_FK";
                    strSQL += " WHERE TC.ID_TMP_CARRITO IS NULL AND IH.ID_USUARIO_RECIBE_FK = " + cuenta.IdUser + " AND IH.RECIBIDO = 0 AND IH.ANULADO = 0";
                    strSQL += " ORDER BY IH.FECHA_INICIO";

                    if (jsonbody.busquedalibre != "")
                    {
                        strSQL += " AND DESC_CONCAT LIKE '%" + jsonbody.busquedalibre + "%'";
                    }

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
        */

        [HttpGet("buscarvalija")]
        public IActionResult BuscarValija()
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                DataTable dt = new DataTable("BuscarValija");
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
                Conexion conn = new Conexion();
                string strSQL = "";
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    strSQL = @"SELECT ID_INVENTARIO_GENERAL AS ID,
                                CASE WHEN UBI.ID_UBICACION = 1 THEN USU.NOMBRE_USUARIO
                                     WHEN UBI.ID_UBICACION = 2 THEN USUEX.NOMBRE_USUARIO_EXTERNO
                                     ELSE UBI.NOMBRE_UBICACION
                                END AS UBICACION,
                                NUMERO_DE_CAJA AS CAJA, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD,
                                TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA,
                                LPROD.NOMBRE_PRODUCTO AS PRODUCTO, LDEP.NOMBRE_DEPARTAMENTO AS DEP,
                                LDOC.NOMBRE_DOCUMENTO AS DOC, LDET.NOMBRE_DETALLE AS DET, LE.NOMBRE_ESTADO AS ESTADO,
                                NUMERO_DE_CAJA || CODIGO_SOCIO || NOMBRE_SOCIO || NUMEROSOLICITUD || TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') ||
                                TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy')  AS CONCAT";
                    strSQL += " FROM ADMIN.INVENTARIO_GENERAL IG";
                    strSQL += " LEFT JOIN ADMIN.LESTADO LE ON LE.ID_ESTADO = IG.ID_ESTADO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDOCUMENTO DOC ON DOC.ID_DOCUMENTO = IG.ID_DOCUMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON DEP.ID_DEPARTAMENTO = IG.ID_DEPARTAMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.UBICACION UBI ON UBI.ID_UBICACION = IG.ID_UBICACION_FK";
                    strSQL += " LEFT JOIN ADMIN.USUARIO USU ON USU.ID_USUARIO = IG.ID_USUARIO_POSEE_FK";
                    strSQL += " LEFT JOIN ADMIN.USUARIO_EXTERNO USUEX ON USUEX.ID_USUARIO_EXTERNO = IG.ID_USUARIO_POSEE_FK";
                    strSQL += " LEFT JOIN ADMIN.LDOCUMENTO LDOC ON LDOC.ID_DOCUMENTO = IG.ID_DOCUMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO LDEP ON LDEP.ID_DEPARTAMENTO = IG.ID_DEPARTAMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDETALLE LDET ON LDET.ID_DETALLE = IG.ID_DETALLE_FK";
                    strSQL += " LEFT JOIN ADMIN.LPRODUCTO LPROD ON LPROD.ID_PRODUCTO = IG.ID_PRODUCTO_FK";
                    strSQL += " WHERE UBI.ID_UBICACION = " + _configuration.GetSection("Ubicacion:Valija").Value;

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


        [HttpGet("buscartransicion")]
        public IActionResult BuscarTransicion()
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                DataTable dt = new DataTable("BuscarTransicion");
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
                Conexion conn = new Conexion();
                string strSQL = "";
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    strSQL = @"SELECT ID_INVENTARIO_GENERAL AS ID,
                                CASE WHEN UBI.ID_UBICACION = 1 THEN USU.NOMBRE_USUARIO
                                     WHEN UBI.ID_UBICACION = 2 THEN USUEX.NOMBRE_USUARIO_EXTERNO
                                     ELSE UBI.NOMBRE_UBICACION
                                END AS UBICACION,
                                NUMERO_DE_CAJA AS CAJA, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD,
                                TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA,
                                LPROD.NOMBRE_PRODUCTO AS PRODUCTO, LDEP.NOMBRE_DEPARTAMENTO AS DEP,
                                LDOC.NOMBRE_DOCUMENTO AS DOC, LDET.NOMBRE_DETALLE AS DET, LE.NOMBRE_ESTADO AS ESTADO,
                                NUMERO_DE_CAJA || CODIGO_SOCIO || NOMBRE_SOCIO || NUMEROSOLICITUD || TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') ||
                                TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy')  AS CONCAT";
                    strSQL += " FROM ADMIN.INVENTARIO_GENERAL IG";
                    strSQL += " LEFT JOIN ADMIN.LESTADO LE ON LE.ID_ESTADO = IG.ID_ESTADO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDOCUMENTO DOC ON DOC.ID_DOCUMENTO = IG.ID_DOCUMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON DEP.ID_DEPARTAMENTO = IG.ID_DEPARTAMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.UBICACION UBI ON UBI.ID_UBICACION = IG.ID_UBICACION_FK";
                    strSQL += " LEFT JOIN ADMIN.USUARIO USU ON USU.ID_USUARIO = IG.ID_USUARIO_POSEE_FK";
                    strSQL += " LEFT JOIN ADMIN.USUARIO_EXTERNO USUEX ON USUEX.ID_USUARIO_EXTERNO = IG.ID_USUARIO_POSEE_FK";
                    strSQL += " LEFT JOIN ADMIN.LDOCUMENTO LDOC ON LDOC.ID_DOCUMENTO = IG.ID_DOCUMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO LDEP ON LDEP.ID_DEPARTAMENTO = IG.ID_DEPARTAMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDETALLE LDET ON LDET.ID_DETALLE = IG.ID_DETALLE_FK";
                    strSQL += " LEFT JOIN ADMIN.LPRODUCTO LPROD ON LPROD.ID_PRODUCTO = IG.ID_PRODUCTO_FK";
                    strSQL += " WHERE UBI.ID_UBICACION = " + _configuration.GetSection("Ubicacion:Transicion").Value;

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

        [HttpGet("buscarpendiente")]
        public IActionResult BuscarPendiente()
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                DataTable dt = new DataTable("BuscarPendiente");
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
                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    string strSQL = @"SELECT ID_INVENTARIO_GENERAL AS ID,
                                CASE WHEN UBI.ID_UBICACION = 1 THEN USU.NOMBRE_USUARIO
                                     WHEN UBI.ID_UBICACION = 2 THEN USUEX.NOMBRE_USUARIO_EXTERNO
                                     ELSE UBI.NOMBRE_UBICACION
                                END AS UBICACION,
                                NUMERO_DE_CAJA AS CAJA, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD,
                                TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA,
                                LPROD.NOMBRE_PRODUCTO AS PRODUCTO, LDEP.NOMBRE_DEPARTAMENTO AS DEP,
                                LDOC.NOMBRE_DOCUMENTO AS DOC, LDET.NOMBRE_DETALLE AS DET, LE.NOMBRE_ESTADO AS ESTADO,
                                PEN_NOMBRE, PEN_DETALLE, PEN_BANCA,
                                NUMERO_DE_CAJA || CODIGO_SOCIO || NOMBRE_SOCIO || NUMEROSOLICITUD || TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') ||
                                TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') || 
                                PEN_NOMBRE || PEN_NOMBRE || PEN_NOMBRE AS CONCAT";
                    strSQL += " FROM ADMIN.INVENTARIO_GENERAL IG";
                    strSQL += " LEFT JOIN ADMIN.LESTADO LE ON LE.ID_ESTADO = IG.ID_ESTADO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDOCUMENTO DOC ON DOC.ID_DOCUMENTO = IG.ID_DOCUMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON DEP.ID_DEPARTAMENTO = IG.ID_DEPARTAMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.UBICACION UBI ON UBI.ID_UBICACION = IG.ID_UBICACION_FK";
                    strSQL += " LEFT JOIN ADMIN.USUARIO USU ON USU.ID_USUARIO = IG.ID_USUARIO_POSEE_FK";
                    strSQL += " LEFT JOIN ADMIN.USUARIO_EXTERNO USUEX ON USUEX.ID_USUARIO_EXTERNO = IG.ID_USUARIO_POSEE_FK";
                    strSQL += " LEFT JOIN ADMIN.LDOCUMENTO LDOC ON LDOC.ID_DOCUMENTO = IG.ID_DOCUMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO LDEP ON LDEP.ID_DEPARTAMENTO = IG.ID_DEPARTAMENTO_FK";
                    strSQL += " LEFT JOIN ADMIN.LDETALLE LDET ON LDET.ID_DETALLE = IG.ID_DETALLE_FK";
                    strSQL += " LEFT JOIN ADMIN.LPRODUCTO LPROD ON LPROD.ID_PRODUCTO = IG.ID_PRODUCTO_FK";
                    strSQL += " WHERE UBI.ID_UBICACION = " + _configuration.GetSection("Ubicacion:Pendiente").Value;

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


        [HttpPost("recibir")]
        public IActionResult Recibir(RecibirRecibirClass recibir)
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
                if (cuenta.IdUser <= 0)
                {
                    return Unauthorized("Sesion no encontrada");
                }

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    conn.Conectar();
                    string strSQL = "INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_INVENTARIO_GENERAL_FK, ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, FECHA_INICIO, FECHA_FIN, OBSERVACION_RECIBE, RECIBIDO, ANULADO, ID_UBICACION_ENTREGA_FK, ID_UBICACION_RECIBE_FK, USUARIO, FECHA)";
                    strSQL += " VALUES (" + recibir.idinventario + ", (SELECT ID_USUARIO_POSEE_FK FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + recibir.idinventario + "), " + cuenta.IdUser + ", TO_DATE('" + recibir.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + recibir.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + recibir.observacion + "', 1, 0, (SELECT ID_UBICACION_FK FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + recibir.idinventario + "), " + recibir.idubicacionrecibe + ", " + cuenta.IdUser + ", TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS'))";
                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET ID_ESTADO_FK = " + recibir.idestado + ", ID_UBICACION_FK = " + recibir.idubicacionrecibe + ", FECHA_POSEE = TO_DATE('" + recibir.fecha + "', 'YYYY-MM-DD HH24:MI:SS') WHERE ID_INVENTARIO_GENERAL = " + recibir.idinventario + "";

                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.INVENTARIO_HISTORICO SET ANULADO = 1 WHERE RECIBIDO = 0 AND ID_INVENTARIO_GENERAL_FK = " + recibir.idinventario + "";
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


        [HttpPost("ActualizarPendiente")]
        public IActionResult ActualizarPendiente(GuardarClass recactualizarpen)
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
                if (cuenta.IdUser <= 0)
                {
                    return Unauthorized("Sesion no encontrada");
                }
                string strSQL = "";
                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    conn.Conectar();

                    if (recactualizarpen.modificado)
                    {
                        Functions.guardarEditar(conn, cuenta, recactualizarpen);
                    }

                    if (recactualizarpen.pendienteok)
                    {
                        strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET ID_UBICACION_FK = " + recactualizarpen.idubicacionrecibe + ", FECHA_POSEE = TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS') WHERE ID_INVENTARIO_GENERAL = " + recactualizarpen.idinventario + "";
                        conn.EjecutarQuery(strSQL);

                        strSQL = "INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_INVENTARIO_GENERAL_FK, ID_USUARIO_ENTREGA_FK, FECHA_INICIO, FECHA_FIN, OBSERVACION_RECIBE, RECIBIDO, ANULADO, ID_UBICACION_ENTREGA_FK, ID_UBICACION_RECIBE_FK, USUARIO, FECHA)";
                        strSQL += " VALUES (" + recactualizarpen.idinventario + ", " + cuenta.IdUser + ", TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS'), '" + recactualizarpen.observacion + "', 1, 0, " + _configuration.GetSection("Ubicacion:Pendiente").Value + ", " + recactualizarpen.idubicacionrecibe + ", " + cuenta.IdUser + ", TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS'))";
                        conn.EjecutarQuery(strSQL);

                        strSQL = "UPDATE ADMIN.INVENTARIO_HISTORICO SET ANULADO = 1 WHERE RECIBIDO = 0 AND ID_INVENTARIO_GENERAL_FK = " + recactualizarpen.idinventario + "";
                        conn.EjecutarQuery(strSQL);
                    }
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
