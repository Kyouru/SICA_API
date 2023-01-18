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


        [HttpPost("listaconcat")]
        public IActionResult ListaConcat(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT * FROM ADMIN.LDOCUMENTO WHERE ANULADO = 0 AND ID_DEPARTAMENTO_FK = " + jsontoken.iddepartamento + " ORDER BY ORDEN DESC";

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

        [HttpPost("listadetalle")]
        public IActionResult ListaDetalle(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT ID_DETALLE, NOMBRE_DETALLE FROM ADMIN.LDETALLE WHERE ANULADO = 0 AND ID_DOCUMENTO_FK = " + jsontoken.iddocumento + " ORDER BY ORDEN ASC";

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

        [HttpPost("listaclasificacion")]
        public IActionResult ListaClasificacion(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT ID_CLASIFICACION, NOMBRE_CLASIFICACION FROM ADMIN.LCLASIFICACION WHERE ANULADO = 0 ORDER BY ORDEN ASC";

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

        [HttpPost("listaproducto")]
        public IActionResult ListaProducto(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT ID_PRODUCTO, NOMBRE_PRODUCTO FROM ADMIN.LPRODUCTO WHERE ANULADO = 0 ORDER BY ORDEN ASC";

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
        [HttpPost("listapendientes")]
        public IActionResult ListaPendientes(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT NOMBRE, TIPO FROM ADMIN.LPENDIENTE WHERE ANULADO = 0 ORDER BY TIPO ASC, ORDEN ASC";

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

        [HttpPost("listacentrocosto")]
        public IActionResult ListaCentroCosto(Class.JsonToken jsontoken)
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

            string strSQL = "SELECT ID_CENTRO_COSTO, NOMBRE_CENTRO_COSTO FROM ADMIN.CENTRO_COSTO";

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

            string strSQL = "";;
            if (jsontoken.tiposeleccionarusuario == 1)
            {
                strSQL =  "SELECT UX.ID_USUARIO_EXTERNO, UX.NOMBRE_USUARIO_EXTERNO, UX.EMAIL, UX.NOTIFICAR FROM ADMIN.USUARIOEXTERNO UX ";
                strSQL += "WHERE UX.ANULADO = 0 ORDER BY UX.ORDEN ASC";
            }
            else if (jsontoken.tiposeleccionarusuario == 2)
            {
                strSQL += " AND U.ANULADO = 0 AND ID_AREA = " + _configuration.GetSection("Area:Boveda").Value;
            }
            else if (jsontoken.tiposeleccionarusuario == 3)
            {
                strSQL += " AND ID_AREA <> " + _configuration.GetSection("Area:Administrador").Value;
            }
            else
            {
                strSQL += " AND U.ANULADO = 0 AND U.ID_USUARIO <> " + cuenta.IdUser + " AND A.REAL = 1 AND U.REAL = 1";
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

        [HttpPost("validarubicacion")]
        public IActionResult ValidarUbicacion(Class.JsonToken jsontoken)
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
                return Unauthorized("Sesión no encontrada");
            }

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                string strSQL = "SELECT NVL(ID_UBICACION, 0) FROM UBICACION UBI WHERE UPPER(NOMBRE_UBICACION) = UPPER('" + jsontoken.strubicacion + "') AND ANULADO = 0";
                conn.iniciaCommand(strSQL);
                int i = conn.ejecutarQueryEscalar();
                conn.cerrar();

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
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("obtenercaja")]
        public IActionResult ObtenerCaja(Class.JsonToken jsontoken)
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
                strSQL += " WHERE NUMERO_DE_CAJA = '" + jsontoken.numerocaja + "'";

                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                dt = conn.llenarDataTable();
                conn.cerrar();

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
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }
    }
}
