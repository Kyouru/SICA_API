using APISICA.Class;
using System.Data;
using System.IdentityModel.Tokens.Jwt;

namespace APISICA
{
    public class TokenFunctions
    {
        public static Cuenta ValidarToken(string connString, string token)
        {
            Cuenta cuenta = new Cuenta();
            if (!CheckTokenIsValid(token))
            {
                cuenta.IdUser = -1;
                cuenta.Permiso = "";
                return cuenta;
            }

            string strSQL = "SELECT ID_USUARIO, CONNUSER FROM ADMIN.USUARIO WHERE JWT = '" + token + "'";

            Conexion conn = new Conexion(connString);
            DataTable dt = new DataTable();
            try
            {
                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();
                dt = conn.llenarDataTable();
                conn.cerrar();
                cuenta.IdUser = Int32.Parse(dt.Rows[0]["ID_USUARIO"].ToString());
                cuenta.Permiso = dt.Rows[0]["CONNUSER"].ToString();
            }
            catch
            {
                conn.cerrar();
                cuenta.IdUser = -1;
                cuenta.Permiso = "No";
            }
            return cuenta;
        }

        private static long GetTokenExpirationTime(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            var tokenExp = jwtSecurityToken.Claims.First(claim => claim.Type.Equals("exp")).Value;
            var ticks = long.Parse(tokenExp);
            return ticks;
        }

        private static bool CheckTokenIsValid(string token)
        {
            var tokenTicks = GetTokenExpirationTime(token);
            var tokenDate = DateTimeOffset.FromUnixTimeSeconds(tokenTicks).UtcDateTime;

            var now = DateTime.Now.ToUniversalTime();

            var valid = tokenDate >= now;

            return valid;
        }
    }
}
