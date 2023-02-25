using APISICA.Class;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APISICA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("actualizarpassword")]
        public IActionResult ActualizarPassword(User request)
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
                    conn = new Conexion(AesFunctions.connString(cuenta, _configuration.GetSection("AuthController:aeskey").Value, _configuration.GetConnectionString("BASE")));
                    CreatePasswordHash(request.Password, out string passwordHash, out string passwordSalt);

                    string strSQL = "UPDATE ADMIN.USUARIO SET CAMBIAR_PASSWORD = 0, PASSWORDHASH = '" + passwordHash + "', PASSWORDSALT = '" + passwordSalt + "' WHERE ID_USUARIO = " + cuenta.IdUser;

                    conn.Conectar();
                    conn.EjecutarQuery(strSQL);
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

        [HttpPost("login")]
        public IActionResult Login(User request)
        {
            DataTable dt;
            string strSQL = "SELECT U.PASSWORDHASH, U.PASSWORDSALT, U.ID_USUARIO, U.CAMBIAR_PASSWORD, U.ACCESO_PERMITIDO, U.CERRAR_SESION, NVL(P.BUSQUEDA, 0) AS BUSQUEDA, NVL(P.BUSQUEDA_HISTORICO, 0) AS BUSQUEDA_HISTORICO, NVL(P.BUSQUEDA_EDITAR, 0) AS BUSQUEDA_EDITAR, NVL(P.MOVER, 0) MOVER, NVL(P.MOVER_EXPEDIENTE, 0) MOVER_EXPEDIENTE, NVL(P.MOVER_DOCUMENTO, 0) MOVER_DOCUMENTO, NVL(P.MOVER_MASIVO, 0) MOVER_MASIVO, NVL(P.VALIJA, 0) VALIJA, NVL(P.VALIJA_NUEVO, 0) VALIJA_NUEVO , NVL(P.VALIJA_VALIJA, 0) VALIJA_VALIJA, NVL(P.VALIJA_TRANSICION, 0) VALIJA_TRANSICION, NVL(P.PAGARE, 0) PAGARE, NVL(P.PAGARE_BUSCAR, 0) PAGARE_BUSCAR, NVL(P.PAGARE_RECIBIR, 0) PAGARE_RECIBIR, NVL(P.PAGARE_ENTREGAR, 0) PAGARE_ENTREGAR, NVL(P.LETRA, 0) LETRA, NVL(P.LETRA_NUEVO, 0) LETRA_NUEVO, NVL(P.LETRA_ENTREGAR, 0) LETRA_ENTREGAR, NVL(P.LETRA_REINGRESO, 0) LETRA_REINGRESO, NVL(P.LETRA_BUSCAR, 0) LETRA_BUSCAR, NVL(P.MANTENIMIENTO, 0) MANTENIMIENTO, NVL(P.MANTENIMIENTO_USUARIO_EXTERNO, 0) MANTENIMIENTO_USUARIO_EXTERNO, NVL(P.MANTENIMIENTO_LISTAS, 0) MANTENIMIENTO_LISTAS, NVL(P.PENDIENTE, 0) PENDIENTE, NVL(P.PENDIENTE_REGULARIZAR, 0) PENDIENTE_REGULARIZAR, NVL(P.REPORTE, 0) REPORTE, NVL(P.REPORTE_CAJAS, 0) REPORTE_CAJAS, NVL(P.PRESTAR, 0) PRESTAR, NVL(P.PRESTAR_PRESTAR, 0) PRESTAR_PRESTAR, NVL(P.PRESTAR_RECIBIR, 0) PRESTAR_RECIBIR FROM ADMIN.USUARIO U LEFT JOIN ADMIN.PERMISO P ON U.ID_USUARIO = P.ID_USUARIO_FK WHERE U.NOMBRE_USUARIO = '" + request.Username + "'";
            string token;
            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString("UserCheck"));
                conn.Conectar();
                dt = conn.LlenarDataTable(strSQL);

                if (dt.Rows.Count <= 0)
                {
                    conn.Cerrar();
                    return BadRequest("Usuario no Encontrado");
                }

                if (!VerifyPasswordHash(request.Password, dt.Rows[0]["PASSWORDHASH"].ToString() ?? "NULL", dt.Rows[0]["PASSWORDSALT"].ToString() ?? "NULL"))
                {
                    conn.Cerrar();
                    return BadRequest("Contraseña Errada");
                }

                token = CreateToken(request.Username);

                //strSQL = "UPDATE ADMIN.USUARIO SET JWT = '" + token + "' WHERE NOMBRE_USUARIO = '" + request.Username + "'";
                strSQL = "UPDATE ADMIN.LOGTOKEN SET FECHAFIN = TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS') WHERE FECHAFIN IS NULL AND ID_USUARIO_FK = " + Int32.Parse(dt.Rows[0]["ID_USUARIO"].ToString() ?? "-1");
                conn.EjecutarQuery(strSQL);
                strSQL = "INSERT INTO ADMIN.LOGTOKEN (JWT, ID_USUARIO_FK) VALUES ('" + token + "', " + Int32.Parse(dt.Rows[0]["ID_USUARIO"].ToString() ?? "-1") + ")";
                conn.EjecutarQuery(strSQL);
                conn.Cerrar();
            }
            catch (Exception ex)
            {
                conn.Cerrar();
                return BadRequest(ex.Message + strSQL);
            }

            try
            {
                UserData userdata = new UserData();

                userdata.Username = request.Username;
                userdata.IdUser = Int32.Parse(dt.Rows[0]["ID_USUARIO"].ToString() ?? "-1");
                userdata.CambiarPassword = Int32.Parse(dt.Rows[0]["CAMBIAR_PASSWORD"].ToString() ?? "-1");
                userdata.AccesoPermitido = Int32.Parse(dt.Rows[0]["ACCESO_PERMITIDO"].ToString() ?? "-1");
                userdata.CerrarSesion = Int32.Parse(dt.Rows[0]["CERRAR_SESION"].ToString() ?? "-1");

                userdata.Busqueda = Int32.Parse(dt.Rows[0]["BUSQUEDA"].ToString() ?? "-1");
                userdata.BusquedaHistorico = Int32.Parse(dt.Rows[0]["BUSQUEDA_HISTORICO"].ToString() ?? "-1");
                userdata.BusquedaEditar = Int32.Parse(dt.Rows[0]["BUSQUEDA_EDITAR"].ToString() ?? "-1");
                userdata.Mover = Int32.Parse(dt.Rows[0]["MOVER"].ToString() ?? "-1");
                userdata.MoverExpediente = Int32.Parse(dt.Rows[0]["MOVER_EXPEDIENTE"].ToString() ?? "-1");
                userdata.MoverDocumento = Int32.Parse(dt.Rows[0]["MOVER_DOCUMENTO"].ToString() ?? "-1");
                userdata.Valija = Int32.Parse(dt.Rows[0]["VALIJA"].ToString() ?? "-1");
                userdata.ValijaNuevo = Int32.Parse(dt.Rows[0]["VALIJA_NUEVO"].ToString() ?? "-1");
                userdata.ValijaValija = Int32.Parse(dt.Rows[0]["VALIJA_VALIJA"].ToString() ?? "-1");
                userdata.ValijaTransicion = Int32.Parse(dt.Rows[0]["VALIJA_TRANSICION"].ToString() ?? "-1");
                userdata.Pagare = Int32.Parse(dt.Rows[0]["PAGARE"].ToString() ?? "-1");
                userdata.PagareBuscar = Int32.Parse(dt.Rows[0]["PAGARE_BUSCAR"].ToString() ?? "-1");
                userdata.PagareRecibir = Int32.Parse(dt.Rows[0]["PAGARE_RECIBIR"].ToString() ?? "-1");
                userdata.PagareEntregar = Int32.Parse(dt.Rows[0]["PAGARE_ENTREGAR"].ToString() ?? "-1");
                userdata.Letra = Int32.Parse(dt.Rows[0]["LETRA"].ToString() ?? "-1");
                userdata.LetraNuevo = Int32.Parse(dt.Rows[0]["LETRA_NUEVO"].ToString() ?? "-1");
                userdata.LetraEntregar = Int32.Parse(dt.Rows[0]["LETRA_ENTREGAR"].ToString() ?? "-1");
                userdata.LetraReingreso = Int32.Parse(dt.Rows[0]["LETRA_REINGRESO"].ToString() ?? "-1");
                userdata.LetraBuscar = Int32.Parse(dt.Rows[0]["LETRA_BUSCAR"].ToString() ?? "-1");
                userdata.Mantenimiento = Int32.Parse(dt.Rows[0]["MANTENIMIENTO"].ToString() ?? "-1");
                userdata.MantenimientoUsuarioExterno = Int32.Parse(dt.Rows[0]["MANTENIMIENTO_USUARIO_EXTERNO"].ToString() ?? "-1");
                userdata.MantenimientoListas = Int32.Parse(dt.Rows[0]["MANTENIMIENTO_LISTAS"].ToString() ?? "-1");
                userdata.Pendiente = Int32.Parse(dt.Rows[0]["PENDIENTE"].ToString() ?? "-1");
                userdata.PendienteRegularizar = Int32.Parse(dt.Rows[0]["PENDIENTE_REGULARIZAR"].ToString() ?? "-1");
                userdata.Reporte = Int32.Parse(dt.Rows[0]["REPORTE"].ToString() ?? "-1");
                userdata.ReporteCajas = Int32.Parse(dt.Rows[0]["REPORTE_CAJAS"].ToString() ?? "-1");
                userdata.Prestar = Int32.Parse(dt.Rows[0]["PRESTAR"].ToString() ?? "-1");
                userdata.PrestarPrestar = Int32.Parse(dt.Rows[0]["PRESTAR_PRESTAR"].ToString() ?? "-1");
                userdata.PrestarRecibir = Int32.Parse(dt.Rows[0]["PRESTAR_RECIBIR"].ToString() ?? "-1");

                userdata.Token = token;

                return Ok(userdata);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + strSQL);
            }
        }

        private string CreateToken (string username)
        {

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AuthController:key").Value));

            var creds = new SigningCredentials (key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(6),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private static void CreatePasswordHash (string password, out string passwordHash, out string passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = Convert.ToBase64String(hmac.Key);
                passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }

        private static bool VerifyPasswordHash(string password, string passwordHash, string passwordSalt)
        {
            using (var hmac = new HMACSHA512(Convert.FromBase64String(passwordSalt)))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(Convert.FromBase64String(passwordHash));
            }
        }
        
        /*
        [HttpPost("getpasswordhash")]
        public string GetPasswordHash(string password)
        {
            try
            {
                using (var hmac = new HMACSHA512())
                {
                    string passwordSalt = Convert.ToBase64String(hmac.Key);
                    string passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
                    return passwordSalt + ";" + passwordHash;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        */
    }
}
