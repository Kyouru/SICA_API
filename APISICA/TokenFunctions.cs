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
                return cuenta;
            }

            string strSQL = "SELECT U.ID_USUARIO, UP.USLO, UP.USPS FROM ADMIN.USUARIO U LEFT JOIN ADMIN.USPW UP ON U.ID_USPW_FK = UP.ID_USPW INNER JOIN ADMIN.LOGTOKEN LT ON U.ID_USUARIO = LT.ID_USUARIO_FK WHERE JWT = '" + token + "' AND LT.FECHAFIN IS NULL";

            Conexion conn = new Conexion(connString);
            DataTable dt = new DataTable();
            try
            {
                conn.Conectar();
                dt = conn.LlenarDataTable(strSQL);
                conn.Cerrar();
                cuenta.IdUser = Int32.Parse(dt.Rows[0]["ID_USUARIO"].ToString() ?? "NULL");
                cuenta.UsLo = dt.Rows[0]["USLO"].ToString() ?? "NULL";
                cuenta.UsPs = dt.Rows[0]["USPS"].ToString() ?? "NULL";
            }
            catch
            {
                conn.Cerrar();
                cuenta.IdUser = -1;
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
