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
    public class EntregarController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EntregarController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("buscar")]
        public IActionResult Buscar(Class.JsonBody jsonbody)
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                DataTable dt;
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

                string strSQL = @"SELECT ID_INVENTARIO_GENERAL AS ID,TO_CHAR(IG.FECHA_REGISTRO, 'DD/MM/YYYY') AS REGISTRO, EST.NOMBRE_ESTADO AS ESTADO,
                        CASE WHEN UBI.ID_UBICACION = 1 THEN USU.NOMBRE_USUARIO
                             WHEN UBI.ID_UBICACION = 2 THEN USUEX.NOMBRE_USUARIO_EXTERNO
                             ELSE UBI.NOMBRE_UBICACION
                        END AS UBICACION,
                        NUMERO_DE_CAJA AS CAJA,
                        LDEP.NOMBRE_DEPARTAMENTO AS DEPARTAMENTO, LDOC.NOMBRE_DOCUMENTO AS DOCUMENTO, LDET.NOMBRE_DETALLE AS DETALLE,
                        TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA";
                strSQL += " FROM ADMIN.INVENTARIO_GENERAL IG LEFT JOIN ADMIN.TMP_CARRITO TC ON IG.ID_INVENTARIO_GENERAL = TC.ID_INVENTARIO_GENERAL_FK";
                strSQL += @"    LEFT JOIN ADMIN.LDEPARTAMENTO LDEP
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
                strSQL += " WHERE TC.ID_TMP_CARRITO IS NULL";
                strSQL += " AND UBI.PRESTAR = 1 ";

                if (jsonbody.busquedalibre != "")
                    strSQL += " AND DESC_CONCAT LIKE '%" + jsonbody.busquedalibre + "%'";
                if (jsonbody.entransito == 0)
                    strSQL += " AND ID_ESTADO_FK <> " + _configuration.GetSection("Estados:Transito").Value;

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
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

        [HttpPost("entregar")]
        public IActionResult Entregar(Class.JsonBody jsonbody)
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
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    strSQL = "UPDATE ADMIN.INVENTARIO_HISTORICO SET ANULADO = 1 WHERE RECIBIDO = 0 AND ID_INVENTARIO_GENERAL_FK = " + jsonbody.idinventario;
                    conn.Conectar();
                    conn.EjecutarQuery(strSQL);

                    strSQL = @"INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_UBICACION_ENTREGA_FK, ID_UBICACION_RECIBE_FK, ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, ID_INVENTARIO_GENERAL_FK,
                            FECHA_INICIO, OBSERVACION, FECHA_FIN, RECIBIDO, ANULADO, USUARIO, FECHA)
                            VALUES ((SELECT ID_UBICACION_FK FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "), " + jsonbody.idubicacionrecibe + ", " + cuenta.IdUser + ", " + jsonbody.idrecibe + ", " + jsonbody.idinventario + ", TO_DATE('" + jsonbody.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + jsonbody.observacion + "',";
                    strSQL += " TO_DATE('" + jsonbody.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), 1, 0, " + cuenta.IdUser + ", TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS'))";

                    conn.EjecutarQuery(strSQL);

                    strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET ID_ESTADO_FK = " + jsonbody.idestado + ", ID_UBICACION_FK = " + jsonbody.idubicacionrecibe + ", ID_USUARIO_POSEE_FK = " + jsonbody.idrecibe;
                    strSQL += " WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "";

                    conn.EjecutarQuery(strSQL);


                    conn.Cerrar();

                    return Ok();
                }
                catch (Exception ex)
                {
                    conn.Cerrar();
                    return BadRequest(ex.Message + "\n" + strSQL);
                }
            }
            else
            {
                return Unauthorized("No se recibió bearer token");
            }
            
        }
    }
}
