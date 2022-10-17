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
    public class PagareController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PagareController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("buscar")]
        public IActionResult Buscar(Class.JsonToken jsontoken)
        {
            DataTable dt = new DataTable("Pagares");
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
                string strSQL = "SELECT SOLICITUD_SISGO, DESCRIPCION_3, DESCRIPCION_4, DESCRIPCION_5, U.NOMBRE_USUARIO";
                strSQL += " FROM ADMIN.PAGARE PA LEFT JOIN ADMIN.USUARIO U ON U.ID_USUARIO = PA.ID_USUARIO_POSEE";
                strSQL += " WHERE 1 = 1";

                if (jsontoken.busquedalibre != "")
                {
                    strSQL += " AND CONCAT LIKE '%" + jsontoken.busquedalibre + "%'";
                }
                strSQL += " ORDER BY SOLICITUD_SISGO DESC";

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


        [HttpPost("buscarentregar")]
        public IActionResult BuscarEntregar(Class.JsonToken jsontoken)
        {
            DataTable dt = new DataTable("Pagares");
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
                string strSQL = "SELECT ID_PAGARE, SOLICITUD_SISGO AS SOLICITUD, DESCRIPCION_3 AS CODIGO, DESCRIPCION_4 AS NOMBRE, DESCRIPCION_5";
                strSQL += " FROM (ADMIN.PAGARE PA LEFT JOIN (SELECT * FROM ADMIN.USUARIO WHERE ID_AREA_FK = " + _configuration.GetSection("Area:Custodia").Value + " OR ID_AREA_FK = " + _configuration.GetSection("Area:Administrador").Value + ") U ON U.ID_USUARIO = PA.ID_USUARIO_POSEE)";
                strSQL += " LEFT JOIN ADMIN.TMP_CARRITO TC ON TC.ID_AUX_FK = PA.ID_PAGARE";
                strSQL += " WHERE TC.ID_TMP_CARRITO IS NULL";
                strSQL += " AND U.ID_USUARIO IS NOT NULL";

                if (jsontoken.busquedalibre != "")
                {
                    strSQL += " AND CONCAT LIKE '%" + jsontoken.busquedalibre + "%'";
                }

                strSQL += " ORDER BY SOLICITUD_SISGO DESC";

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

        [HttpPost("entregar")]
        public IActionResult Entregar(Class.JsonToken jsontoken)
        {
            DataTable dt = new DataTable("Pagares");
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
                string strSQL = "SELECT TC.ID_AUX_FK, SOLICITUD_SISGO, DESCRIPCION_3, DESCRIPCION_4, DESCRIPCION_5, U.NOMBRE_USUARIO";
                strSQL += " FROM (ADMIN.TMP_CARRITO TC LEFT JOIN ADMIN.PAGARE PA ON TC.ID_AUX_FK = PA.ID_PAGARE)";
                strSQL += " LEFT JOIN ADMIN.USUARIO U ON U.ID_USUARIO = PA.ID_USUARIO_POSEE";
                strSQL += " WHERE TIPO = '" + _configuration.GetSection("TipoCarrito:EntregarPagare").Value + "' AND ID_USUARIO_FK = " + cuenta.IdUser;

                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();
                dt = conn.llenarDataTable();

                string recibido = "0";
                if (jsontoken.idarearecibe == Int32.Parse(_configuration.GetSection("Area:Custodia").Value))
                {
                    recibido = "0";
                }
                else
                {
                    recibido = "0";
                }

                foreach (DataRow row in dt.Rows)
                {
                    strSQL = "INSERT INTO ADMIN.PAGARE_HISTORICO (ID_PAGARE_FK, ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, FECHA_INICIO, FECHA_FIN, OBSERVACION_ENTREGA, RECIBIDO, ANULADO) VALUES (";
                    strSQL += row["ID_AUX_FK"].ToString() + ", " + cuenta.IdUser + ", " + jsontoken.idaux + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + jsontoken.observacion + "', " + recibido + ", 0)";

                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();

                    strSQL = "UPDATE ADMIN.PAGARE SET ID_USUARIO_POSEE = " + jsontoken.idaux + ", FECHA_POSEE = TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS') WHERE ID_PAGARE = " + row["ID_AUX_FK"].ToString();

                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();
                }

                strSQL = "DELETE FROM ADMIN.TMP_CARRITO WHERE ID_USUARIO_FK = " + cuenta.IdUser + " AND TIPO = '" + _configuration.GetSection("TipoCarrito:EntregarPagare").Value + "'";

                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();


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


        [HttpPost("registrar")]
        public IActionResult Registrar(Class.JsonToken jsontoken)
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
                string strSQL = "SELECT COUNT(*) FROM ADMIN.PAGARE WHERE SOLICITUD_SISGO = '" + jsontoken.descripcion1 + "'";

                conn.conectar();
                conn.iniciaCommand(strSQL);
                int cont = conn.ejecutarQueryEscalar();

                if (cont > 0)
                {
                    conn.cerrar();
                    return Ok("Duplicado");
                }
                else
                {
                    strSQL = "INSERT INTO ADMIN.PAGARE (SOLICITUD_SISGO, CODIGO_SOCIO, DESCRIPCION_3, DESCRIPCION_4, ID_USUARIO_POSEE, CONCAT) ";
                    strSQL += "VALUES (";
                    strSQL += "'" + jsontoken.descripcion1 + "', ";

                    strSQL += "'" + jsontoken.descripcion2 + "', ";
                    strSQL += "'" + jsontoken.descripcion2 + "', ";
                    strSQL += "'" + jsontoken.descripcion3 + "', ";
                    strSQL += "" + cuenta.IdUser + ", ";

                    strSQL += "'" + jsontoken.descripcion1 + ";" + jsontoken.descripcion2 + ";" + jsontoken.descripcion3 + ";')";
                    strSQL += " RETURNING ID_PAGARE INTO :numero";
                    int lastinsertid = conn.InsertReturnID(strSQL);

                    strSQL = "INSERT INTO ADMIN.PAGARE_HISTORICO (ID_PAGARE_FK, ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, FECHA_INICIO, FECHA_FIN, OBSERVACION_RECIBE, RECIBIDO, ANULADO) VALUES (";
                    strSQL += lastinsertid + ", " + jsontoken.idaux + ", " + cuenta.IdUser + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + jsontoken.observacion + "', 1, 0)";

                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();

                    conn.cerrar();

                    return Ok("Nuevo");
                }
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("buscarrecibir")]
        public IActionResult BuscarRecibir(Class.JsonToken jsontoken)
        {
            DataTable dt = new DataTable("Pagares");
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
                string strSQL = "SELECT ID_PAGARE, SOLICITUD_SISGO, DESCRIPCION_3 AS CODIGO, DESCRIPCION_4 AS NOMBRE, DESCRIPCION_5";
                strSQL += " FROM (ADMIN.PAGARE PA LEFT JOIN (SELECT * FROM ADMIN.USUARIO WHERE ID_AREA_FK <> " + _configuration.GetSection("Area:Custodia").Value + ") U ON U.ID_USUARIO = PA.ID_USUARIO_POSEE)";
                strSQL += " LEFT JOIN ADMIN.TMP_CARRITO TC ON TC.ID_AUX_FK = PA.ID_PAGARE";
                strSQL += " WHERE TC.ID_TMP_CARRITO IS NULL";
                strSQL += " AND U.ID_USUARIO IS NOT NULL";
                strSQL += " ORDER BY SOLICITUD_SISGO DESC";

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
            DataTable dt = new DataTable("");
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
                string strSQL = "SELECT TC.ID_AUX_FK, SOLICITUD_SISGO, DESCRIPCION_3, DESCRIPCION_4, DESCRIPCION_5, U.NOMBRE_USUARIO";
                strSQL += " FROM (ADMIN.TMP_CARRITO TC LEFT JOIN ADMIN.PAGARE PA ON TC.ID_AUX_FK = PA.ID_PAGARE)";
                strSQL += " LEFT JOIN ADMIN.USUARIO U ON U.ID_USUARIO = PA.ID_USUARIO_POSEE";
                strSQL += " WHERE TIPO = '" + _configuration.GetSection("TipoCarrito:RecibirPagare").Value + "' AND ID_USUARIO_FK = " + cuenta.IdUser; 

                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();
                dt = conn.llenarDataTable();

                foreach (DataRow row in dt.Rows) 
                {
                    strSQL = "INSERT INTO ADMIN.PAGARE_HISTORICO (ID_PAGARE_FK, ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, FECHA_INICIO, FECHA_FIN, OBSERVACION_RECIBE, RECIBIDO, ANULADO) VALUES (";
                    strSQL += row["ID_AUX_FK"].ToString() + ", " + jsontoken.idaux + ", " + cuenta.IdUser + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + jsontoken.observacion + "', 1, 0)";

                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();

                    strSQL = "UPDATE ADMIN.PAGARE SET ID_USUARIO_POSEE = " + cuenta.IdUser + ", FECHA_POSEE = TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS') WHERE ID_PAGARE = " + row["ID_AUX_FK"].ToString();

                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();
                }

                strSQL = "DELETE FROM ADMIN.TMP_CARRITO WHERE ID_USUARIO_FK = " + cuenta.IdUser + " AND TIPO = '" + _configuration.GetSection("TipoCarrito:RecibirPagare").Value + "'";

                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();


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
