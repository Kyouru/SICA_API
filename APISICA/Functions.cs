using APISICA.Class;
using Newtonsoft.Json;
using System.Data;
using JsonToken = APISICA.Class.JsonToken;

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

        public static void guardarEditar(Conexion conn, Cuenta cuenta, JsonToken jsontoken)
        {
            string strSQL = @"INSERT INTO ADMIN.INVENTARIO_ANTERIOR (ID_INVENTARIO_GENERAL_FK, NUMERO_DE_CAJA, ID_DEPARTAMENTO_FK, ID_DOCUMENTO_FK, ID_DETALLE_FK, FECHA_DESDE, FECHA_HASTA,
                    ID_CLASIFICACION_FK, ID_PRODUCTO_FK, ID_CENTRO_COSTO_FK, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD, OBSERVACION, ID_USUARIO_REGISTRA_FK, FECHA_MODIFICA, PEN_NOMBRE, PEN_DETALLE, PEN_BANCA)
                                                                SELECT  ID_INVENTARIO_GENERAL, NUMERO_DE_CAJA, ID_DEPARTAMENTO_FK, ID_DOCUMENTO_FK, ID_DETALLE_FK, FECHA_DESDE, FECHA_HASTA,
                    ID_CLASIFICACION_FK, ID_PRODUCTO_FK, ID_CENTRO_COSTO_FK, CODIGO_SOCIO, NOMBRE_SOCIO, NUMEROSOLICITUD, OBSERVACION, ID_USUARIO_REGISTRA_FK, FECHA_MODIFICA, PEN_NOMBRE, PEN_DETALLE, PEN_BANCA
                    FROM INVENTARIO_GENERAL WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario;

            conn.conectar();
            conn.iniciaCommand(strSQL);
            conn.ejecutarQuery();

            strSQL = "UPDATE ADMIN.INVENTARIO_GENERAL SET";
            strSQL += " NUMERO_DE_CAJA = '" + jsontoken.numerocaja + "',";
            if (jsontoken.iddetalle > 0)
            {
                strSQL += " ID_DETALLE_FK = " + jsontoken.iddetalle + ",";
            }
            if (jsontoken.iddepartamento > 0)
            {
                strSQL += " ID_DEPARTAMENTO_FK = " + jsontoken.iddepartamento + ",";
            }
            if (jsontoken.iddocumento > 0)
            {
                strSQL += " ID_DOCUMENTO_FK = " + jsontoken.iddocumento + ",";
            }
            if (jsontoken.idclasificacion > 0)
            {
                strSQL += " ID_CLASIFICACION_FK = " + jsontoken.idclasificacion + ",";
            }
            if (jsontoken.idproducto > 0)
            {
                strSQL += " ID_PRODUCTO_FK = " + jsontoken.idproducto + ",";
            }
            if (jsontoken.idcentrocosto > 0)
            {
                strSQL += " ID_CENTRO_COSTO_FK = " + jsontoken.idcentrocosto + ",";
            }
            if (jsontoken.pendiente != "")
            {
                strSQL += " PEN_NOMBRE = '" + jsontoken.pendiente + "',";
            }
            if (jsontoken.detallepen != "")
            {
                strSQL += " PEN_DETALLE = '" + jsontoken.detallepen + "',";
            }
            if (jsontoken.banca != "")
            {
                strSQL += " PEN_BANCA = '" + jsontoken.banca + "',";
            }
            strSQL += " ID_USUARIO_REGISTRA_FK = " + cuenta.IdUser + ",";
            if (jsontoken.codigosocio != "")
            {
                strSQL += " CODIGO_SOCIO = '" + jsontoken.codigosocio + "',";
            }
            if (jsontoken.nombresocio != "")
            {
                strSQL += " NOMBRE_SOCIO = '" + jsontoken.nombresocio + "',";
            }
            if (jsontoken.numerosolicitud != "")
            {
                strSQL += " NUMEROSOLICITUD = '" + jsontoken.numerosolicitud + "',";
            }
            if (jsontoken.observacion != "")
            {
                strSQL += " OBSERVACION = '" + jsontoken.observacion + "',";
            }

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
            strSQL += " WHERE ID_INVENTARIO_GENERAL = " + jsontoken.idinventario;

            //conn = new Conexion(_configuration.GetConnectionString(cuenta.Permiso));
            //conn.conectar();
            conn.iniciaCommand(strSQL);
            conn.ejecutarQuery();

            strSQL = Functions.obtenerDescConcatSQL(jsontoken.idinventario);

            conn.iniciaCommand(strSQL);
            conn.ejecutarQuery();

        }

    }
}
