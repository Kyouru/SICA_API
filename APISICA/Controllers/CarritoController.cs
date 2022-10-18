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
    public class CarritoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CarritoController(IConfiguration configuration)
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
            string strSQL = "";
            if (jsontoken.tipocarrito == _configuration.GetSection("TipoCarrito:RecibirPagare").Value || jsontoken.tipocarrito == _configuration.GetSection("TipoCarrito:EntregarPagare").Value)
            {
                strSQL = "SELECT ID_TMP_CARRITO AS ID, SOLICITUD_SISGO, DESCRIPCION_3, DESCRIPCION_4, DESCRIPCION_5";
                strSQL += " FROM ADMIN.PAGARE PA LEFT JOIN ADMIN.TMP_CARRITO TC ON TC.ID_AUX_FK = PA.ID_PAGARE";
                strSQL += " WHERE TC.TIPO = '" + jsontoken.tipocarrito + "'";
                strSQL += " AND TC.ID_USUARIO_FK = " + cuenta.IdUser;
            }
            else if (jsontoken.tipocarrito == _configuration.GetSection("TipoCarrito:VerificarCaja").Value)
            {
                strSQL = @"SELECT NUMERO_DE_CAJA, DESCRIPCION_1, DESCRIPCION_2, DESCRIPCION_3, DESCRIPCION_4, DESCRIPCION_5, U.NOMBRE_USUARIO, TO_CHAR(FECHA_POSEE, 'dd/MM/yyyy hh:mm:ss') AS FECHA
                            FROM ADMIN.INVENTARIO_GENERAL IG
                            LEFT JOIN ADMIN.USUARIO U ON U.ID_USUARIO = IG.ID_USUARIO_POSEE
                            WHERE NUMERO_DE_CAJA = '" + jsontoken.numerocaja + "' AND ID_USUARIO_POSEE <> " + cuenta.IdUser + "";
            }
            else
            {
                strSQL = "SELECT ID_TMP_CARRITO AS ID, NUMERO_CAJA, TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, DESCRIPCION_1 AS DESC_1, DESCRIPCION_2 AS DESC_2, DESCRIPCION_3 AS DESC_3, DESCRIPCION_4 AS DESC_4, DESCRIPCION_5 AS DESC_5, LE.NOMBRE_ESTADO AS CUSTODIADO, U.NOMBRE_USUARIO AS POSEE, TO_CHAR(FECHA_POSEE, 'dd/MM/yyyy hh:mm:ss') AS FECHA";
                strSQL += " FROM ((ADMIN.TMP_CARRITO TC LEFT JOIN ADMIN.INVENTARIO_GENERAL IG ON IG.ID_INVENTARIO_GENERAL = TC.ID_INVENTARIO_GENERAL_FK)";
                strSQL += " LEFT JOIN ADMIN.LESTADO LE ON LE.ID_ESTADO = IG.ID_ESTADO_FK)";
                strSQL += " LEFT JOIN ADMIN.USUARIO U ON U.ID_USUARIO = IG.ID_USUARIO_POSEE";
                strSQL += " WHERE TC.TIPO = '" + jsontoken.tipocarrito + "' AND TC.ID_USUARIO_FK = " + cuenta.IdUser;
                strSQL += " ORDER BY NUMERO_CAJA";
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

        [HttpPost("eliminar")]
        public IActionResult Eliminar(Class.JsonToken jsontoken)
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

            string strSQL = "DELETE FROM ADMIN.TMP_CARRITO WHERE ID_TMP_CARRITO = " + jsontoken.idaux;

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

        [HttpPost("cantidadcarrito")]
        public IActionResult CantidadCarrito(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT COUNT(*) FROM ADMIN.TMP_CARRITO WHERE TIPO = '" + jsontoken.tipocarrito + "' AND ID_USUARIO_FK = " + cuenta.IdUser;

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                int n = conn.ejecutarQueryEscalar();
                conn.cerrar();
                return Ok(n.ToString());
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("verificarcarrito")]
        public IActionResult VerificarCarrito(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT COUNT(*) FROM ADMIN.INVENTARIO_GENERAL WHERE NUMERO_DE_CAJA = '" + jsontoken.numerocaja + "' AND ID_USUARIO_POSEE <> " + cuenta.IdUser;

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                int i = conn.ejecutarQueryEscalar();
                conn.cerrar();
                if (i > 0)
                {
                    return Ok(false);
                }
                else
                {
                    return Ok(true);
                }
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("limpiarcarrito")]
        public IActionResult LimpiarCarrito(Class.JsonToken jsontoken)
        {
            Conexion conn = new Conexion();
            try
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

                string strSQL = "DELETE FROM ADMIN.TMP_CARRITO WHERE ID_USUARIO_FK = " + cuenta.IdUser;
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

        [HttpPost("obtenercarrito")]
        public IActionResult ObternerCarrito(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT TC.ID_INVENTARIO_GENERAL_FK AS ID, ROW_NUMBER() OVER(ORDER BY ID_INVENTARIO_GENERAL) AS NRO, DESCRIPCION_1 AS DEFINICION, DESCRIPCION_2 AS SOLICITUD, DESCRIPCION_3 AS COD_PRESTAMO, DESCRIPCION_4 AS NOMBRE_SOCIO";
            strSQL += " FROM ADMIN.TMP_CARRITO TC LEFT JOIN ADMIN.INVENTARIO_GENERAL IG ON TC.ID_INVENTARIO_GENERAL_FK = IG.ID_INVENTARIO_GENERAL";
            strSQL += " WHERE TIPO = '" + jsontoken.tipocarrito + "' AND ID_USUARIO_FK = " + cuenta.IdUser;

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

        [HttpPost("agregarcarrito")]
        public IActionResult AgregarCarrito(Class.JsonToken jsontoken)
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

            string strSQL;

                strSQL = "INSERT INTO ADMIN.TMP_CARRITO (ID_INVENTARIO_GENERAL_FK, ID_AUX_FK, ID_USUARIO_FK, TIPO, NUMERO_CAJA) VALUES (";
                strSQL += jsontoken.idinventario + ", " + jsontoken.idaux + ", " + cuenta.IdUser + ", '" + jsontoken.tipocarrito + "', '" + jsontoken.numerocaja + "')";
            

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

    }
}
