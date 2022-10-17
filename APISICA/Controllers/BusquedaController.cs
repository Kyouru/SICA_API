using APISICA.Class;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Configuration;
using System.Data;
using System.Net;
using System.Reflection;


namespace APISICA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusquedaController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BusquedaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost("buscar")]
        public IActionResult Buscar(Class.JsonToken jsontoken)
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
            
            string
                strSQL = @"SELECT ID_INVENTARIO_GENERAL AS ID, TRIM(NUMERO_DE_CAJA) AS CAJA, TRIM(DEP.NOMBRE_DEPARTAMENTO) AS DEPART, TRIM(DOC.NOMBRE_DOCUMENTO) AS DOC, 
                        TO_CHAR(FECHA_DESDE, 'DD/MM/YYYY') AS DESDE, TO_CHAR(FECHA_HASTA, 'DD/MM/YYYY') AS HASTA, TRIM(DESCRIPCION_1) AS DESC_1, TRIM(DESCRIPCION_2) AS DESC_2,
                        TRIM(DESCRIPCION_3) AS DESC_3, TRIM(DESCRIPCION_4) AS DESC_4, TRIM(DESCRIPCION_5) AS DESC_5, TRIM(LE.NOMBRE_ESTADO) AS CUSTODIADO, TRIM(U.NOMBRE_USUARIO) AS POSEE, TO_CHAR(FECHA_POSEE, 'DD/MM/YYYY HH24:MI:SS') AS FECHA
                        FROM (((ADMIN.INVENTARIO_GENERAL IG
                        LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON IG.ID_DEPARTAMENTO_FK = DEP.ID_DEPARTAMENTO)
                        LEFT JOIN ADMIN.LDOCUMENTO DOC ON IG.ID_DOCUMENTO_FK = DOC.ID_DOCUMENTO)
                        LEFT JOIN ADMIN.USUARIO U ON U.ID_USUARIO = IG.ID_USUARIO_POSEE)
                        LEFT JOIN ADMIN.LESTADO LE ON LE.ID_ESTADO = IG.ID_ESTADO_FK";
            strSQL += " WHERE 1 = 1";
            if (jsontoken.busquedalibre != "")
                strSQL += " AND DESC_CONCAT LIKE '%" + jsontoken.busquedalibre + "%'";
            if (jsontoken.numerocaja != "")
                strSQL += " AND NUMERO_DE_CAJA LIKE '%" + jsontoken.numerocaja + "%'";
            if (jsontoken.fecha != "")
                strSQL += " AND TRUNC(FECHA_DESDE) <= TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD') AND TRUNC(FECHA_HASTA) >= TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD')";

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();
                dt = conn.llenarDataTable();
                dt.TableName = "Buscar";
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


        [HttpPost("datoseditar")]
        public IActionResult DatosEditar(Class.JsonToken jsontoken)
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

            string strSQL = @"SELECT *
                FROM (ADMIN.INVENTARIO_GENERAL IG LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON IG.ID_DEPARTAMENTO_FK = DEP.ID_DEPARTAMENTO)
                LEFT JOIN ADMIN.LDOCUMENTO DOC ON IG.ID_DOCUMENTO_FK = DOC.ID_DOCUMENTO
                WHERE IG.ID_INVENTARIO_GENERAL = " + jsontoken.idinventario;

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

        [HttpPost("guardareditar")]
        public IActionResult GuardarEditar(Class.JsonToken jsontoken)
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
            string strSQL = @"INSERT INTO ADMIN.INVENTARIO_ANTERIOR (ID_INVENTARIO_GENERAL_FK, NUMERO_DE_CAJA, CAJA_CLIENTE, ID_DEPARTAMENTO_FK, ID_DOCUMENTO_FK, FECHA_DESDE, FECHA_HASTA, DESCRIPCION_1, DESCRIPCION_2, DESCRIPCION_3, DESCRIPCION_4, DESCRIPCION_5, EXPEDIENTE, ID_USUARIO_MODIFICA, FECHA_MODIFICA) VALUES
                                (" + jsontoken.idinventario + ", '" + jsontoken.numerocaja + "', '" + jsontoken.numerocaja + "', " + jsontoken.iddepartamento + ", " + jsontoken.iddocumento + ",";
            if (jsontoken.fechadesde != "")
                strSQL += " TO_DATE('" + jsontoken.fechadesde + "', 'YYYY-MM-DD HH24:MI:SS'),";
            else
                strSQL += " NULL,";
            if (jsontoken.fechahasta != "")
                strSQL += " TO_DATE('" + jsontoken.fechahasta + "', 'YYYY-MM-DD HH24:MI:SS'),";
            else
                strSQL += " NULL,";
            strSQL += " '" + jsontoken.descripcion1 + "', '" + jsontoken.descripcion2 + "', '" + jsontoken.descripcion3 + "', '" + jsontoken.descripcion4 + "', '" + jsontoken.descripcion5 + "', " + jsontoken.expediente + ", " + cuenta.IdUser + ",";
            if (jsontoken.fechamodifica != "")
                strSQL += " TO_DATE('" + jsontoken.fechamodifica + "', 'YYYY-MM-DD HH24:MI:SS')";
            else
                strSQL += " NULL,";
            strSQL += ")";

            Conexion conn = new Conexion();

            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET";
                strSQL += " NUMERO_DE_CAJA = '" + jsontoken.numerocaja + "', CAJA_CLIENTE = '" + jsontoken.numerocaja + "', ID_DEPARTAMENTO_FK = " + jsontoken.iddepartamento + ", ID_DOCUMENTO_FK = " + jsontoken.iddocumento + ",";
                strSQL += " DESCRIPCION_1 = '" + jsontoken.descripcion1 + "', DESCRIPCION_2 = '" + jsontoken.descripcion2 + "', DESCRIPCION_3 = '" + jsontoken.descripcion3 + "', DESCRIPCION_4 = '" + jsontoken.descripcion4 + "', DESCRIPCION_5 = '" + jsontoken.descripcion5 + "',";

                if (jsontoken.fechadesde != "")
                    strSQL += " FECHA_DESDE = TO_DATE('" + jsontoken.fechadesde + "', 'YYYY-MM-DD HH24:MI:SS'),";
                else
                    strSQL += " FECHA_DESDE = NULL,";
                if (jsontoken.fechahasta != "")
                    strSQL += " FECHA_HASTA = TO_DATE('" + jsontoken.fechahasta + "', 'YYYY-MM-DD HH24:MI:SS'),";
                else
                    strSQL += " FECHA_HASTA = NULL,";
                if (jsontoken.fechamodifica != "")
                    strSQL += " FECHA_MODIFICA = TO_DATE('" + jsontoken.fechamodifica + "', 'YYYY-MM-DD HH24:MI:SS'),";
                else
                    strSQL += " FECHA_MODIFICA = NULL,";
                strSQL += " ID_USUARIO_MODIFICA = " + cuenta.IdUser + ", EXPEDIENTE = " + jsontoken.expediente + ",";
                strSQL += " DESC_CONCAT = '" + jsontoken.descripcion1 + ";" + jsontoken.descripcion2 + ";" + jsontoken.descripcion3 + ";" + jsontoken.descripcion4 + ";" + jsontoken.descripcion5 + ";" + jsontoken.nombredepartamento + ";" + jsontoken.nombredocumento + ";'";
                strSQL += " WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario;

                //conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                //conn.conectar();
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

        [HttpPost("historicomovimiento")]
        public IActionResult HistoricoMovimiento(Class.JsonToken jsontoken)
        {
            DataTable dt = new DataTable("HistoricoMovimiento");
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
            string strSQL = @"SELECT ID_INVENTARIO_GENERAL AS ID, TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, DESCRIPCION_1 AS DESC_1, DESCRIPCION_2 AS DESC_2, DESCRIPCION_3 AS DESC_3, DESCRIPCION_4 AS DESC_4, DESCRIPCION_5 AS DESC_5, U1.NOMBRE_USUARIO AS ENTREGA, U2.NOMBRE_USUARIO AS RECIBE, TO_CHAR(FECHA_INICIO, 'dd/MM/yyyy HH24:MI:SS') AS FECHA_ENTREGA, TO_CHAR(FECHA_FIN, 'dd/MM/yyyy HH24:MI:SS') AS FECHA_RECIBE, NUMERO_CAJA
                                FROM ((ADMIN.INVENTARIO_HISTORICO IH LEFT JOIN ADMIN.USUARIO U1 ON IH.ID_USUARIO_ENTREGA_FK = U1.ID_USUARIO) LEFT JOIN ADMIN.USUARIO U2 ON IH.ID_USUARIO_RECIBE_FK = U2.ID_USUARIO) LEFT JOIN ADMIN.INVENTARIO_GENERAL IG ON IG.ID_INVENTARIO_GENERAL = IH.ID_INVENTARIO_GENERAL_FK WHERE IH.ANULADO = 0 AND IH.RECIBIDO = 1
                                        AND IG.ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + " ORDER BY FECHA_INICIO";

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

        [HttpPost("historicoedicion")]
        public IActionResult HistoricoEditar(Class.JsonToken jsontoken)
        {
            DataTable dt = new DataTable("HistoricoEdicion");
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
            string strSQL = @"SELECT FECHA_MODIFICA, U.NOMBRE_USUARIO AS USUARIO_MODIFICA, DEP.NOMBRE_DEPARTAMENTO AS DEPART, DOC.NOMBRE_DOCUMENTO AS DOC, TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, DESCRIPCION_1 AS DESC_1, DESCRIPCION_2 AS DESC_2, DESCRIPCION_3 AS DESC_3, DESCRIPCION_4 AS DESC_4, DESCRIPCION_5 AS DESC_5, NUMERO_DE_CAJA
                                FROM ((ADMIN.INVENTARIO_ANTERIOR IA LEFT JOIN ADMIN.USUARIO U ON IA.ID_USUARIO_MODIFICA = U.ID_USUARIO)
                                LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON IA.ID_DEPARTAMENTO_FK = DEP.ID_DEPARTAMENTO)
                                LEFT JOIN ADMIN.LDOCUMENTO DOC ON IA.ID_DOCUMENTO_FK = DOC.ID_DOCUMENTO";
            strSQL += " WHERE IA.ID_INVENTARIO_GENERAL_FK = " + jsontoken.idinventario;
            strSQL += @" UNION ALL
                                SELECT FECHA_MODIFICA, U.NOMBRE_USUARIO AS USUARIO_MODIFICA, DEP.NOMBRE_DEPARTAMENTO AS DEPART, DOC.NOMBRE_DOCUMENTO AS DOC, TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, DESCRIPCION_1 AS DESC_1, DESCRIPCION_2 AS DESC_2, DESCRIPCION_3 AS DESC_3, DESCRIPCION_4 AS DESC_4, DESCRIPCION_5 AS DESC_5, NUMERO_DE_CAJA
                                FROM ((ADMIN.INVENTARIO_GENERAL IG LEFT JOIN ADMIN.USUARIO U ON IG.ID_USUARIO_MODIFICA = U.ID_USUARIO)
                                LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON IG.ID_DEPARTAMENTO_FK = DEP.ID_DEPARTAMENTO)
                                LEFT JOIN ADMIN.LDOCUMENTO DOC ON IG.ID_DOCUMENTO_FK = DOC.ID_DOCUMENTO";
            strSQL += " WHERE IG.ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + " ORDER BY FECHA_MODIFICA ASC";

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
