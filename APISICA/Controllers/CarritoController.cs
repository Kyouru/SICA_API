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
                strSQL = "SELECT ID_TMP_CARRITO AS ID, NUMEROSOLICITUD, CODIGO_SOCIO, NOMBRE_SOCIO";
                strSQL += " FROM ADMIN.PAGARE PA LEFT JOIN ADMIN.TMP_CARRITO TC ON TC.ID_AUX_FK = PA.ID_PAGARE";
                strSQL += " WHERE TC.TIPO = '" + jsontoken.tipocarrito + "'";
                strSQL += " AND TC.ID_USUARIO_FK = " + cuenta.IdUser;
            }
            else if (jsontoken.tipocarrito == _configuration.GetSection("TipoCarrito:VerificarCaja").Value)
            {
                strSQL = @"SELECT NUMERO_DE_CAJA,
                        CASE WHEN UBI.ID_UBICACION = 1 THEN USU.NOMBRE_USUARIO
                             WHEN UBI.ID_UBICACION = 2 THEN USUEX.NOMBRE_USUARIO_EXTERNO
                             ELSE UBI.NOMBRE_UBICACION
                        END AS UBICACION,
                        LDEP.NOMBRE_DEPARTAMENTO AS DEPARTAMENTO, LDOC.NOMBRE_DOCUMENTO AS DOCUMENTO, LDET.NOMBRE_DETALLE AS DETALLE,
                        TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA,
                        LCLA.NOMBRE_CLASIFICACION AS CLASIFICACION, OBSERVACION, CC.NOMBRE_CENTRO_COSTO AS CENTRO_COSTO, LPRO.NOMBRE_PRODUCTO AS PRODUCTO
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
                            WHERE NUMERO_DE_CAJA = '" + jsontoken.numerocaja + "' AND ID_USUARIO_POSEE_FK <> " + cuenta.IdUser + "";
            }
            else
            {
                strSQL = @"SELECT ID_TMP_CARRITO AS ID, TO_CHAR(IG.FECHA_REGISTRO, 'DD/MM/YYYY') AS REGISTRO, EST.NOMBRE_ESTADO AS ESTADO,
                        CASE WHEN UBI.ID_UBICACION = 1 THEN USU.NOMBRE_USUARIO
                             WHEN UBI.ID_UBICACION = 2 THEN USUEX.NOMBRE_USUARIO_EXTERNO
                             ELSE UBI.NOMBRE_UBICACION
                        END AS UBICACION,
                        IG.NUMERO_DE_CAJA AS CAJA, LDEP.NOMBRE_DEPARTAMENTO AS DEPARTAMENTO, LDOC.NOMBRE_DOCUMENTO AS DOCUMENTO, LDET.NOMBRE_DETALLE AS DETALLE,
                        TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, LCLA.NOMBRE_CLASIFICACION AS CLASIFICACION, OBSERVACION, CC.NOMBRE_CENTRO_COSTO AS CENTRO_COSTO, LPRO.NOMBRE_PRODUCTO AS PRODUCTO";
                strSQL += " FROM ADMIN.TMP_CARRITO TC LEFT JOIN ADMIN.INVENTARIO_GENERAL IG ON IG.ID_INVENTARIO_GENERAL = TC.ID_INVENTARIO_GENERAL_FK";
                strSQL += @" LEFT JOIN ADMIN.LDEPARTAMENTO LDEP
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
                strSQL += " WHERE TC.TIPO = '" + jsontoken.tipocarrito + "' AND TC.ID_USUARIO_FK = " + cuenta.IdUser;
                strSQL += " ORDER BY NUMERO_DE_CAJA";
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

            string strSQL = @"SELECT TC.ID_INVENTARIO_GENERAL_FK AS ID, ROW_NUMBER() OVER(ORDER BY ID_INVENTARIO_GENERAL) AS NRO, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD,
                                LDEP.NOMBRE_DEPARTAMENTO AS DEP, LDOC.NOMBRE_DOCUMENTO AS DOC, LDET.NOMBRE_DETALLE AS DET, LPRO.NOMBRE_PRODUCTO AS PRODUCTO, CC.NOMBRE_CENTRO_COSTO AS CENTRO_COSTO, LCLA.NOMBRE_CLASIFICACION AS CLASIFICACION";
            strSQL += @" FROM ADMIN.TMP_CARRITO TC LEFT JOIN ADMIN.INVENTARIO_GENERAL IG ON TC.ID_INVENTARIO_GENERAL_FK = IG.ID_INVENTARIO_GENERAL
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
                            LEFT JOIN ADMIN.CENTRO_COSTO CC
                                ON IG.ID_CENTRO_COSTO_FK = CC.ID_CENTRO_COSTO";
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
