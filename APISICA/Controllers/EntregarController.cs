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

            string strSQL = "SELECT ID_INVENTARIO_GENERAL AS ID, NUMERO_DE_CAJA AS CAJA, DEP.NOMBRE_DEPARTAMENTO AS DEPART, DOC.NOMBRE_DOCUMENTO AS DOC, TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, DESCRIPCION_1 AS DESC_1, DESCRIPCION_2 AS DESC_2, DESCRIPCION_3 AS DESC_3, DESCRIPCION_4 AS DESC_4, DESCRIPCION_5 AS DESC_5, LE.NOMBRE_ESTADO AS CUSTODIADO, U.NOMBRE_USUARIO AS POSEE, TO_CHAR(FECHA_POSEE, 'dd/MM/yyyy hh:mm:ss') AS FECHA";
            strSQL += " FROM ((((ADMIN.INVENTARIO_GENERAL IG LEFT JOIN ADMIN.TMP_CARRITO TC ON IG.ID_INVENTARIO_GENERAL = TC.ID_INVENTARIO_GENERAL_FK)";
            strSQL += " LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON DEP.ID_DEPARTAMENTO = IG.ID_DEPARTAMENTO_FK)";
            strSQL += " LEFT JOIN ADMIN.LDOCUMENTO DOC ON DOC.ID_DOCUMENTO = IG.ID_DOCUMENTO_FK)";
            strSQL += " LEFT JOIN ADMIN.USUARIO U ON U.ID_USUARIO = IG.ID_USUARIO_POSEE)";
            strSQL += " LEFT JOIN ADMIN.LESTADO LE ON LE.ID_ESTADO = IG.ID_ESTADO_FK";
            strSQL += " WHERE TC.ID_TMP_CARRITO IS NULL";
            strSQL += " AND IG.EXPEDIENTE = " + jsontoken.expediente + " AND ID_USUARIO_POSEE = " + cuenta.IdUser;

            if (jsontoken.busquedalibre != "")
                strSQL += " AND NUMERO_DE_CAJA || DESC_CONCAT LIKE '%" + jsontoken.busquedalibre + "%'";
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
            int identrega, idrecibe, idestado;
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

            identrega = cuenta.IdUser;
            idrecibe = jsontoken.idaux;

            if (jsontoken.idarearecibe == Int32.Parse(_configuration.GetSection("Area:Custodia").Value) || jsontoken.idarearecibe == Int32.Parse(_configuration.GetSection("Area:Administrador").Value))
            {
                idestado = Int32.Parse(_configuration.GetSection("Estados:Custodiado").Value);
            }
            else
            {
                if (!jsontoken.confirmar)
                {
                    idestado = Int32.Parse(_configuration.GetSection("Estados:Transito").Value);
                }
                else
                {
                    idestado = Int32.Parse(_configuration.GetSection("Estados:Prestado").Value);
                }
            }

            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                string strSQL = "UPDATE ADMIN.INVENTARIO_HISTORICO SET ANULADO = 1 WHERE ID_INVENTARIO_GENERAL_FK = " + jsontoken.idinventario;
                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                strSQL = @"INSERT INTO ADMIN.INVENTARIO_HISTORICO (ID_USUARIO_ENTREGA_FK, ID_USUARIO_RECIBE_FK, ID_AREA_ENTREGA_FK, ID_AREA_RECIBE_FK, ID_INVENTARIO_GENERAL_FK, FECHA_INICIO, OBSERVACION, FECHA_FIN, RECIBIDO, ANULADO)
                            VALUES (" + identrega + ", " + idrecibe + ", " + jsontoken.idareaentrega + ", " + jsontoken.idarearecibe + ", " + jsontoken.idinventario + ", TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), '" + jsontoken.observacion + "',";

                if (!jsontoken.confirmar)
                {
                    strSQL += " TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS'), 1, 0)";
                }
                else
                {
                    strSQL += " NULL, 0, 0)";
                }

                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                if (!jsontoken.confirmar)
                {
                    strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET ID_ESTADO_FK = " + idestado + ", ID_USUARIO_POSEE = " + idrecibe + ", FECHA_POSEE = TO_DATE('" + jsontoken.fecha + "', 'YYYY-MM-DD HH24:MI:SS') WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + "";
                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();
                    strSQL = "UPDATE ADMIN.INVENTARIO_HISTORICO SET ANULADO = 1 WHERE RECIBIDO = 0 AND ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + "";
                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();
                }
                else
                {
                    strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET ID_ESTADO_FK = " + Int32.Parse(_configuration.GetSection("Estados:Transito").Value) + " WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + "";
                    conn.iniciaCommand(strSQL);
                    conn.ejecutarQuery();
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
    }
}
