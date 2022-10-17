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

            string strSQL = "INSERT INTO ADMIN.INVENTARIO_GENERAL (NUMERO_DE_CAJA, ID_DEPARTAMENTO_FK, ID_DOCUMENTO_FK, FECHA_DESDE, FECHA_HASTA, DESCRIPCION_1, DESCRIPCION_2, DESCRIPCION_3, DESCRIPCION_4, DESCRIPCION_5, DESC_CONCAT, FECHA_POSEE, ID_USUARIO_POSEE, ID_ESTADO_FK, FECHA_MODIFICA, ID_USUARIO_MODIFICA, EXPEDIENTE)";
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
            if (jsontoken.descripcion1 != "")
            {
                strSQL += "'" + jsontoken.descripcion1 + "', ";
            }
            else
            {
                strSQL += "NULL, ";
            }
            if (jsontoken.descripcion2 != "")
            {
                strSQL += "'" + jsontoken.descripcion2 + "', ";
            }
            else
            {
                strSQL += "NULL, ";
            }
            if (jsontoken.descripcion3 != "")
            {
                strSQL += "'" + jsontoken.descripcion3 + "', ";
            }
            else
            {
                strSQL += "NULL, ";
            }
            if (jsontoken.descripcion4 != "")
            {
                strSQL += "'" + jsontoken.descripcion4 + "', ";
            }
            else
            {
                strSQL += "NULL, ";
            }
            if (jsontoken.descripcion5 != "")
            {
                strSQL += "'" + jsontoken.descripcion5 + "', ";
            }
            else
            {
                strSQL += "NULL, ";
            }

            //DESC_CONCAT
            strSQL += " '" + jsontoken.nomdepartamento + ";" + jsontoken.nomdocumento + ";" + jsontoken.fechadesde + ";" + jsontoken.fechahasta + ";" + jsontoken.descripcion1 + ";" + jsontoken.descripcion2 + ";" + jsontoken.descripcion3 + ";" + jsontoken.descripcion4 + ";" + jsontoken.descripcion5 + ";',";
            strSQL += " TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'),";
            strSQL += " " + cuenta.IdUser + ", " + _configuration.GetSection("Estados:Custodiado").Value + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), " + cuenta.IdUser + ", " + jsontoken.expediente + ")";
            strSQL += " RETURNING ID_INVENTARIO_GENERAL INTO :numero";

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                int lastinsertid = conn.InsertReturnID(strSQL);

                strSQL = "INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_INVENTARIO_GENERAL_FK, ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, ID_AREA_ENTREGA_FK, ID_AREA_RECIBE_FK, FECHA_INICIO, FECHA_FIN, OBSERVACION_RECIBE, RECIBIDO, ANULADO)";
                strSQL += " VALUES (" + lastinsertid + ", " + jsontoken.idaux + ", " + cuenta.IdUser + ", " + jsontoken.idareaentrega + ", " + jsontoken.idarearecibe + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + jsontoken.observacion + "', 1, 0)";

                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                if (jsontoken.pagare == 1)
                {
                    strSQL = "SELECT * FROM ADMIN.PAGARE WHERE SOLICITUD_SISGO = '" + jsontoken.descripcion2 + "'";

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

                        strSQL = "UPDATE ADMIN.PAGARE SET ID_USUARIO_POSEE = " + cuenta.IdUser + "";
                        strSQL += " WHERE ID_PAGARE = " + dt.Rows[0]["ID_PAGARE"].ToString();

                        conn.iniciaCommand(strSQL);
                        conn.ejecutarQuery();
                    }
                    else
                    {
                        //Nuevo
                        strSQL = "INSERT INTO ADMIN.PAGARE (SOLICITUD_SISGO, CODIGO_SOCIO, ID_USUARIO_POSEE, DESCRIPCION_3, DESCRIPCION_4, DESCRIPCION_5, CONCAT) VALUES (";
                        strSQL += "'" + jsontoken.descripcion2 + "', '" + jsontoken.descripcion3.Split('-')[0] + "', " + cuenta.IdUser + ", '" + jsontoken.descripcion3 + "', '" + jsontoken.descripcion4 + "', '" + jsontoken.descripcion5 + "', '" + jsontoken.descripcion2 + ";" + jsontoken.descripcion3 + ";" + jsontoken.descripcion4 + ";" + jsontoken.descripcion5 + "')";
                        strSQL += " RETURNING ID_PAGARE INTO :numero";

                        lastinsertid = conn.InsertReturnID(strSQL);


                        strSQL = "INSERT INTO ADMIN.PAGARE_HISTORICO (ID_PAGARE_FK, ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, FECHA_INICIO, FECHA_FIN, OBSERVACION_RECIBE, RECIBIDO, ANULADO) VALUES (";
                        strSQL += lastinsertid + ", " + jsontoken.idaux + ", " + cuenta.IdUser + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + jsontoken.observacion + "', 1, 0)";

                        conn.iniciaCommand(strSQL);
                        conn.ejecutarQuery();
                    }
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

        [HttpPost("duplicado")]
        public IActionResult RecibirDuplicado(Class.JsonToken jsontoken)
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
                int cont;
                string strSQL = "SELECT COUNT(*) FROM ADMIN.INVENTARIO_GENERAL WHERE DESC_CONCAT LIKE '%" + jsontoken.concat + "%'";
                conn.conectar();
                conn.iniciaCommand(strSQL);
                cont = conn.ejecutarQueryEscalar();
                conn.cerrar();
                return Ok(cont);
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
                string strSQL = "SELECT IG.ID_INVENTARIO_GENERAL AS ID, IG.NUMERO_DE_CAJA AS CAJA, DEP.NOMBRE_DEPARTAMENTO AS DEPART, DOC.NOMBRE_DOCUMENTO AS DOC, TO_CHAR(IG.FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(IG.FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, IG.DESCRIPCION_1 AS DESC_1, IG.DESCRIPCION_2 AS DESC_2, IG.DESCRIPCION_3 AS DESC_3, IG.DESCRIPCION_4 AS DESC_4, IG.DESCRIPCION_5 AS DESC_5, LE.NOMBRE_ESTADO AS CUSTODIADO, U.NOMBRE_USUARIO AS POSEE, TO_CHAR(FECHA_POSEE, 'dd/MM/yyyy hh:mm:ss') AS FECHA";
                strSQL += " FROM ((((ADMIN.INVENTARIO_GENERAL IG LEFT JOIN ADMIN.TMP_CARRITO TC ON IG.ID_INVENTARIO_GENERAL = TC.ID_INVENTARIO_GENERAL_FK)";
                strSQL += " LEFT JOIN ADMIN.LESTADO LE ON LE.ID_ESTADO = IG.ID_ESTADO_FK)";
                strSQL += " LEFT JOIN ADMIN.LDOCUMENTO DOC ON DOC.ID_DOCUMENTO = IG.ID_DOCUMENTO_FK)";
                strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON DEP.ID_DEPARTAMENTO = IG.ID_DEPARTAMENTO_FK)";
                strSQL += " LEFT JOIN ADMIN.USUARIO U ON U.ID_USUARIO = IG.ID_USUARIO_POSEE";
                strSQL += " WHERE TC.ID_TMP_CARRITO IS NULL AND (IG.ID_ESTADO_FK = " + _configuration.GetSection("Estados:Prestado").Value + " OR IG.ID_ESTADO_FK = " + _configuration.GetSection("Estados:Transito").Value + ") AND IG.ID_USUARIO_POSEE <> " + cuenta.IdUser + "";

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
                string strSQL = "SELECT ID_INVENTARIO_GENERAL AS ID, IH.OBSERVACION AS OBSERVACION_ENTREGA, IH.OBSERVACION_RECIBE, NUMERO_DE_CAJA AS CAJA, DEP.NOMBRE_DEPARTAMENTO AS DEPART, DOC.NOMBRE_DOCUMENTO AS DOC, TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, DESCRIPCION_1 AS DESC_1, DESCRIPCION_2 AS DESC_2, DESCRIPCION_3 AS DESC_3, DESCRIPCION_4 AS DESC_4, DESCRIPCION_5 AS DESC_5, LE.NOMBRE_ESTADO AS CUSTODIADO, U.NOMBRE_USUARIO AS ENTREGA, TO_CHAR(IH.FECHA_INICIO, 'DD/MM/YYYY HH24:MI:SS') AS INICIO";
                strSQL += " FROM (((((ADMIN.INVENTARIO_GENERAL IG LEFT JOIN (SELECT * FROM ADMIN.TMP_CARRITO WHERE TIPO = '" + _configuration.GetSection("TipoCarrito:RecibirConfirmar").Value + "') TC ON IG.ID_INVENTARIO_GENERAL = TC.ID_INVENTARIO_GENERAL_FK)";
                strSQL += " LEFT JOIN ADMIN.INVENTARIO_HISTORICO IH ON IH.ID_INVENTARIO_GENERAL_FK = IG.ID_INVENTARIO_GENERAL)";
                strSQL += " LEFT JOIN ADMIN.LESTADO LE ON LE.ID_ESTADO = IG.ID_ESTADO_FK)";
                strSQL += " LEFT JOIN ADMIN.LDOCUMENTO DOC ON DOC.ID_DOCUMENTO = IG.ID_DOCUMENTO_FK)";
                strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON DEP.ID_DEPARTAMENTO = IG.ID_DEPARTAMENTO_FK)";
                strSQL += " LEFT JOIN ADMIN.USUARIO U ON U.ID_USUARIO = IG.ID_USUARIO_POSEE";
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
                string strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET ID_ESTADO_FK = " + idestado + ", ID_USUARIO_POSEE = " + cuenta.IdUser + ", FECHA_POSEE = TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS') WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + "";

                conn.conectar(); 
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
    }
}
