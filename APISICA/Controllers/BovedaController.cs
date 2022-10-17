using APISICA.Class;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;

namespace APISICA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BovedaController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BovedaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("buscarretirardocumento")]
        public IActionResult BuscarRetirarDocumento(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT ID_INVENTARIO_GENERAL AS ID, U.ID_USUARIO AS ID_BOVEDA, U.NOMBRE_USUARIO AS POSEE, NUMERO_DE_CAJA AS CAJA, DEP.NOMBRE_DEPARTAMENTO AS DEPART, DOC.NOMBRE_DOCUMENTO AS DOC, TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, DESCRIPCION_1 AS DESC_1, DESCRIPCION_2 AS DESC_2, DESCRIPCION_3 AS DESC_3, DESCRIPCION_4 AS DESC_4, DESCRIPCION_5 AS DESC_5, LE.NOMBRE_ESTADO AS CUSTODIADO, TO_CHAR(FECHA_POSEE, 'dd/MM/yyyy hh:mm:ss') AS FECHA";
            strSQL += " FROM ((((ADMIN.INVENTARIO_GENERAL IG LEFT JOIN ADMIN.USUARIO U ON U.ID_USUARIO = IG.ID_USUARIO_POSEE)";
            strSQL += " LEFT JOIN ADMIN.TMP_CARRITO TC ON TC.ID_INVENTARIO_GENERAL_FK = IG.ID_INVENTARIO_GENERAL)";
            strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON IG.ID_DEPARTAMENTO_FK = DEP.ID_DEPARTAMENTO)";
            strSQL += " LEFT JOIN ADMIN.LDOCUMENTO DOC ON IG.ID_DOCUMENTO_FK = DOC.ID_DOCUMENTO)";
            strSQL += " LEFT JOIN ADMIN.LESTADO LE ON LE.ID_ESTADO = IG.ID_ESTADO_FK";
            strSQL += " WHERE U.ID_AREA_FK = " + _configuration.GetSection("Area:Boveda").Value + " AND IG.ID_ESTADO_FK = " + _configuration.GetSection("Estados:Custodiado").Value + " AND TC.ID_TMP_CARRITO IS NULL";
            if (jsontoken.busquedalibre != "")
            {
                strSQL += " AND DESC_CONCAT LIKE '%" + jsontoken.busquedalibre + "%'";
            }
            strSQL += " ORDER BY NUMERO_DE_CAJA";

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



        [HttpPost("agregarcarritodocumento")]
        public IActionResult AgregarCarritoDocumento(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT COUNT(*)";
            strSQL += " FROM (ADMIN.TMP_CARRITO TC LEFT JOIN ADMIN.INVENTARIO_GENERAL IG ON TC.ID_INVENTARIO_GENERAL_FK = IG.ID_INVENTARIO_GENERAL)";
            strSQL += " LEFT JOIN ADMIN.USUARIO U ON U.ID_USUARIO = TC.ID_USUARIO_FK";
            strSQL += " WHERE ID_INVENTARIO_GENERAL_FK = " + jsontoken.idinventario;


            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                int cont = conn.ejecutarQueryEscalar();

                if (cont > 0)
                {
                    conn.cerrar();
                    return Ok("Tomado");
                }
                else
                {
                    strSQL = "INSERT INTO ADMIN.TMP_CARRITO (ID_INVENTARIO_GENERAL_FK, ID_AUX_FK, ID_USUARIO_FK, TIPO, NUMERO_CAJA) VALUES (";
                    strSQL += jsontoken.idinventario + ", " + jsontoken.idaux + ", " + cuenta.IdUser + ", '" + jsontoken.tipocarrito + "', '" + jsontoken.numerocaja + "')";
                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();
                    conn.cerrar();

                    return Ok("OK");
                }

            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("retirardocumento")]
        public IActionResult RetirarDocumento(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT ID_INVENTARIO_GENERAL_FK AS ID, ID_AUX_FK";
            strSQL += " FROM ADMIN.TMP_CARRITO TC LEFT JOIN ADMIN.INVENTARIO_GENERAL IG ON IG.ID_INVENTARIO_GENERAL = TC.ID_INVENTARIO_GENERAL_FK ";
            strSQL += " WHERE TIPO = '" + jsontoken.tipocarrito + "'";
            strSQL += " AND ID_USUARIO_FK = " + cuenta.IdUser + "";

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                dt = conn.llenarDataTable();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET ID_USUARIO_POSEE = " + cuenta.IdUser + ", FECHA_POSEE = TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS') WHERE ID_INVENTARIO_GENERAL = " + row["ID"].ToString() + "";

                        conn.iniciaCommand(strSQL);
                        conn.ejecutarQuery();

                        strSQL = "INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_INVENTARIO_GENERAL_FK, ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, FECHA_INICIO, FECHA_FIN, RECIBIDO, ANULADO) VALUES (" + row["ID"].ToString() + ", " + row["ID_AUX_FK"].ToString() + ", " + cuenta.IdUser + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), 1, 0)";

                        conn.iniciaCommand(strSQL);
                        conn.ejecutarQuery();
                    }
                }

                strSQL = "DELETE FROM ADMIN.TMP_CARRITO WHERE ID_USUARIO_FK = " + cuenta.IdUser + " AND TIPO = '" + jsontoken.tipocarrito + "'";

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


        [HttpPost("buscarretirarcaja")]
        public IActionResult BuscarRetirarCaja(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT U.ID_USUARIO AS ID_BOVEDA, U.NOMBRE_USUARIO AS BOVEDA, NUMERO_DE_CAJA AS CAJA, DEP.NOMBRE_DEPARTAMENTO AS DEPART, DOC.NOMBRE_DOCUMENTO AS DOC, TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, TRIM(DESCRIPCION_1) AS DESC_1, TRIM(DESCRIPCION_2) AS DESC_2, TRIM(DESCRIPCION_3) AS DESC_3, TRIM(DESCRIPCION_4) AS DESC_4, TRIM(DESCRIPCION_5) AS DESC_5";
            strSQL += " FROM (((ADMIN.INVENTARIO_GENERAL IG LEFT JOIN ADMIN.USUARIO U ON U.ID_USUARIO = IG.ID_USUARIO_POSEE)";
            strSQL += " LEFT JOIN ADMIN.TMP_CARRITO TC ON TC.NUMERO_CAJA = IG.NUMERO_DE_CAJA)";
            strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON IG.ID_DEPARTAMENTO_FK = DEP.ID_DEPARTAMENTO)";
            strSQL += " LEFT JOIN ADMIN.LDOCUMENTO DOC ON IG.ID_DOCUMENTO_FK = DOC.ID_DOCUMENTO";
            strSQL += " WHERE U.ID_AREA_FK = " + _configuration.GetSection("Area:Boveda").Value + " AND IG.ID_ESTADO_FK = " + _configuration.GetSection("Estados:Custodiado").Value + " AND TC.ID_TMP_CARRITO IS NULL";
            if (jsontoken.numerocaja != "")
            {
                strSQL += " AND NUMERO_DE_CAJA LIKE '%" + jsontoken.numerocaja + "%'";
            }

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

        [HttpPost("retirarcaja")]
        public IActionResult RetirarCaja(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT ID_INVENTARIO_GENERAL AS ID FROM ADMIN.INVENTARIO_GENERAL WHERE NUMERO_DE_CAJA = '" + jsontoken.numerocaja + "' AND ID_USUARIO_POSEE = " + jsontoken.idaux + ""; 

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();
                dt = conn.llenarDataTable();


                foreach (DataRow row in dt.Rows)
                {
                    strSQL = "INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, ID_INVENTARIO_GENERAL_FK, FECHA_INICIO, FECHA_FIN, RECIBIDO, NUMERO_CAJA, ANULADO) VALUES (" + jsontoken.idaux + ", " + cuenta.IdUser + ", " + row["ID"].ToString() + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), 1, '" + jsontoken.numerocaja + "', 0)";
                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();
                }
                if (dt.Rows.Count > 0)
                {
                    strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET ID_USUARIO_POSEE = " + cuenta.IdUser + ", FECHA_POSEE = TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS') WHERE ID_USUARIO_POSEE = " + jsontoken.idaux + " AND NUMERO_DE_CAJA = '" + jsontoken.numerocaja + "'";
                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();
                }

                strSQL = "DELETE FROM ADMIN.TMP_CARRITO WHERE ID_USUARIO_FK = " + cuenta.IdUser + " AND TIPO = '" + jsontoken.tipocarrito + "'";

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

        [HttpPost("buscarguardardocumento")]
        public IActionResult BuscarGuardarDocumento(Class.JsonToken jsontoken)
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


            string strSQL = "SELECT ID_INVENTARIO_GENERAL AS ID, NUMERO_DE_CAJA AS CAJA, DEP.NOMBRE_DEPARTAMENTO AS DEPART, DOC.NOMBRE_DOCUMENTO AS DOC, TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, DESCRIPCION_1 AS DESC_1, DESCRIPCION_2 AS DESC_2, DESCRIPCION_3 AS DESC_3, DESCRIPCION_4 AS DESC_4, DESCRIPCION_5 AS DESC_5, LE.NOMBRE_ESTADO AS CUSTODIADO, U.NOMBRE_USUARIO AS POSEE, TO_CHAR(FECHA_POSEE, 'dd/MM/yyyy hh:mm:ss') AS FECHA";
            strSQL += " FROM ((((ADMIN.INVENTARIO_GENERAL IG LEFT JOIN (SELECT * FROM ADMIN.TMP_CARRITO WHERE TIPO = '" + jsontoken.tipocarrito + "') TC ON TC.ID_INVENTARIO_GENERAL_FK = IG.ID_INVENTARIO_GENERAL)";
            strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON IG.ID_DEPARTAMENTO_FK = DEP.ID_DEPARTAMENTO)";
            strSQL += " LEFT JOIN ADMIN.LDOCUMENTO DOC ON IG.ID_DOCUMENTO_FK = DOC.ID_DOCUMENTO)";
            strSQL += " LEFT JOIN ADMIN.USUARIO U ON U.ID_USUARIO = IG.ID_USUARIO_POSEE)";
            strSQL += " LEFT JOIN ADMIN.LESTADO LE ON LE.ID_ESTADO = IG.ID_ESTADO_FK";
            strSQL += " WHERE TC.ID_TMP_CARRITO IS NULL AND IG.ID_USUARIO_POSEE = " + cuenta.IdUser + "";

            if (jsontoken.busquedalibre != "")
            {
                strSQL += " AND DESC_CONCAT LIKE '%" + jsontoken.busquedalibre + "%'";
            }
            strSQL += " ORDER BY NUMERO_DE_CAJA";

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

        [HttpPost("guardardocumento")]
        public IActionResult GuardarDocumento(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT TC.ID_INVENTARIO_GENERAL_FK AS ID, '0' AS NRO, DESCRIPCION_1, DESCRIPCION_2, DESCRIPCION_3, DESCRIPCION_4, DESCRIPCION_5 FROM ADMIN.TMP_CARRITO TC";
            strSQL += " LEFT JOIN ADMIN.INVENTARIO_GENERAL IG ON TC.ID_INVENTARIO_GENERAL_FK = IG.ID_INVENTARIO_GENERAL";
            strSQL += " WHERE TIPO = '" + jsontoken.tipocarrito + "' AND TC.ID_USUARIO_FK = " + cuenta.IdUser;

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();
                dt = conn.llenarDataTable();

                foreach (DataRow row in dt.Rows)
                {

                    strSQL = "INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, ID_INVENTARIO_GENERAL_FK, FECHA_INICIO, FECHA_FIN, RECIBIDO, ANULADO) VALUES (" + cuenta.IdUser + ", " + jsontoken.idaux + ", " + row["ID"].ToString() + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), 1, 0)";

                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();

                    strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET ID_USUARIO_POSEE = " + jsontoken.idaux + ", FECHA_POSEE = TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS') WHERE ID_INVENTARIO_GENERAL = " + row["ID"].ToString();

                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();
                }

                strSQL = "DELETE FROM ADMIN.TMP_CARRITO WHERE ID_USUARIO_FK = " + cuenta.IdUser + " AND TIPO = '" + jsontoken.tipocarrito + "'";

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

        [HttpPost("agregarguardardocumento")]
        public IActionResult AgregarGuardarDocumento(Class.JsonToken jsontoken)
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

            string strSQL = "INSERT INTO ADMIN.TMP_CARRITO (ID_INVENTARIO_GENERAL_FK, ID_AUX_FK, ID_USUARIO_FK, TIPO, NUMERO_CAJA) VALUES (";
            strSQL += jsontoken.idinventario + ", " + 0 + ", " + cuenta.IdUser + ", '" + jsontoken.tipocarrito + "', '" + jsontoken.numerocaja + "')";

            Conexion conn = new Conexion();

            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
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


        [HttpPost("buscarguardarcaja")]
        public IActionResult BuscarGuardarCaja(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT DISTINCT NUMERO_DE_CAJA AS CAJA, DEP.NOMBRE_DEPARTAMENTO AS DEPART";
            strSQL += " FROM (((ADMIN.INVENTARIO_GENERAL IG LEFT JOIN (SELECT * FROM ADMIN.TMP_CARRITO WHERE TIPO = '" + jsontoken.tipocarrito + "') TC ON TC.NUMERO_CAJA = IG.NUMERO_DE_CAJA)";
            strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON IG.ID_DEPARTAMENTO_FK = DEP.ID_DEPARTAMENTO)";
            strSQL += " LEFT JOIN ADMIN.LDOCUMENTO DOC ON IG.ID_DOCUMENTO_FK = DOC.ID_DOCUMENTO)";
            strSQL += " WHERE TC.ID_TMP_CARRITO IS NULL AND IG.ID_USUARIO_POSEE = " + cuenta.IdUser + " AND NUMERO_DE_CAJA <> ''";

            if (jsontoken.numerocaja != "")
            {
                strSQL += " AND NUMERO_DE_CAJA LIKE '%" + jsontoken.numerocaja + "%'";
            }

            strSQL += " ORDER BY NUMERO_DE_CAJA";

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


        [HttpPost("guardarcaja")]
        public IActionResult GuardarCaja(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT ID_INVENTARIO_GENERAL AS ID, NUMERO_DE_CAJA, DESCRIPCION_1, DESCRIPCION_2, DESCRIPCION_3, DESCRIPCION_4, DESCRIPCION_5 FROM ADMIN.TMP_CARRITO TC";
            strSQL += " LEFT JOIN ADMIN.INVENTARIO_GENERAL IG ON TC.NUMERO_CAJA = IG.NUMERO_DE_CAJA";
            strSQL += " WHERE TIPO = '" + jsontoken.tipocarrito + "' AND TC.ID_USUARIO_FK = " + cuenta.IdUser + "";

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();
                dt = conn.llenarDataTable();

                foreach (DataRow row in dt.Rows)
                {
                    strSQL = "INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, ID_INVENTARIO_GENERAL_FK, NUMERO_CAJA, FECHA_INICIO, FECHA_FIN, RECIBIDO, ANULADO) VALUES (" + cuenta.IdUser + ", " + jsontoken.idaux + ", " + row["ID"].ToString() + ", '" + row["NUMERO_DE_CAJA"].ToString() + "', TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), 1, 0)";
                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();

                    strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET ID_USUARIO_POSEE = " + jsontoken.idaux + ", FECHA_POSEE = TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS') WHERE NUMERO_DE_CAJA = '" + row["NUMERO_DE_CAJA"].ToString() + "' AND ID_USUARIO_POSEE = " + cuenta.IdUser + "";
                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();
                }

                strSQL = "DELETE FROM ADMIN.TMP_CARRITO WHERE ID_USUARIO_FK = " + cuenta.IdUser + " AND TIPO = '" + jsontoken.tipocarrito + "'";
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
