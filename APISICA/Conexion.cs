using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Text;

namespace APISICA
{
    public class Conexion
    {
        public static OracleConnection? connection;
        public static OracleCommand? command;
        public static OracleDataReader? reader;
        public static string? lastError;
        public static string? _connString;

        public Conexion(string connString)
        {
            _connString = connString;
        }
        public Conexion()
        {

        }

        public bool Conectar()
        {
            if (connection != null)
            {
                if (connection.State == ConnectionState.Open)
                {
                    return true;
                }
                else
                {
                    connection = new OracleConnection(_connString);
                }
            }
            else
            {
                connection = new OracleConnection(_connString);
            }

            connection.Open();
            return true;
        }

        public bool EjecutarQuery(string strSQL)
        {
            command = new OracleCommand(strSQL, connection);
            command.ExecuteNonQuery();
            return true;
        }

        public DataTable LlenarDataTable(string strSQL)
        {
            DataTable dt = new DataTable();
            command = new OracleCommand(strSQL, connection);
            //command.ExecuteNonQuery();
            reader = command.ExecuteReader();
            dt.Load(reader);
            return dt;
        }

        public int EjecutarQueryEscalar(string strSQL)
        {
            int resp;
            try
            {
                command = new OracleCommand(strSQL, connection);
                if (!(command.ExecuteScalar() is null))
                {
                    string ret = command.ExecuteScalar().ToString() ?? "-1";
                    resp = Convert.ToInt32(ret);
                }
                else
                {
                    resp = -1;
                }
            }
            catch
            {
                resp = -1;
            }
            return resp;
        }

        public int InsertReturnID(string strInsert)
        {
            try
            {
                OracleCommand cmd2 = new OracleCommand(strInsert, connection);
                cmd2.Parameters.Add("numero", OracleDbType.Decimal, ParameterDirection.ReturnValue);
                cmd2.ExecuteNonQuery();
                return Convert.ToInt32(cmd2.Parameters["numero"].Value.ToString());
            }
            catch
            {
                return -1;
            }
        }

        public void Cerrar()
        {
            try
            {
                if (connection != null)
                    connection.Close();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
            }

            try
            {
                if (connection != null)
                    connection.Dispose();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
            }

            try
            {
                if (reader != null)
                    reader.Dispose();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
            }

            try
            {
                if (command != null)
                    command.Dispose();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
            }
        }
    }
}
