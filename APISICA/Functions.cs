using APISICA.Class;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Security.Policy;

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

        public static void guardarEditar(Conexion conn, Cuenta cuenta, GuardarClass guardar)
        {
            /*string strSQL = @"INSERT INTO ADMIN.INVENTARIO_ANTERIOR (ID_INVENTARIO_GENERAL_FK, NUMERO_DE_CAJA, ID_DEPARTAMENTO_FK, ID_DOCUMENTO_FK, ID_DETALLE_FK, FECHA_DESDE, FECHA_HASTA,
                    ID_CLASIFICACION_FK, ID_PRODUCTO_FK, ID_CENTRO_COSTO_FK, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD, OBSERVACION, ID_USUARIO_REGISTRA_FK, FECHA_MODIFICA, PEN_NOMBRE, PEN_DETALLE, PEN_BANCA, DESC_CONCAT, USUARIO, FECHA)
                                                                SELECT  ID_INVENTARIO_GENERAL, NUMERO_DE_CAJA, ID_DEPARTAMENTO_FK, ID_DOCUMENTO_FK, ID_DETALLE_FK, FECHA_DESDE, FECHA_HASTA,
                    ID_CLASIFICACION_FK, ID_PRODUCTO_FK, ID_CENTRO_COSTO_FK, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD, OBSERVACION, ID_USUARIO_REGISTRA_FK, FECHA_MODIFICA, PEN_NOMBRE, PEN_DETALLE, PEN_BANCA, DESC_CONCAT,
                     " + cuenta.IdUser + ", SYSDATE FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsonbody.idinventario + " RETURNING ID_INVENTARIO_ANTERIOR_NEW INTO :numero";
            */
            string strSQL = "INSERT INTO ADMIN.INVENTARIO_ANTERIOR (ID_INVENTARIO_GENERAL_FK, NUMERO_DE_CAJA, ID_DEPARTAMENTO_FK, ID_DOCUMENTO_FK, ID_DETALLE_FK, FECHA_DESDE, FECHA_HASTA, ID_CLASIFICACION_FK, ID_PRODUCTO_FK, ID_CENTRO_COSTO_FK, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD, OBSERVACION, ID_USUARIO_REGISTRA_FK, FECHA_MODIFICA, PEN_NOMBRE, PEN_DETALLE, PEN_BANCA, DESC_CONCAT, USUARIO, FECHA) VALUES (";
            strSQL += "(SELECT ID_INVENTARIO_GENERAL FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT NUMERO_DE_CAJA FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT ID_DEPARTAMENTO_FK FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT ID_DOCUMENTO_FK FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT ID_DETALLE_FK FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT FECHA_DESDE FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT FECHA_HASTA FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT ID_CLASIFICACION_FK FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT ID_PRODUCTO_FK FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT ID_CENTRO_COSTO_FK FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT CODIGO_SOCIO FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT NOMBRE_SOCIO FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT NUMEROSOLICITUD FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT OBSERVACION FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT ID_USUARIO_REGISTRA_FK FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT FECHA_MODIFICA FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT PEN_NOMBRE FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT PEN_DETALLE FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT PEN_BANCA FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += "(SELECT DESC_CONCAT FROM ADMIN.INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario + "),";
            strSQL += cuenta.IdUser + ", TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS')) RETURNING ID_INVENTARIO_ANTERIOR INTO :numero";

            conn.Conectar();

            int i = conn.InsertReturnID(strSQL);
            if (i == -1)
            {
                throw new ArgumentException("No se pudo grabar el histórico INVENTARIO_ANTERIOR\n" + strSQL);
            }
            strSQL = "UPDATE ADMIN.INVENTARIO_HISTORICO SET ID_INVENTARIO_ANTERIOR_FK = " + i + " WHERE ID_INVENTARIO_GENERAL_FK = " + guardar.idinventario + " AND NVL(ID_INVENTARIO_ANTERIOR_FK, -1) = -1";
            conn.EjecutarQuery(strSQL);

            strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET";
            strSQL += " NUMERO_DE_CAJA = '" + guardar.numerocaja + "',";
            if (guardar.iddetalle > 0)
            {
                strSQL += " ID_DETALLE_FK = " + guardar.iddetalle + ",";
            }
            if (guardar.iddepartamento > 0)
            {
                strSQL += " ID_DEPARTAMENTO_FK = " + guardar.iddepartamento + ",";
            }
            if (guardar.iddocumento > 0)
            {
                strSQL += " ID_DOCUMENTO_FK = " + guardar.iddocumento + ",";
            }
            if (guardar.idclasificacion > 0)
            {
                strSQL += " ID_CLASIFICACION_FK = " + guardar.idclasificacion + ",";
            }
            if (guardar.idproducto > 0)
            {
                strSQL += " ID_PRODUCTO_FK = " + guardar.idproducto + ",";
            }
            if (guardar.idcentrocosto > 0)
            {
                strSQL += " ID_CENTRO_COSTO_FK = " + guardar.idcentrocosto + ",";
            }
            if (guardar.pendiente != "")
            {
                strSQL += " PEN_NOMBRE = '" + guardar.pendiente + "',";
            }
            if (guardar.detallepen != "")
            {
                strSQL += " PEN_DETALLE = '" + guardar.detallepen + "',";
            }
            if (guardar.banca != "")
            {
                strSQL += " PEN_BANCA = '" + guardar.banca + "',";
            }

            strSQL += " ID_USUARIO_REGISTRA_FK = " + cuenta.IdUser + ",";

            if (guardar.codigosocio != "")
            {
                strSQL += " CODIGO_SOCIO = '" + guardar.codigosocio + "',";
            }
            if (guardar.nombresocio != "")
            {
                strSQL += " NOMBRE_SOCIO = '" + guardar.nombresocio + "',";
            }
            if (guardar.numerosolicitud != "")
            {
                strSQL += " NUMEROSOLICITUD = '" + guardar.numerosolicitud + "',";
            }
            if (guardar.observacion != "")
            {
                strSQL += " OBSERVACION = '" + guardar.observacion + "',";
            }

            if (guardar.fechadesde != "")
                strSQL += " FECHA_DESDE = TO_DATE('" + guardar.fechadesde + "', 'YYYY-MM-DD HH24:MI:SS'),";
            else
                strSQL += " FECHA_DESDE = NULL,";
            if (guardar.fechahasta != "")
                strSQL += " FECHA_HASTA = TO_DATE('" + guardar.fechahasta + "', 'YYYY-MM-DD HH24:MI:SS'),";
            else
                strSQL += " FECHA_HASTA = NULL,";
            /*
            if (guardar.fechamodifica != "")
                strSQL += " FECHA_MODIFICA = TO_DATE('" + guardar.fechamodifica + "', 'YYYY-MM-DD HH24:MI:SS')";
            */
            strSQL += " FECHA_MODIFICA = TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS')";
            strSQL += " WHERE ID_INVENTARIO_GENERAL = " + guardar.idinventario;

            //conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
            //conn.Conectar();
            conn.EjecutarQuery(strSQL);

            strSQL = Functions.obtenerDescConcatSQL(guardar.idinventario);

            conn.EjecutarQuery(strSQL);

        }
    }
}
