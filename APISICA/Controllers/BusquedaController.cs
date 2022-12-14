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
                strSQL = @"SELECT ID_INVENTARIO_GENERAL AS ID, TO_CHAR(IG.FECHA_REGISTRO, 'DD/MM/YYYY') AS REGISTRO, EST.NOMBRE_ESTADO AS ESTADO,
                        CASE WHEN UBI.ID_UBICACION = 1 THEN USU.NOMBRE_USUARIO
                             WHEN UBI.ID_UBICACION = 2 THEN USUEX.NOMBRE_USUARIO_EXTERNO
                             ELSE UBI.NOMBRE_UBICACION
                        END AS UBICACION,
                        IG.NUMERO_DE_CAJA AS CAJA, LDEP.NOMBRE_DEPARTAMENTO AS DEPARTAMENTO, 
                        TO_CHAR(IG.FECHA_DESDE, 'DD/MM/YYYY') AS DESDE, TO_CHAR(IG.FECHA_HASTA, 'DD/MM/YYYY') AS HASTA, LDOC.NOMBRE_DOCUMENTO AS DOCUMENTO, LDET.NOMBRE_DETALLE AS DETALLE,
                        NUMEROSOLICITUD, CODIGO_SOCIO AS CODIGO, NOMBRE_SOCIO AS NOMBRE, LCLA.NOMBRE_CLASIFICACION AS CLASIFICACION, OBSERVACION,
                        CC.NOMBRE_CENTRO_COSTO AS CENTRO_COSTO, LPRO.NOMBRE_PRODUCTO AS PRODUCTO, TO_CHAR(IG.FECHA_MODIFICA, 'DD/MM/YYYY HH24:MI:SS') AS MODIFICA
                        FROM ADMIN.INVENTARIO_GENERAL IG
                            LEFT JOIN LDEPARTAMENTO LDEP
                                ON IG.ID_DEPARTAMENTO_FK = LDEP.ID_DEPARTAMENTO
                            LEFT JOIN LDOCUMENTO LDOC
                                ON IG.ID_DOCUMENTO_FK = LDOC.ID_DOCUMENTO
                            LEFT JOIN LDETALLE LDET
                                ON IG.ID_DETALLE_FK = LDET.ID_DETALLE
                            LEFT JOIN LCLASIFICACION LCLA
                                ON IG.ID_CLASIFICACION_FK = LCLA.ID_CLASIFICACION
                            LEFT JOIN LPRODUCTO LPRO
                                ON IG.ID_PRODUCTO_FK = LPRO.ID_PRODUCTO
                            LEFT JOIN UBICACION UBI
                                ON IG.ID_UBICACION_FK = UBI.ID_UBICACION
                            LEFT JOIN USUARIO USU
                                ON IG.ID_USUARIO_POSEE_FK = USU.ID_USUARIO
                            LEFT JOIN USUARIO_EXTERNO USUEX
                                ON IG.ID_USUARIO_POSEE_FK = USUEX.ID_USUARIO_EXTERNO
                            LEFT JOIN CENTRO_COSTO CC
                                ON IG.ID_CENTRO_COSTO_FK = CC.ID_CENTRO_COSTO
                            LEFT JOIN LESTADO EST
                                ON IG.ID_ESTADO_FK = EST.ID_ESTADO";
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
                return BadRequest(ex.Message + "\n" + strSQL);
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
                return BadRequest(ex.Message + "\n" + strSQL);
            }
        }

        [HttpPost("guardareditar")]
        public IActionResult GuardarEditar(Class.JsonToken jsontoken)
        {
            string status;
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
            status = "Insert INVENTARIO_ANTERIOR";

            string strSQL = @"INSERT INTO ADMIN.INVENTARIO_ANTERIOR (ID_INVENTARIO_GENERAL_FK, NUMERO_DE_CAJA, ID_DEPARTAMENTO_FK, ID_DOCUMENTO_FK, ID_DETALLE_FK, FECHA_DESDE, FECHA_HASTA,
                    ID_CLASIFICACION_FK, ID_PRODUCTO_FK, ID_CENTRO_COSTO_FK, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD, OBSERVACION, ID_USUARIO_REGISTRA_FK, FECHA_MODIFICA)
                                                                SELECT  ID_INVENTARIO_GENERAL, NUMERO_DE_CAJA, ID_DEPARTAMENTO_FK, ID_DOCUMENTO_FK, ID_DETALLE_FK, FECHA_DESDE, FECHA_HASTA,
                    ID_CLASIFICACION_FK, ID_PRODUCTO_FK, ID_CENTRO_COSTO_FK, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD, OBSERVACION, ID_USUARIO_REGISTRA_FK, FECHA_MODIFICA
                    FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario;

            Conexion conn = new Conexion();

            try
            {
                conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                status = "UPDATE INVENTARIO_GENERAL";
                strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET";
                strSQL += " NUMERO_DE_CAJA = '" + jsontoken.numerocaja + "', ID_DETALLE_FK = " + jsontoken.iddetalle + ", ID_DEPARTAMENTO_FK = " + jsontoken.iddepartamento + ", ID_DOCUMENTO_FK = " + jsontoken.iddocumento + ",";
                strSQL += " ID_CLASIFICACION_FK = " + jsontoken.idclasificacion + ", ID_PRODUCTO_FK = " + jsontoken.idproducto + ", ID_UBICACION_FK = " + jsontoken.idubicacion + ", ID_CENTRO_COSTO_FK = " + jsontoken.idcentrocosto + ",";
                strSQL += " ID_USUARIO_REGISTRA_FK = " + cuenta.IdUser + ", CODIGO_SOCIO = '" + jsontoken.codigosocio + "', NOMBRE_SOCIO = '" + jsontoken.nombresocio + "', NUMEROSOLICITUD = '" + jsontoken.numerosolicitud + "',";
                strSQL += " OBSERVACION = '" + jsontoken.observacion + "',";

                if (jsontoken.fechadesde != "")
                    strSQL += " FECHA_DESDE = TO_DATE('" + jsontoken.fechadesde + "', 'YYYY-MM-DD HH24:MI:SS'),";
                else
                    strSQL += " FECHA_DESDE = NULL,";
                if (jsontoken.fechahasta != "")
                    strSQL += " FECHA_HASTA = TO_DATE('" + jsontoken.fechahasta + "', 'YYYY-MM-DD HH24:MI:SS'),";
                else
                    strSQL += " FECHA_HASTA = NULL,";
                if (jsontoken.fechamodifica != "")
                    strSQL += " FECHA_MODIFICA = TO_DATE('" + jsontoken.fechamodifica + "', 'YYYY-MM-DD HH24:MI:SS')";
                else
                    strSQL += " FECHA_MODIFICA = NULL";
                strSQL += " WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario;

                //conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                //conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                status = "UPDATE INVENTARIO_GENERAL DESCCONCAT";
                strSQL = @"UPDATE ADMIN.INVENTARIO_GENERAL IG
                        SET DESC_CONCAT =   UPPER(
                        TO_CHAR(FECHA_REGISTRO, 'DD/MM/YYYY') || ';' ||
                        (SELECT NOMBRE_ESTADO FROM LESTADO WHERE ID_ESTADO = IG.ID_ESTADO_FK) || ';' ||
                        NVL(TO_CHAR(FECHA_MODIFICA, 'DD/MM/YYYY'), 'NULO') || ';' ||
                        NVL((SELECT NOMBRE_DEPARTAMENTO FROM LDEPARTAMENTO WHERE ID_DEPARTAMENTO = IG.ID_DEPARTAMENTO_FK), 'NULO') || ';' ||
                        NVL(TO_CHAR(FECHA_DESDE, 'DD/MM/YYYY'), 'NULO') || ';' ||
                        NVL(TO_CHAR(FECHA_HASTA, 'DD/MM/YYYY'), 'NULO') || ';' ||
                        NVL((SELECT NOMBRE_DOCUMENTO FROM LDOCUMENTO WHERE ID_DOCUMENTO = IG.ID_DOCUMENTO_FK), 'NULO') || ';' ||
                        NVL((SELECT NOMBRE_DETALLE FROM LDETALLE WHERE ID_DETALLE = IG.ID_DETALLE_FK), 'NULO') || ';' ||
                        NVL(NUMEROSOLICITUD, 'NULO') || ';' ||
                        NVL(CODIGO_SOCIO, 'NULO') || ';' ||
                        NVL(NOMBRE_SOCIO, 'NULO') || ';' ||
                        NVL((SELECT NVL(NOMBRE_CLASIFICACION, 'NULO') FROM LCLASIFICACION WHERE ID_CLASIFICACION = IG.ID_CLASIFICACION_FK), 'NULO') || ';' ||
                        NVL(OBSERVACION, 'NULO') || ';' ||
                        NVL((SELECT NOMBRE_UBICACION FROM UBICACION WHERE ID_UBICACION = IG.ID_UBICACION_FK), 'NULO') || ';' ||
                        NVL((SELECT NOMBRE_CENTRO_COSTO FROM CENTRO_COSTO WHERE ID_CENTRO_COSTO = IG.ID_CENTRO_COSTO_FK), 'NULO') || ';' ||
                        NVL((SELECT NOMBRE_PRODUCTO FROM LPRODUCTO WHERE ID_PRODUCTO = IG.ID_PRODUCTO_FK), 'NULO')
                    ) WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario;
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();

                conn.cerrar();
                
                return Ok();
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message + " " + status + " " + strSQL);
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
            string strSQL = @"SELECT IG.ID_INVENTARIO_GENERAL AS ID, TO_CHAR(IG.FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(IG.FECHA_HASTA, 'dd/MM/yyyy') AS HASTA,
                                LDEP.NOMBRE_DEPARTAMENTO, LDOC.NOMBRE_DOCUMENTO, LDET.NOMBRE_DETALLE,
                                IG.NUMERO_DE_CAJA, IG.CODIGO_SOCIO, IG.NOMBRE_SOCIO, IG.NUMEROSOLICITUD,
                                CASE WHEN UB1.ID_UBICACION = 1 THEN U1.NOMBRE_USUARIO
                                     WHEN UB1.ID_UBICACION = 2 THEN UE1.NOMBRE_USUARIO_EXTERNO
                                     ELSE UB1.NOMBRE_UBICACION
                                END AS ENTREGA,
                                CASE WHEN UB2.ID_UBICACION = 1 THEN U2.NOMBRE_USUARIO
                                     WHEN UB2.ID_UBICACION = 2 THEN UE2.NOMBRE_USUARIO_EXTERNO
                                     ELSE UB2.NOMBRE_UBICACION
                                END AS RECIBE,
                                TO_CHAR(IH.FECHA_INICIO, 'dd/MM/yyyy HH24:MI:SS') AS FECHA_ENTREGA,
                                TO_CHAR(IH.FECHA_FIN, 'dd/MM/yyyy HH24:MI:SS') AS FECHA_RECIBE
                                FROM ADMIN.INVENTARIO_HISTORICO IH LEFT JOIN ADMIN.USUARIO U1 ON IH.ID_USUARIO_ENTREGA_FK = U1.ID_USUARIO
                                    LEFT JOIN ADMIN.USUARIO U2 ON IH.ID_USUARIO_RECIBE_FK = U2.ID_USUARIO
                                    LEFT JOIN ADMIN.USUARIO_EXTERNO UE1 ON IH.ID_USUARIO_ENTREGA_FK = UE1.ID_USUARIO_EXTERNO
                                    LEFT JOIN ADMIN.USUARIO_EXTERNO UE2 ON IH.ID_USUARIO_RECIBE_FK = UE2.ID_USUARIO_EXTERNO
                                    LEFT JOIN ADMIN.UBICACION UB1 ON IH.ID_UBICACION_ENTREGA_FK = UB1.ID_UBICACION
                                    LEFT JOIN ADMIN.UBICACION UB2 ON IH.ID_UBICACION_RECIBE_FK = UB2.ID_UBICACION
                                    LEFT JOIN ADMIN.INVENTARIO_GENERAL IG ON IG.ID_INVENTARIO_GENERAL = IH.ID_INVENTARIO_GENERAL_FK
                                    LEFT JOIN LDEPARTAMENTO LDEP ON IG.ID_DEPARTAMENTO_FK = LDEP.ID_DEPARTAMENTO
                                    LEFT JOIN LDOCUMENTO LDOC ON IG.ID_DOCUMENTO_FK = LDOC.ID_DOCUMENTO
                                    LEFT JOIN LDETALLE LDET ON IG.ID_DETALLE_FK = LDET.ID_DETALLE
                                WHERE IH.ANULADO = 0 AND IH.RECIBIDO = 1
                                        AND IG.ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + " ORDER BY IH.FECHA_INICIO";

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
                return BadRequest(ex.Message + "\n" + strSQL);
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
            string strSQL = @"SELECT ROW_NUMBER() OVER (ORDER BY FECHA_MODIFICA ASC) AS N, FECHA_MODIFICA, U.NOMBRE_USUARIO AS REGISTRA, DEP.NOMBRE_DEPARTAMENTO AS DEPART, DOC.NOMBRE_DOCUMENTO AS DOC, DET.NOMBRE_DETALLE AS DETALLE, CLA.NOMBRE_CLASIFICACION AS CLASIFICACION, PRO.NOMBRE_PRODUCTO AS NOMBRE_PRODUCTO, CC.NOMBRE_CENTRO_COSTO AS CENTRO_COSTO, TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, CODIGO_SOCIO AS CODIGO, NOMBRE_SOCIO AS NOMBRE, NUMEROSOLICITUD AS NUMEROSOLICITUD, OBSERVACION, NUMERO_DE_CAJA AS CAJA
                                FROM ADMIN.INVENTARIO_ANTERIOR IA LEFT JOIN ADMIN.USUARIO U ON IA.ID_USUARIO_REGISTRA_FK = U.ID_USUARIO
                                LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON IA.ID_DEPARTAMENTO_FK = DEP.ID_DEPARTAMENTO
                                LEFT JOIN ADMIN.LDOCUMENTO DOC ON IA.ID_DOCUMENTO_FK = DOC.ID_DOCUMENTO
                                LEFT JOIN ADMIN.LDETALLE DET ON IA.ID_DETALLE_FK = DET.ID_DETALLE
                                LEFT JOIN ADMIN.LCLASIFICACION CLA ON IA.ID_CLASIFICACION_FK = CLA.ID_CLASIFICACION
                                LEFT JOIN ADMIN.LPRODUCTO PRO ON IA.ID_PRODUCTO_FK = PRO.ID_PRODUCTO
                                LEFT JOIN ADMIN.CENTRO_COSTO CC ON IA.ID_CENTRO_COSTO_FK = CC.ID_CENTRO_COSTO";
            strSQL += " WHERE IA.ID_INVENTARIO_GENERAL_FK = " + jsontoken.idinventario;
            strSQL += @" UNION ALL
                                SELECT                                             0 AS N, FECHA_MODIFICA, U.NOMBRE_USUARIO AS REGISTRA, DEP.NOMBRE_DEPARTAMENTO AS DEPART, DOC.NOMBRE_DOCUMENTO AS DOC, DET.NOMBRE_DETALLE AS DETALLE, CLA.NOMBRE_CLASIFICACION AS CLASIFICACION, PRO.NOMBRE_PRODUCTO AS NOMBRE_PRODUCTO, CC.NOMBRE_CENTRO_COSTO AS CENTRO_COSTO, TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, CODIGO_SOCIO AS CODIGO, NOMBRE_SOCIO AS NOMBRE, NUMEROSOLICITUD AS NUMEROSOLICITUD, OBSERVACION, NUMERO_DE_CAJA AS CAJA
                                FROM ADMIN.INVENTARIO_GENERAL IG LEFT JOIN ADMIN.USUARIO U ON IG.ID_USUARIO_REGISTRA_FK = U.ID_USUARIO
                                LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON IG.ID_DEPARTAMENTO_FK = DEP.ID_DEPARTAMENTO
                                LEFT JOIN ADMIN.LDOCUMENTO DOC ON IG.ID_DOCUMENTO_FK = DOC.ID_DOCUMENTO
                                LEFT JOIN ADMIN.LDETALLE DET ON IG.ID_DETALLE_FK = DET.ID_DETALLE
                                LEFT JOIN ADMIN.LCLASIFICACION CLA ON IG.ID_CLASIFICACION_FK = CLA.ID_CLASIFICACION
                                LEFT JOIN ADMIN.LPRODUCTO PRO ON IG.ID_PRODUCTO_FK = PRO.ID_PRODUCTO
                                LEFT JOIN ADMIN.CENTRO_COSTO CC ON IG.ID_CENTRO_COSTO_FK = CC.ID_CENTRO_COSTO";
            strSQL += " WHERE IG.ID_INVENTARIO_GENERAL = " + jsontoken.idinventario + " ORDER BY N DESC";

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
                return BadRequest(ex.Message + "\n" + strSQL);
            }
        }
    }
}
