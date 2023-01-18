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
        public IActionResult BuscarEntregar(Class.JsonToken jsontoken)
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
            strSQL += " AND UBI.ID_UBICACION = 1 AND USU.ID_USUARIO = " + cuenta.IdUser;

            if (jsontoken.busquedalibre != "")
                strSQL += " AND DESC_CONCAT LIKE '%" + jsontoken.busquedalibre + "%'";
            if (jsontoken.entransito == 0)
                strSQL += " AND ID_ESTADO_FK <> " + _configuration.GetSection("Estados:Transito").Value;

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

        [HttpPost("entregar")]
        public IActionResult Entregar(Class.JsonToken jsontoken)
        {
            Cuenta cuenta;
            int idestado;
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

            if (jsontoken.idarearecibe == Int32.Parse(_configuration.GetSection("Area:Custodia").Value) || jsontoken.idarearecibe == Int32.Parse(_configuration.GetSection("Area:Administrador").Value))
            {
                idestado = Int32.Parse(_configuration.GetSection("Estados:Custodiado").Value);
            }
            else
            {
                if (!jsontoken.confirmar)
                {
                    idestado = Int32.Parse(_configuration.GetSection("Estados:Prestado").Value);
                }
                else
                {
                    idestado = Int32.Parse(_configuration.GetSection("Estados:Transito").Value);
                }
            }


            string strSQL = "";
            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                strSQL = "UPDATE ADMIN.INVENTARIO_HISTORICO SET ANULADO = 1 WHERE ID_INVENTARIO_GENERAL_FK = " + jsontoken.idinventario;
                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET ID_ESTADO_FK = " + idestado + ", ID_UBICACION_FK = " + jsontoken.idubicacionrecibe;
                if (jsontoken.idubicacionrecibe == Int32.Parse(_configuration.GetSection("Ubicacion:UsuarioExterno").Value) || jsontoken.idubicacionrecibe == Int32.Parse(_configuration.GetSection("Ubicacion:UsuarioInterno").Value))
                {
                    strSQL += ", ID_USUARIO_POSEE_FK = " + cuenta.IdUser;
                }
                else
                {
                    strSQL += ", ID_USUARIO_POSEE_FK = -1";
                }
                strSQL += " WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + "";

                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();
                if (!jsontoken.confirmar)
                {
                    //Prestado
                    strSQL = @"INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, ID_UBICACION_ENTREGA_FK, ID_UBICACION_RECIBE_FK, ID_INVENTARIO_GENERAL_FK,
                            FECHA_INICIO, OBSERVACION, FECHA_FIN, RECIBIDO, ANULADO, USUARIO, FECHA)
                            VALUES (" + jsontoken.identrega + ", " + jsontoken.idrecibe + ", (SELECT ID_UBICACION_FK FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + "), " + jsontoken.idubicacionrecibe + ", " + jsontoken.idinventario  + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + jsontoken.observacion + "',";
                    strSQL += " TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), 1, 0, " + cuenta.IdUser + ", SYSDATE)";
                }
                else
                {
                    //En Transito
                    strSQL = @"INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, ID_AREA_ENTREGA_FK, ID_AREA_RECIBE_FK, ID_INVENTARIO_GENERAL_FK,
                            FECHA_INICIO, OBSERVACION, FECHA_FIN, RECIBIDO, ANULADO, USUARIO, FECHA)
                            VALUES (" + jsontoken.identrega + ", " + jsontoken.idrecibe + ", (SELECT ID_UBICACION_FK FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + "), " + jsontoken.idubicacionrecibe + ", " + jsontoken.idinventario + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + jsontoken.observacion + "',";
                    strSQL += " NULL, 0, 0, " + cuenta.IdUser + ", SYSDATE)";

                }

                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                conn.cerrar();

                return Ok();
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message + "\n" + strSQL);
            }
        }
    }
}
