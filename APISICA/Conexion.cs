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

        public bool conectar()
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

        public bool iniciaCommand(string strSQL)
        {
            command = new OracleCommand(strSQL, connection);
            return true;
        }

        public bool agregarParametroCommand(string nombre, string valor)
        {
            command.Parameters.Add(nombre, valor);
            return true;
        }

        public bool agregarParametroCommandInt(string nombre, int valor)
        {
            OracleParameter parameter = new OracleParameter();
            parameter.OracleDbType = OracleDbType.Int32;
            parameter.Value = valor;
            command.Parameters.Add(nombre, parameter);
            return true;
        }

        public bool agregarParametroCommandDate(string nombre, string valor)
        {
            DateTime fecha = DateTime.Parse(valor);
            OracleParameter parameter = new OracleParameter();
            parameter.OracleDbType = OracleDbType.Date;
            parameter.Value = fecha;
            command.Parameters.Add(nombre, parameter);
            return true;
        }

        public bool ejecutarQuery()
        {
            command.ExecuteNonQuery();
            return true;
        }

        public DataTable llenarDataTable()
        {
            DataTable dt = new DataTable();
            reader = command.ExecuteReader();
            dt.Load(reader);
            return dt;
        }

        public int ejecutarQueryEscalar()
        {
            int resp;
            try
            {
                if (!(command.ExecuteScalar() is null))
                {
                    string ret = command.ExecuteScalar().ToString();
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

        public void cerrar()
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
