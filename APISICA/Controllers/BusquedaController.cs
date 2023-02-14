using APISICA.Class;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Configuration;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
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
        public IActionResult Buscar(Class.JsonBody jsonbody)
        {
            DataTable dt;
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
                strSQL += " WHERE 1 = 1";
                if (jsonbody.busquedalibre != "")
                    strSQL += " AND DESC_CONCAT LIKE '%" + jsonbody.busquedalibre + "%'";
                if (jsonbody.numerocaja != "")
                    strSQL += " AND NUMERO_DE_CAJA LIKE '" + jsonbody.numerocaja + "'";
                if (jsonbody.fecha != "")
                    strSQL += " AND TRUNC(FECHA_DESDE) <= TO_DATE('" + jsonbody.fecha + "', 'YYYY-MM-DD') AND TRUNC(FECHA_HASTA) >= TO_DATE('" + jsonbody.fecha + "', 'YYYY-MM-DD')";
                strSQL += " ORDER BY ID_INVENTARIO_GENERAL DESC";

                Conexion conn = new Conexion();
                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    conn.Conectar();
                    dt = conn.LlenarDataTable(strSQL);
                    dt.TableName = "Buscar";
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


        [HttpPost("datoseditar")]
        public IActionResult DatosEditar(Class.JsonBody jsonbody)
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

                string strSQL = @"SELECT *
                FROM (ADMIN.INVENTARIO_GENERAL IG LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON IG.ID_DEPARTAMENTO_FK = DEP.ID_DEPARTAMENTO)
                LEFT JOIN ADMIN.LDOCUMENTO DOC ON IG.ID_DOCUMENTO_FK = DOC.ID_DOCUMENTO
                WHERE IG.ID_INVENTARIO_GENERAL = " + jsonbody.idinventario;

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
                    return BadRequest(ex.Message + "\n" + strSQL);
                }
            }
            else
            {
                return Unauthorized("No se recibió bearer token");
            }
        }

        [HttpPost("guardareditar")]
        public IActionResult GuardarEditar(Class.JsonBody jsonbody)
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
                if (!(cuenta.IdUser > 0))
                {
                    return Unauthorized("Sesion no encontrada");
                }


                Conexion conn = new Conexion();

                try
                {
                    conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
                    Functions.guardarEditar(conn, cuenta, jsonbody);

                    conn.Cerrar();
                    return Ok();
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

        [HttpPost("historicomovimiento")]
        public IActionResult HistoricoMovimiento(Class.JsonBody jsonbody)
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                DataTable dt = new DataTable("HistoricoMovimiento");
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
                string strSQL = @"SELECT IG.ID_INVENTARIO_GENERAL AS ID, IH.OBSERVACION,
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
                                    LEFT JOIN ADMIN.LDEPARTAMENTO LDEP ON IG.ID_DEPARTAMENTO_FK = LDEP.ID_DEPARTAMENTO
                                    LEFT JOIN ADMIN.LDOCUMENTO LDOC ON IG.ID_DOCUMENTO_FK = LDOC.ID_DOCUMENTO
                                    LEFT JOIN ADMIN.LDETALLE LDET ON IG.ID_DETALLE_FK = LDET.ID_DETALLE
                                WHERE IH.ANULADO = 0 AND IH.RECIBIDO = 1
                                        AND IG.ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + " ORDER BY IH.FECHA DESC";

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
                    return BadRequest(ex.Message + "\n" + strSQL);
                }
            }
            else
            {
                return Unauthorized("No se recibió bearer token");
            }
            
        }

        [HttpPost("historicoedicion")]
        public IActionResult HistoricoEditar(Class.JsonBody jsonbody)
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                DataTable dt = new DataTable("HistoricoEdicion");
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

                string strSQL = @"SELECT ROW_NUMBER() OVER (ORDER BY FECHA_MODIFICA ASC) AS N, FECHA_MODIFICA, U.NOMBRE_USUARIO AS REGISTRA, DEP.NOMBRE_DEPARTAMENTO AS DEPART, DOC.NOMBRE_DOCUMENTO AS DOC, DET.NOMBRE_DETALLE AS DETALLE, CLA.NOMBRE_CLASIFICACION AS CLASIFICACION, PRO.NOMBRE_PRODUCTO AS NOMBRE_PRODUCTO, CC.NOMBRE_CENTRO_COSTO AS CENTRO_COSTO, TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, CODIGO_SOCIO AS CODIGO, NOMBRE_SOCIO AS NOMBRE, NUMEROSOLICITUD AS NUMEROSOLICITUD, OBSERVACION, NUMERO_DE_CAJA AS CAJA, PEN_NOMBRE, PEN_DETALLE, PEN_BANCA
                                FROM ADMIN.INVENTARIO_ANTERIOR IA LEFT JOIN ADMIN.USUARIO U ON IA.ID_USUARIO_REGISTRA_FK = U.ID_USUARIO
                                LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON IA.ID_DEPARTAMENTO_FK = DEP.ID_DEPARTAMENTO
                                LEFT JOIN ADMIN.LDOCUMENTO DOC ON IA.ID_DOCUMENTO_FK = DOC.ID_DOCUMENTO
                                LEFT JOIN ADMIN.LDETALLE DET ON IA.ID_DETALLE_FK = DET.ID_DETALLE
                                LEFT JOIN ADMIN.LCLASIFICACION CLA ON IA.ID_CLASIFICACION_FK = CLA.ID_CLASIFICACION
                                LEFT JOIN ADMIN.LPRODUCTO PRO ON IA.ID_PRODUCTO_FK = PRO.ID_PRODUCTO
                                LEFT JOIN ADMIN.CENTRO_COSTO CC ON IA.ID_CENTRO_COSTO_FK = CC.ID_CENTRO_COSTO";
                strSQL += " WHERE IA.ID_INVENTARIO_GENERAL_FK = " + jsonbody.idinventario;
                strSQL += @" UNION ALL
                                SELECT                                             9999 AS N, FECHA_MODIFICA, U.NOMBRE_USUARIO AS REGISTRA, DEP.NOMBRE_DEPARTAMENTO AS DEPART, DOC.NOMBRE_DOCUMENTO AS DOC, DET.NOMBRE_DETALLE AS DETALLE, CLA.NOMBRE_CLASIFICACION AS CLASIFICACION, PRO.NOMBRE_PRODUCTO AS NOMBRE_PRODUCTO, CC.NOMBRE_CENTRO_COSTO AS CENTRO_COSTO, TO_CHAR(FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(FECHA_HASTA, 'dd/MM/yyyy') AS HASTA, CODIGO_SOCIO AS CODIGO, NOMBRE_SOCIO AS NOMBRE, NUMEROSOLICITUD AS NUMEROSOLICITUD, OBSERVACION, NUMERO_DE_CAJA AS CAJA, PEN_NOMBRE, PEN_DETALLE, PEN_BANCA
                                FROM ADMIN.INVENTARIO_GENERAL IG LEFT JOIN ADMIN.USUARIO U ON IG.ID_USUARIO_REGISTRA_FK = U.ID_USUARIO
                                LEFT JOIN ADMIN.LDEPARTAMENTO DEP ON IG.ID_DEPARTAMENTO_FK = DEP.ID_DEPARTAMENTO
                                LEFT JOIN ADMIN.LDOCUMENTO DOC ON IG.ID_DOCUMENTO_FK = DOC.ID_DOCUMENTO
                                LEFT JOIN ADMIN.LDETALLE DET ON IG.ID_DETALLE_FK = DET.ID_DETALLE
                                LEFT JOIN ADMIN.LCLASIFICACION CLA ON IG.ID_CLASIFICACION_FK = CLA.ID_CLASIFICACION
                                LEFT JOIN ADMIN.LPRODUCTO PRO ON IG.ID_PRODUCTO_FK = PRO.ID_PRODUCTO
                                LEFT JOIN ADMIN.CENTRO_COSTO CC ON IG.ID_CENTRO_COSTO_FK = CC.ID_CENTRO_COSTO";
                strSQL += " WHERE IG.ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + " ORDER BY N DESC";

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
                    return BadRequest(ex.Message + "\n" + strSQL);
                }
            }
            else
            {
                return Unauthorized("No se recibió bearer token");
            }
        }

        [HttpPost("historico")]
        public IActionResult Historico(Class.JsonBody jsonbody)
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                string bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                DataTable dt = new DataTable("Historico");
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

                string strSQL = @"SELECT IA.ID_INVENTARIO_GENERAL_FK AS ID, 'MOVIMIENTO' AS ESTADO, TO_CHAR(IA.FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(IA.FECHA_HASTA, 'dd/MM/yyyy') AS HASTA,
                                LDEP.NOMBRE_DEPARTAMENTO, LDOC.NOMBRE_DOCUMENTO, LDET.NOMBRE_DETALLE,
                                IA.NUMERO_DE_CAJA, IA.CODIGO_SOCIO, IA.NOMBRE_SOCIO, IA.NUMEROSOLICITUD,
                                CASE WHEN UB1.ID_UBICACION = 1 THEN U1.NOMBRE_USUARIO
                                     WHEN UB1.ID_UBICACION = 2 THEN UE1.NOMBRE_USUARIO_EXTERNO
                                     ELSE UB1.NOMBRE_UBICACION
                                END AS ENTREGA,
                                CASE WHEN UB2.ID_UBICACION = 1 THEN U2.NOMBRE_USUARIO
                                     WHEN UB2.ID_UBICACION = 2 THEN UE2.NOMBRE_USUARIO_EXTERNO
                                     ELSE UB2.NOMBRE_UBICACION
                                END AS RECIBE,
                                TO_CHAR(IH.FECHA_INICIO, 'dd/MM/yyyy HH24:MI:SS') AS FECHA_ENTREGA,
                                TO_CHAR(IH.FECHA_FIN, 'dd/MM/yyyy HH24:MI:SS') AS FECHA_RECIBE,
                                IA.FECHA,
                                99 AS PRIORIDAD
                                FROM ADMIN.INVENTARIO_HISTORICO IH LEFT JOIN ADMIN.INVENTARIO_ANTERIOR IA ON IH.ID_INVENTARIO_ANTERIOR_FK = IA.ID_INVENTARIO_ANTERIOR
                                    LEFT JOIN ADMIN.USUARIO U1 ON IH.ID_USUARIO_ENTREGA_FK = U1.ID_USUARIO
                                    LEFT JOIN ADMIN.USUARIO U2 ON IH.ID_USUARIO_RECIBE_FK = U2.ID_USUARIO
                                    LEFT JOIN ADMIN.USUARIO_EXTERNO UE1 ON IH.ID_USUARIO_ENTREGA_FK = UE1.ID_USUARIO_EXTERNO
                                    LEFT JOIN ADMIN.USUARIO_EXTERNO UE2 ON IH.ID_USUARIO_RECIBE_FK = UE2.ID_USUARIO_EXTERNO
                                    LEFT JOIN ADMIN.UBICACION UB1 ON IH.ID_UBICACION_ENTREGA_FK = UB1.ID_UBICACION
                                    LEFT JOIN ADMIN.UBICACION UB2 ON IH.ID_UBICACION_RECIBE_FK = UB2.ID_UBICACION
                                    LEFT JOIN ADMIN.LDEPARTAMENTO LDEP ON IA.ID_DEPARTAMENTO_FK = LDEP.ID_DEPARTAMENTO
                                    LEFT JOIN ADMIN.LDOCUMENTO LDOC ON IA.ID_DOCUMENTO_FK = LDOC.ID_DOCUMENTO
                                    LEFT JOIN ADMIN.LDETALLE LDET ON IA.ID_DETALLE_FK = LDET.ID_DETALLE
                                WHERE IH.ANULADO = 0 AND IH.RECIBIDO = 1 AND IA.ID_INVENTARIO_GENERAL_FK IS NOT NULL
                                        AND IH.ID_INVENTARIO_GENERAL_FK = " + jsonbody.idinventario + "";

                strSQL += @"UNION ALL SELECT IA.ID_INVENTARIO_GENERAL_FK AS ID, 'MOVIMIENTO' AS ESTADO, TO_CHAR(IA.FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(IA.FECHA_HASTA, 'dd/MM/yyyy') AS HASTA,
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
                                TO_CHAR(IH.FECHA_FIN, 'dd/MM/yyyy HH24:MI:SS') AS FECHA_RECIBE,
                                IH.FECHA_FIN AS FECHA,
                                99 AS PRIORIDAD
                                FROM ADMIN.INVENTARIO_HISTORICO IH LEFT JOIN ADMIN.INVENTARIO_ANTERIOR IA ON IH.ID_INVENTARIO_ANTERIOR_FK = IA.ID_INVENTARIO_ANTERIOR
                                    LEFT JOIN ADMIN.INVENTARIO_GENERAL IG ON IH.ID_INVENTARIO_GENERAL_FK = IG.ID_INVENTARIO_GENERAL
                                    LEFT JOIN ADMIN.USUARIO U1 ON IH.ID_USUARIO_ENTREGA_FK = U1.ID_USUARIO
                                    LEFT JOIN ADMIN.USUARIO U2 ON IH.ID_USUARIO_RECIBE_FK = U2.ID_USUARIO
                                    LEFT JOIN ADMIN.USUARIO_EXTERNO UE1 ON IH.ID_USUARIO_ENTREGA_FK = UE1.ID_USUARIO_EXTERNO
                                    LEFT JOIN ADMIN.USUARIO_EXTERNO UE2 ON IH.ID_USUARIO_RECIBE_FK = UE2.ID_USUARIO_EXTERNO
                                    LEFT JOIN ADMIN.UBICACION UB1 ON IH.ID_UBICACION_ENTREGA_FK = UB1.ID_UBICACION
                                    LEFT JOIN ADMIN.UBICACION UB2 ON IH.ID_UBICACION_RECIBE_FK = UB2.ID_UBICACION
                                    LEFT JOIN ADMIN.LDEPARTAMENTO LDEP ON IG.ID_DEPARTAMENTO_FK = LDEP.ID_DEPARTAMENTO
                                    LEFT JOIN ADMIN.LDOCUMENTO LDOC ON IG.ID_DOCUMENTO_FK = LDOC.ID_DOCUMENTO
                                    LEFT JOIN ADMIN.LDETALLE LDET ON IG.ID_DETALLE_FK = LDET.ID_DETALLE
                                WHERE IH.ANULADO = 0 AND IH.RECIBIDO = 1 AND IA.ID_INVENTARIO_GENERAL_FK IS NULL
                                        AND IH.ID_INVENTARIO_GENERAL_FK = " + jsonbody.idinventario + "";

                strSQL += @" UNION ALL SELECT IG.ID_INVENTARIO_GENERAL AS ID, 'ACTUAL' AS ESTADO, TO_CHAR(IG.FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(IG.FECHA_HASTA, 'dd/MM/yyyy') AS HASTA,
                                LDEP.NOMBRE_DEPARTAMENTO, LDOC.NOMBRE_DOCUMENTO, LDET.NOMBRE_DETALLE,
                                IG.NUMERO_DE_CAJA, IG.CODIGO_SOCIO, IG.NOMBRE_SOCIO, IG.NUMEROSOLICITUD,
                                UBI.NOMBRE_UBICACION AS ENTREGA,
                                UBI.NOMBRE_UBICACION AS RECIBE,
                                NULL AS FECHA_ENTREGA,
                                NULL AS FECHA_RECIBE,
                                IG.FECHA_MODIFICA AS FECHA,
                                1 AS PRIORIDAD
                                FROM ADMIN.INVENTARIO_GENERAL IG
                                    LEFT JOIN ADMIN.LDEPARTAMENTO LDEP ON IG.ID_DEPARTAMENTO_FK = LDEP.ID_DEPARTAMENTO
                                    LEFT JOIN ADMIN.LDOCUMENTO LDOC ON IG.ID_DOCUMENTO_FK = LDOC.ID_DOCUMENTO
                                    LEFT JOIN ADMIN.LDETALLE LDET ON IG.ID_DETALLE_FK = LDET.ID_DETALLE
                                    LEFT JOIN ADMIN.UBICACION UBI ON IG.ID_UBICACION_FK = UBI.ID_UBICACION
                                WHERE IG.ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "";

                strSQL += @" UNION ALL
                                SELECT IA.ID_INVENTARIO_GENERAL_FK AS ID, 'EDICION' AS ESTADO, TO_CHAR(IA.FECHA_DESDE, 'dd/MM/yyyy') AS DESDE, TO_CHAR(IA.FECHA_HASTA, 'dd/MM/yyyy') AS HASTA,
                                LDEP.NOMBRE_DEPARTAMENTO, LDOC.NOMBRE_DOCUMENTO, LDET.NOMBRE_DETALLE,
                                IA.NUMERO_DE_CAJA, IA.CODIGO_SOCIO, IA.NOMBRE_SOCIO, IA.NUMEROSOLICITUD,
                                NULL AS ENTREGA,
                                NULL AS RECIBE,
                                NULL AS FECHA_ENTREGA,
                                NULL AS FECHA_RECIBE,
                                IA.FECHA,
                                99 AS PRIORIDAD
                                FROM ADMIN.INVENTARIO_ANTERIOR IA
                                    LEFT JOIN ADMIN.LDEPARTAMENTO LDEP ON IA.ID_DEPARTAMENTO_FK = LDEP.ID_DEPARTAMENTO
                                    LEFT JOIN ADMIN.LDOCUMENTO LDOC ON IA.ID_DOCUMENTO_FK = LDOC.ID_DOCUMENTO
                                    LEFT JOIN ADMIN.LDETALLE LDET ON IA.ID_DETALLE_FK = LDET.ID_DETALLE
                                WHERE IA.ID_INVENTARIO_ANTERIOR NOT IN (SELECT DISTINCT ID_INVENTARIO_ANTERIOR_FK FROM ADMIN.INVENTARIO_HISTORICO WHERE ID_INVENTARIO_GENERAL_FK = " + jsonbody.idinventario + ") AND IA.ID_INVENTARIO_GENERAL_FK = " + jsonbody.idinventario;

                strSQL += " ORDER BY PRIORIDAD ASC, FECHA_RECIBE DESC";



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
