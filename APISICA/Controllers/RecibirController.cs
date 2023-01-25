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
        public IActionResult AgregarRecibir(Class.JsonToken jsontoken)
        {
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

            string strSQL = @"INSERT INTO ADMIN.INVENTARIO_GENERAL (NUMERO_DE_CAJA, ID_DEPARTAMENTO_FK, ID_DOCUMENTO_FK, ID_DETALLE_FK, ID_CLASIFICACION_FK,
                            ID_PRODUCTO_FK, ID_CENTRO_COSTO_FK, ID_USUARIO_REGISTRA_FK, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD, OBSERVACION,
                            FECHA_DESDE, FECHA_HASTA, FECHA_MODIFICA, FECHA_POSEE, ID_UBICACION_FK, ID_USUARIO_POSEE_FK, ID_ESTADO_FK, FECHA_REGISTRO)";
            strSQL += " VALUES (";
            if (jsontoken.numerocaja != "")
            {
                strSQL += "'" + jsontoken.numerocaja + "', ";
            }
            else
            {
                strSQL += "NULL, ";
            }

            if (jsontoken.iddepartamento != -1)
            {
                strSQL += jsontoken.iddepartamento + ", ";
            }
            else
            {
                strSQL += "NULL, ";
            }

            if (jsontoken.iddocumento != -1)
            {
                strSQL += "" + jsontoken.iddocumento + ", ";
            }
            else
            {
                strSQL += "NULL, ";
            }

            if (jsontoken.iddetalle != -1)
            {
                strSQL += "" + jsontoken.iddetalle + ", ";
            }
            else
            {
                strSQL += "NULL, ";
            }

            if (jsontoken.idclasificacion != -1)
            {
                strSQL += "" + jsontoken.idclasificacion + ", ";
            }
            else
            {
                strSQL += "NULL, ";
            }

            if (jsontoken.idproducto != -1)
            {
                strSQL += "" + jsontoken.idproducto + ", ";
            }
            else
            {
                strSQL += "NULL, ";
            }

            if (jsontoken.idcentrocosto != -1)
            {
                strSQL += "" + jsontoken.idcentrocosto + ", ";
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

            strSQL += "'" + jsontoken.codigosocio + "', ";
            strSQL += "'" + jsontoken.nombresocio + "', ";
            strSQL += "'" + jsontoken.numerosolicitud + "', ";
            strSQL += "'" + jsontoken.observacion + "', ";

            if (jsontoken.fechadesde != "")
            {
                strSQL += "TO_DATE('" + jsontoken.fechadesde + "', 'YYYY-MM-DD HH24:MI:SS'), ";
            }
            else
            {
                strSQL += "NULL, ";
            }
            if (jsontoken.fechahasta != "")
            {
                strSQL += "TO_DATE('" + jsontoken.fechahasta + "', 'YYYY-MM-DD HH24:MI:SS'), ";
            }
            else
            {
                strSQL += "NULL, ";
            }
            //fechamodifica
            if (jsontoken.fecha != "")
            {
                strSQL += "TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), ";
            }
            else
            {
                strSQL += "NULL, ";
            }
            //fechaposee
            if (jsontoken.fecha != "")
            {
                strSQL += "TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), ";
            }
            else
            {
                strSQL += "NULL, ";
            }

            //UBICACION
            if (jsontoken.idubicacionrecibe != -1)
            {
                strSQL += "" + jsontoken.idubicacionrecibe + ", ";
            }
            else
            {
                strSQL += "NULL, ";
            }

            strSQL += "" + cuenta.IdUser + ", ";
            //estado
            strSQL += _configuration.GetSection("Estados:Custodiado").Value + ", ";
            //fecharegistra
            if (jsontoken.fecha != "")
            {
                strSQL += "TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'))";
            }
            else
            {
                strSQL += "NULL)";
            }

            strSQL += " RETURNING ID_INVENTARIO_GENERAL INTO :numero";

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                int lastinsertid = conn.InsertReturnID(strSQL);
                if (lastinsertid < 0)
                {
                    return BadRequest("Error Insert Inventario General\n" + strSQL);
                }
                strSQL = Functions.obtenerDescConcatSQL(lastinsertid);

                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                strSQL = "INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_INVENTARIO_GENERAL_FK, ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, FECHA_INICIO, FECHA_FIN, OBSERVACION_RECIBE, RECIBIDO, ANULADO, ID_UBICACION_ENTREGA_FK, ID_UBICACION_RECIBE_FK, USUARIO, FECHA)";
                strSQL += " VALUES (" + lastinsertid + ", " + jsontoken.idaux + ", " + cuenta.IdUser + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + jsontoken.observacion + "', 1, 0, " + jsontoken.idubicacionentrega + ", " + jsontoken.idubicacionrecibe + ", " + cuenta.IdUser  + ", SYSDATE)";

                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                if (jsontoken.pagare == 1)
                {
                    /*
                    strSQL = "SELECT * FROM ADMIN.PAGARE WHERE NUMEROSOLICITUD = '" + jsontoken.numerosolicitud + "'";

                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();
                    DataTable dt = conn.llenarDataTable();

                    if (dt.Rows.Count > 0)
                    {
                        //Existe, Retorno
                        strSQL = "INSERT INTO ADMIN.PAGARE_HISTORICO (ID_PAGARE_FK, ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, FECHA_INICIO, FECHA_FIN, OBSERVACION_RECIBE, RECIBIDO, ANULADO) VALUES (";
                        strSQL += dt.Rows[0]["ID_PAGARE"].ToString() + ", " + jsontoken.idaux + ", " + cuenta.IdUser + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + jsontoken.observacion + "', 1, 0)";

                        conn.iniciaCommand(strSQL);
                        conn.ejecutarQuery();

                        strSQL = "UPDATE ADMIN.PAGARE SET ID_USUARIO_POSEE_FK = " + cuenta.IdUser + "";
                        strSQL += " WHERE ID_PAGARE = " + dt.Rows[0]["ID_PAGARE"].ToString();

                        conn.iniciaCommand(strSQL);
                        conn.ejecutarQuery();
                    }
                    else
                    {
                        //Nuevo
                        strSQL = "INSERT INTO ADMIN.PAGARE (NUMEROSOLICITUD, CODIGO_SOCIO, NOMBRE_SOCIO, ID_USUARIO_POSEE_FK, CONCAT, FECHA_REGISTRA) VALUES (";
                        strSQL += "'" + jsontoken.numerosolicitud + "', '" + jsontoken.codigosocio + "', '" + jsontoken.nombresocio + "', " + cuenta.IdUser + ", '" + jsontoken.numerosolicitud + ";" + jsontoken.codigosocio + ";" + jsontoken.nombresocio + ";" + "', TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'))";
                        strSQL += " RETURNING ID_PAGARE INTO :numero";

                        lastinsertid = conn.InsertReturnID(strSQL);

                        strSQL = "INSERT INTO ADMIN.PAGARE_HISTORICO (ID_PAGARE_FK, ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, FECHA_INICIO, FECHA_FIN, OBSERVACION_RECIBE, RECIBIDO, ANULADO) VALUES (";
                        strSQL += lastinsertid + ", " + jsontoken.idaux + ", " + cuenta.IdUser + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + jsontoken.observacion + "', 1, 0)";
                        //strSQL += " RETURNING ID_PAGARE INTO :numero";

                        conn.iniciaCommand(strSQL);
                        conn.ejecutarQuery();
                    }
                    */
                }

                conn.cerrar();

                return Ok(strSQL);
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("validar")]
        public IActionResult RecibirValidar(Class.JsonToken jsontoken)
        {
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
            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                int cont, iddepartamento, iddocumento, iddetalle, idclasificacion, idproducto, idcentrocosto;
                string concat = "";
                string strSQL = "SELECT ID_DEPARTAMENTO FROM ADMIN.LDEPARTAMENTO WHERE NOMBRE_DEPARTAMENTO = '" + jsontoken.strdepartamento + "'";
                conn.iniciaCommand(strSQL);
                iddepartamento = conn.ejecutarQueryEscalar();
                if (iddepartamento > 0)
                {
                    concat += iddepartamento.ToString() + ";";
                }
                else
                {
                    return BadRequest("Error Departamento");
                }

                strSQL = "SELECT ID_DOCUMENTO FROM ADMIN.LDOCUMENTO WHERE NOMBRE_DOCUMENTO = '" + jsontoken.strdocumento + "' AND ID_DEPARTAMENTO_FK = " + iddepartamento;
                //conn.conectar();
                conn.iniciaCommand(strSQL);
                iddocumento = conn.ejecutarQueryEscalar();
                if (iddocumento > 0)
                {
                    concat += iddocumento.ToString() + ";";
                }
                else
                {
                    return BadRequest("Error Documento\n" + strSQL);
                }

                strSQL = "SELECT ID_DETALLE FROM ADMIN.LDETALLE WHERE NOMBRE_DETALLE = '" + jsontoken.strdetalle + "' AND ID_DOCUMENTO_FK = " + iddocumento;
                //conn.conectar();
                conn.iniciaCommand(strSQL);
                iddetalle = conn.ejecutarQueryEscalar();
                if (iddetalle > 0)
                {
                    concat += iddetalle.ToString() + ";";
                }
                else
                {
                    return Ok("Error Detalle");
                }

                strSQL = "SELECT ID_CLASIFICACION FROM ADMIN.LCLASIFICACION WHERE NOMBRE_CLASIFICACION = '" + jsontoken.strclasificacion + "'";
               // conn.conectar();
                conn.iniciaCommand(strSQL);
                idclasificacion = conn.ejecutarQueryEscalar();
                if (idclasificacion > 0)
                {
                    concat += idclasificacion.ToString() + ";";
                }
                else
                {
                    return Ok("Error Clasificacion");
                }

                strSQL = "SELECT ID_PRODUCTO FROM ADMIN.LPRODUCTO WHERE NOMBRE_PRODUCTO = '" + jsontoken.strproducto + "'";
                //conn.conectar();
                conn.iniciaCommand(strSQL);
                idproducto = conn.ejecutarQueryEscalar();
                if (idproducto > 0)
                {
                    concat += idproducto.ToString() + ";";
                }
                else
                {
                    return Ok("Error Producto");
                }

                strSQL = "SELECT ID_CENTRO_COSTO FROM ADMIN.CENTRO_COSTO WHERE NOMBRE_CENTRO_COSTO = '" + jsontoken.strcentrocosto + "'";
                //conn.conectar();
                conn.iniciaCommand(strSQL);
                idcentrocosto = conn.ejecutarQueryEscalar();
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
                strSQL += "AND TRIM(CODIGO_SOCIO) = TRIM('" + jsontoken.codigosocio + "')";
                strSQL += "AND TRIM(NOMBRE_SOCIO) = TRIM('" + jsontoken.nombresocio + "')";
                strSQL += "AND TRIM(NUMEROSOLICITUD) = TRIM('" + jsontoken.numerosolicitud + "')";
                if (jsontoken.fechadesde != "")
                {
                    strSQL += "AND FECHA_DESDE = TO_DATE('" + jsontoken.fechadesde + "', 'YYYY-MM-DD HH24:MI:SS')";
                }
                if (jsontoken.fechahasta != "")
                {
                    strSQL += "AND FECHA_HASTA = TO_DATE('" + jsontoken.fechahasta + "', 'YYYY-MM-DD HH24:MI:SS')";
                }


                //conn.conectar();
                conn.iniciaCommand(strSQL);
                cont = conn.ejecutarQueryEscalar();
                conn.cerrar();

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
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("buscarreingreso")]
        public IActionResult BuscarReingreso(Class.JsonToken jsontoken)
        {
            DataTable dt = new DataTable("Reingresos");
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
            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
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

                if (jsontoken.busquedalibre != "")
                {
                    strSQL += " AND DESC_CONCAT LIKE '%" + jsontoken.busquedalibre + "%'";
                }

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


        [HttpPost("buscarconfirmacionpendiente")]
        public IActionResult BuscarConfirmacionPendiente(Class.JsonToken jsontoken)
        {
            DataTable dt = new DataTable("ConfirmacionesPendientes");
            Cuenta cuenta;
            try
            {
                cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), jsontoken.token);
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
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
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

                if (jsontoken.busquedalibre != "")
                {
                    strSQL += " AND DESC_CONCAT LIKE '%" + jsontoken.busquedalibre + "%'";
                }

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

        [HttpPost("buscarvalija")]
        public IActionResult BuscarValija(Class.JsonToken jsontoken)
        {
            DataTable dt = new DataTable("BuscarValija");
            Cuenta cuenta;
            try
            {
                cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), jsontoken.token);
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
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
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
                strSQL += " WHERE UBI.ID_UBICACION = " + jsontoken.idubicacion; //Valija

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
                return BadRequest(ex.Message + strSQL);
            }
        }
        [HttpPost("buscarpendiente")]
        public IActionResult BuscarPendiente(Class.JsonToken jsontoken)
        {
            DataTable dt = new DataTable("BuscarPendiente");
            Cuenta cuenta;
            try
            {
                cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), jsontoken.token);
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
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
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
                //strSQL += " LEFT JOIN (SELECT * FROM ADMIN.LPENDIENTE WHERE TIPO = 1) LP1 ON LP1.ID_LPENDIENTE = IG.ID_LPENNOMBRE_FK";
                //strSQL += " LEFT JOIN (SELECT * FROM ADMIN.LPENDIENTE WHERE TIPO = 2) LP2 ON LP2.ID_LPENDIENTE = IG.ID_LPENDETALLE_FK";
                //strSQL += " LEFT JOIN (SELECT * FROM ADMIN.LPENDIENTE WHERE TIPO = 3) LP3 ON LP3.ID_LPENDIENTE = IG.ID_LPENBANCA_FK";
                strSQL += " WHERE UBI.ID_UBICACION = 9"; //Pendiente

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

        [HttpPost("recibir")]
        public IActionResult Recibir(Class.JsonToken jsontoken)
        {
            int idestado;
            Cuenta cuenta;
            try
            {
                cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), jsontoken.token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (cuenta.IdUser <= 0)
            {
                return Unauthorized("Sesion no encontrada");
            }

            /*if (jsontoken.entrega == 1)
            {
                //Entrega
                identrega = iduser;
                idrecibe = jsontoken.idaux;
            }
            else
            {
                //Recibe
                identrega = jsontoken.idaux;
                idrecibe = iduser;
            }*/

            if (jsontoken.idarearecibe == Int32.Parse(_configuration.GetSection("Area:Custodia").Value) || jsontoken.idarearecibe == Int32.Parse(_configuration.GetSection("Area:Administrador").Value))
            {
                idestado = Int32.Parse(_configuration.GetSection("Estados:Custodiado").Value);
            }
            else
            {
                idestado = Int32.Parse(_configuration.GetSection("Estados:Prestado").Value);
            }

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();

                string strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET ID_ESTADO_FK = " + idestado + ", ID_USUARIO_POSEE_FK = " + cuenta.IdUser + ", FECHA_POSEE = TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS') WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + "";
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                strSQL = "UPDATE ADMIN.INVENTARIO_HISTORICO SET RECIBIDO = 1, FECHA_FIN = TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), OBSERVACION_RECIBE = '" + jsontoken.observacion + "' WHERE ANULADO = 0 AND ID_USUARIO_RECIBE_FK = " + cuenta.IdUser + " AND ID_INVENTARIO_GENERAL_FK = " + jsontoken.idinventario + "";
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                strSQL = "UPDATE ADMIN.INVENTARIO_HISTORICO SET ANULADO = 1 WHERE RECIBIDO = 0 AND ID_INVENTARIO_GENERAL_FK = " + jsontoken.idinventario + "";
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                conn.cerrar();

                return Ok();
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("ValijaMover")]
        public IActionResult ValijaMover(Class.JsonToken jsontoken)
        {
            Cuenta cuenta;
            try
            {
                cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), jsontoken.token);
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
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                string strSQL = "INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_INVENTARIO_GENERAL_FK, ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, FECHA_INICIO, FECHA_FIN, OBSERVACION_RECIBE, RECIBIDO, ANULADO, ID_UBICACION_ENTREGA_FK, ID_UBICACION_RECIBE_FK, USUARIO, FECHA)";
                strSQL += " VALUES (" + jsontoken.idinventario + ", (SELECT ID_USUARIO_POSEE_FK FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + "), " + cuenta.IdUser + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + jsontoken.observacion + "', 1, 0, (SELECT ID_UBICACION_FK FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + "), " + jsontoken.idubicacionrecibe + ", " + cuenta.IdUser + ", SYSDATE)";
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET ID_ESTADO_FK = " + jsontoken.idestado + ", ID_UBICACION_FK = " + jsontoken.idubicacionrecibe + ", FECHA_POSEE = TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS') WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + "";

                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();


                strSQL = "UPDATE ADMIN.INVENTARIO_HISTORICO SET ANULADO = 1 WHERE RECIBIDO = 0 AND ID_INVENTARIO_GENERAL_FK = " + jsontoken.idinventario + "";
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                conn.cerrar();

                return Ok();
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("ActualizarPendiente")]
        public IActionResult ActualizarPendiente(Class.JsonToken jsontoken)
        {
            Cuenta cuenta;
            try
            {
                cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), jsontoken.token);
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
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();

                if (jsontoken.modificado)
                {
                    Functions.guardarEditar(conn, cuenta, jsontoken);
                }

                if (jsontoken.pendienteok)
                {
                    strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET ID_UBICACION_FK = " + jsontoken.idubicacionrecibe + ", FECHA_POSEE = TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS') WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + "";
                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();

                    strSQL = "INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_INVENTARIO_GENERAL_FK, ID_USUARIO_ENTREGA_FK, ID_AREA_ENTREGA_FK, FECHA_INICIO, FECHA_FIN, OBSERVACION_RECIBE, RECIBIDO, ANULADO, ID_UBICACION_ENTREGA_FK, ID_UBICACION_RECIBE_FK, USUARIO, FECHA)";
                    strSQL += " VALUES (" + jsontoken.idinventario + ", " + cuenta.IdUser + ", " + jsontoken.idareaentrega + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + jsontoken.observacion + "', 1, 0, " + jsontoken.idubicacionentrega + ", " + jsontoken.idubicacionrecibe + ", " + cuenta.IdUser + ", SYSDATE)";
                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();

                    strSQL = "UPDATE ADMIN.INVENTARIO_HISTORICO SET ANULADO = 1 WHERE RECIBIDO = 0 AND ID_INVENTARIO_GENERAL_FK = " + jsontoken.idinventario + "";
                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();
                }
                conn.cerrar();

                return Ok();
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }
    }
}
