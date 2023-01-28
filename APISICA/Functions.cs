using APISICA.Class;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using JsonBody = APISICA.Class.JsonBody;

namespace APISICA
{
    public class Functions
    {
        public string DataTableToJsonWithJsonNet(DataTable table)
        {
            string jsonString = string.Empty;
            jsonString = JsonConvert.SerializeObject(table);
            return jsonString;
        }

        public static string obtenerDescConcatSQL(int idinventario)
        {
            return @"UPDATE ADMIN.INVENTARIO_GENERAL IG
                        SET DESC_CONCAT =   UPPER(
                        TO_CHAR(FECHA_REGISTRO, 'DD/MM/YYYY') || ';' ||
                        (SELECT NOMBRE_ESTADO FROM ADMIN.LESTADO WHERE ID_ESTADO = IG.ID_ESTADO_FK) || ';' ||
                        NVL(TO_CHAR(FECHA_MODIFICA, 'DD/MM/YYYY'), 'NULO') || ';' ||
                        NVL((SELECT NOMBRE_DEPARTAMENTO FROM ADMIN.LDEPARTAMENTO WHERE ID_DEPARTAMENTO = IG.ID_DEPARTAMENTO_FK), 'NULO') || ';' ||
                        NVL(TO_CHAR(FECHA_DESDE, 'DD/MM/YYYY'), 'NULO') || ';' ||
                        NVL(TO_CHAR(FECHA_HASTA, 'DD/MM/YYYY'), 'NULO') || ';' ||
                        NVL((SELECT NOMBRE_DOCUMENTO FROM ADMIN.LDOCUMENTO WHERE ID_DOCUMENTO = IG.ID_DOCUMENTO_FK), 'NULO') || ';' ||
                        NVL((SELECT NOMBRE_DETALLE FROM ADMIN.LDETALLE WHERE ID_DETALLE = IG.ID_DETALLE_FK), 'NULO') || ';' ||
                        NVL(NUMEROSOLICITUD, 'NULO') || ';' ||
                        NVL(CODIGO_SOCIO, 'NULO') || ';' ||
                        NVL(NOMBRE_SOCIO, 'NULO') || ';' ||
                        NVL((SELECT NVL(NOMBRE_CLASIFICACION, 'NULO') FROM ADMIN.LCLASIFICACION WHERE ID_CLASIFICACION = IG.ID_CLASIFICACION_FK), 'NULO') || ';' ||
                        NVL(OBSERVACION, 'NULO') || ';' ||
                        NVL((SELECT NOMBRE_UBICACION FROM ADMIN.UBICACION WHERE ID_UBICACION = IG.ID_UBICACION_FK), 'NULO') || ';' ||
                        NVL((SELECT NOMBRE_CENTRO_COSTO FROM ADMIN.CENTRO_COSTO WHERE ID_CENTRO_COSTO = IG.ID_CENTRO_COSTO_FK), 'NULO') || ';' ||
                        NVL((SELECT NOMBRE_PRODUCTO FROM ADMIN.LPRODUCTO WHERE ID_PRODUCTO = IG.ID_PRODUCTO_FK), 'NULO')
                    ) WHERE ID_INVENTARIO_GENERAL = " + idinventario;
        }

        public static void guardarEditar(Conexion conn, Cuenta cuenta, JsonBody jsonbody)
        {
            /*string strSQL = @"INSERT INTO ADMIN.INVENTARIO_ANTERIOR (ID_INVENTARIO_GENERAL_FK, NUMERO_DE_CAJA, ID_DEPARTAMENTO_FK, ID_DOCUMENTO_FK, ID_DETALLE_FK, FECHA_DESDE, FECHA_HASTA,
                    ID_CLASIFICACION_FK, ID_PRODUCTO_FK, ID_CENTRO_COSTO_FK, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD, OBSERVACION, ID_USUARIO_REGISTRA_FK, FECHA_MODIFICA, PEN_NOMBRE, PEN_DETALLE, PEN_BANCA, DESC_CONCAT, USUARIO, FECHA)
                                                                SELECT  ID_INVENTARIO_GENERAL, NUMERO_DE_CAJA, ID_DEPARTAMENTO_FK, ID_DOCUMENTO_FK, ID_DETALLE_FK, FECHA_DESDE, FECHA_HASTA,
                    ID_CLASIFICACION_FK, ID_PRODUCTO_FK, ID_CENTRO_COSTO_FK, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD, OBSERVACION, ID_USUARIO_REGISTRA_FK, FECHA_MODIFICA, PEN_NOMBRE, PEN_DETALLE, PEN_BANCA, DESC_CONCAT,
                     " + cuenta.IdUser + ", SYSDATE FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + " RETURNING ID_INVENTARIO_ANTERIOR_NEW INTO :numero";
            */
            string strSQL = "INSERT INTO ADMIN.INVENTARIO_ANTERIOR (ID_INVENTARIO_GENERAL_FK, NUMERO_DE_CAJA, ID_DEPARTAMENTO_FK, ID_DOCUMENTO_FK, ID_DETALLE_FK, FECHA_DESDE, FECHA_HASTA, ID_CLASIFICACION_FK, ID_PRODUCTO_FK, ID_CENTRO_COSTO_FK, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD, OBSERVACION, ID_USUARIO_REGISTRA_FK, FECHA_MODIFICA, PEN_NOMBRE, PEN_DETALLE, PEN_BANCA, DESC_CONCAT, USUARIO, FECHA) VALUES (";
            strSQL += "(SELECT ID_INVENTARIO_GENERAL FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT NUMERO_DE_CAJA FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT ID_DEPARTAMENTO_FK FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT ID_DOCUMENTO_FK FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT ID_DETALLE_FK FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT FECHA_DESDE FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT FECHA_HASTA FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT ID_CLASIFICACION_FK FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT ID_PRODUCTO_FK FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT ID_CENTRO_COSTO_FK FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT CODIGO_SOCIO FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT NOMBRE_SOCIO FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT NUMEROSOLICITUD FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT OBSERVACION FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT ID_USUARIO_REGISTRA_FK FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT FECHA_MODIFICA FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT PEN_NOMBRE FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT PEN_DETALLE FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT PEN_BANCA FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += "(SELECT DESC_CONCAT FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + "),";
            strSQL += cuenta.IdUser + ", TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS')) RETURNING ID_INVENTARIO_ANTERIOR INTO :numero";

            conn.Conectar();

            int i = conn.InsertReturnID(strSQL);
            if (i == -1)
            {
                throw new ArgumentException("No se pudo grabar el histórico INVENTARIO_ANTERIOR\n" + strSQL);
            }
            strSQL = "UPDATE ADMIN.INVENTARIO_HISTORICO SET ID_INVENTARIO_ANTERIOR_FK = " + i + " WHERE ID_INVENTARIO_GENERAL_FK = " + jsonbody.idinventario + " AND NVL(ID_INVENTARIO_ANTERIOR_FK, -1) = -1";
            conn.EjecutarQuery(strSQL);

            strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET";
            strSQL += " NUMERO_DE_CAJA = '" + jsonbody.numerocaja + "',";
            if (jsonbody.iddetalle > 0)
            {
                strSQL += " ID_DETALLE_FK = " + jsonbody.iddetalle + ",";
            }
            if (jsonbody.iddepartamento > 0)
            {
                strSQL += " ID_DEPARTAMENTO_FK = " + jsonbody.iddepartamento + ",";
            }
            if (jsonbody.iddocumento > 0)
            {
                strSQL += " ID_DOCUMENTO_FK = " + jsonbody.iddocumento + ",";
            }
            if (jsonbody.idclasificacion > 0)
            {
                strSQL += " ID_CLASIFICACION_FK = " + jsonbody.idclasificacion + ",";
            }
            if (jsonbody.idproducto > 0)
            {
                strSQL += " ID_PRODUCTO_FK = " + jsonbody.idproducto + ",";
            }
            if (jsonbody.idcentrocosto > 0)
            {
                strSQL += " ID_CENTRO_COSTO_FK = " + jsonbody.idcentrocosto + ",";
            }
            if (jsonbody.pendiente != "")
            {
                strSQL += " PEN_NOMBRE = '" + jsonbody.pendiente + "',";
            }
            if (jsonbody.detallepen != "")
            {
                strSQL += " PEN_DETALLE = '" + jsonbody.detallepen + "',";
            }
            if (jsonbody.banca != "")
            {
                strSQL += " PEN_BANCA = '" + jsonbody.banca + "',";
            }

            strSQL += " ID_USUARIO_REGISTRA_FK = " + cuenta.IdUser + ",";

            if (jsonbody.codigosocio != "")
            {
                strSQL += " CODIGO_SOCIO = '" + jsonbody.codigosocio + "',";
            }
            if (jsonbody.nombresocio != "")
            {
                strSQL += " NOMBRE_SOCIO = '" + jsonbody.nombresocio + "',";
            }
            if (jsonbody.numerosolicitud != "")
            {
                strSQL += " NUMEROSOLICITUD = '" + jsonbody.numerosolicitud + "',";
            }
            if (jsonbody.observacion != "")
            {
                strSQL += " OBSERVACION = '" + jsonbody.observacion + "',";
            }

            if (jsonbody.fechadesde != "")
                strSQL += " FECHA_DESDE = TO_DATE('" + jsonbody.fechadesde + "', 'YYYY-MM-DD HH24:MI:SS'),";
            else
                strSQL += " FECHA_DESDE = NULL,";
            if (jsonbody.fechahasta != "")
                strSQL += " FECHA_HASTA = TO_DATE('" + jsonbody.fechahasta + "', 'YYYY-MM-DD HH24:MI:SS'),";
            else
                strSQL += " FECHA_HASTA = NULL,";
            if (jsonbody.fechamodifica != "")
                strSQL += " FECHA_MODIFICA = TO_DATE('" + jsonbody.fechamodifica + "', 'YYYY-MM-DD HH24:MI:SS')";
            strSQL += " WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario;

            //conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
            //conn.Conectar();
            conn.EjecutarQuery(strSQL);

            strSQL = Functions.obtenerDescConcatSQL(jsonbody.idinventario);

            conn.EjecutarQuery(strSQL);

        }

    }
}
