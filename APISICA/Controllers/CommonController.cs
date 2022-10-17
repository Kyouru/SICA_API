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
    public class CommonController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CommonController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost("listadepartamento")]
        public IActionResult ListaDepartamento(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT * FROM ADMIN.LDEPARTAMENTO WHERE ANULADO = 0 ORDER BY ORDEN DESC";

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

        [HttpPost("listadocumento")]
        public IActionResult ListaDocumento(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT * FROM ADMIN.LDOCUMENTO WHERE ANULADO = 0 ORDER BY ORDEN DESC";

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

        [HttpPost("listadescripcion")]
        public IActionResult ListaDescripcion(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT ID_DESCRIPCION1, TIPO_DESCRIPCION1, NOMBRE_DESCRIPCION1 FROM ADMIN.LDESCRIPCION1 ORDER BY ORDEN ASC";

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

        [HttpPost("listausuarios")]
        public IActionResult ListaUsuarios(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT U.ID_USUARIO, U.NOMBRE_USUARIO, A.ID_AREA, A.NOMBRE_AREA FROM ADMIN.USUARIO U LEFT JOIN ADMIN.AREA A ON U.ID_AREA_FK = A.ID_AREA WHERE ANULADO = 0 AND U.ID_USUARIO <> " + cuenta.IdUser;
            if (jsontoken.tiposeleccionarusuario == 1)
            {
                strSQL += " AND A.REAL = 1 AND U.REAL = 1 AND ID_AREA <> " + _configuration.GetSection("Area:Custodia").Value;
            }
            if (jsontoken.tiposeleccionarusuario == 2)
            {
                strSQL += " AND ID_AREA = " + _configuration.GetSection("Area:Boveda").Value;
            }
            else
            {
                strSQL += " AND A.REAL = 1 AND U.REAL = 1";
            }
            strSQL += " ORDER BY A.ORDEN, U.ORDEN";

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

        [HttpPost("pendienteconfirmarrecepcion")]
        public IActionResult pendienteConfirmarRecepcion(Class.JsonToken jsontoken)
        {
            Cuenta cuenta;
            try
            {
                cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("Usercheck"), jsontoken.token);
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
                conn.conectar();
                conn.iniciaCommand(strSQL);
                int i = conn.ejecutarQueryEscalar();
                conn.cerrar();
                return Ok(i.ToString());
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }
    }
}
