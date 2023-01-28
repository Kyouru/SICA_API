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
            Cuenta cuenta;
            try
            {
                cuenta = TokenFunctions.ValidarToken(_configuration.GetConnectionString("UserCheck"), request.token);
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

        [HttpPost("login")]
        public IActionResult Login(User request)
        {
            DataTable dt;
            string strSQL = "SELECT U.PASSWORDHASH, U.PASSWORDSALT, U.ID_USUARIO, U.CAMBIAR_PASSWORD, U.ACCESO_PERMITIDO, U.CERRAR_SESION, NVL(P.BUSQUEDA, 0) AS BUSQUEDA, NVL(P.BUSQUEDA_HISTORICO, 0) AS BUSQUEDA_HISTORICO, NVL(P.BUSQUEDA_EDITAR, 0) AS BUSQUEDA_EDITAR, NVL(P.MOVER, 0) MOVER, NVL(P.MOVER_EXPEDIENTE, 0) MOVER_EXPEDIENTE, NVL(P.MOVER_DOCUMENTO, 0) MOVER_DOCUMENTO, NVL(P.MOVER_MASIVO, 0) MOVER_MASIVO, NVL(P.VALIJA, 0) VALIJA, NVL(P.VALIJA_NUEVO, 0) VALIJA_NUEVO , NVL(P.VALIJA_REINGRESO, 0) VALIJA_REINGRESO, NVL(P.VALIJA_CONFIRMAR, 0) VALIJA_CONFIRMAR, NVL(P.VALIJA_MANUAL, 0) VALIJA_MANUAL, NVL(P.PAGARE, 0) PAGARE, NVL(P.PAGARE_BUSCAR, 0) PAGARE_BUSCAR, NVL(P.PAGARE_RECIBIR, 0) PAGARE_RECIBIR, NVL(P.PAGARE_ENTREGAR, 0) PAGARE_ENTREGAR, NVL(P.LETRA, 0) LETRA, NVL(P.LETRA_NUEVO, 0) LETRA_NUEVO, NVL(P.LETRA_ENTREGAR, 0) LETRA_ENTREGAR, NVL(P.LETRA_REINGRESO, 0) LETRA_REINGRESO, NVL(P.LETRA_BUSCAR, 0) LETRA_BUSCAR, NVL(P.MANTENIMIENTO, 0) MANTENIMIENTO, NVL(P.MANTENIMIENTO_USUARIO_EXTERNO, 0) MANTENIMIENTO_USUARIO_EXTERNO, NVL(P.MANTENIMIENTO_SOCIO, 0) MANTENIMIENTO_SOCIO, NVL(P.MANTENIMIENTO_CREDITO, 0) MANTENIMIENTO_CREDITO, NVL(P.PENDIENTE, 0) PENDIENTE, NVL(P.PENDIENTE_REGULARIZAR, 0) PENDIENTE_REGULARIZAR, NVL(P.REPORTE, 0) REPORTE, NVL(P.REPORTE_CAJAS, 0) REPORTE_CAJAS, NVL(P.PRESTAR, 0) PRESTAR, NVL(P.PRESTAR_PRESTAR, 0) PRESTAR_PRESTAR, NVL(P.PRESTAR_RECIBIR, 0) PRESTAR_RECIBIR,  NVL(P.NIVEL, 0) AS NIVEL FROM ADMIN.USUARIO U LEFT JOIN ADMIN.PERMISO P ON U.ID_USUARIO = P.ID_USUARIO_FK WHERE U.NOMBRE_USUARIO = '" + request.Username + "'";
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

                strSQL = "UPDATE ADMIN.USUARIO SET JWT = '" + token + "' WHERE NOMBRE_USUARIO = '" + request.Username + "'";

                conn.EjecutarQuery(strSQL);
                conn.Cerrar();
            }
            catch (Exception ex)
            {
                conn.Cerrar();
                return BadRequest(ex.Message);
            }

            try
            {
                UserData userdata = new UserData();

                userdata.Username = request.Username;
                userdata.IdUser = Int32.Parse(dt.Rows[0]["ID_USUARIO"].ToString() ?? "NULL");
                userdata.CambiarPassword = Int32.Parse(dt.Rows[0]["CAMBIAR_PASSWORD"].ToString() ?? "NULL");
                userdata.AccesoPermitido = Int32.Parse(dt.Rows[0]["ACCESO_PERMITIDO"].ToString() ?? "NULL");
                userdata.CerrarSesion = Int32.Parse(dt.Rows[0]["CERRAR_SESION"].ToString() ?? "NULL");

                userdata.Busqueda = Int32.Parse(dt.Rows[0]["BUSQUEDA"].ToString() ?? "NULL");
                userdata.BusquedaHistorico = Int32.Parse(dt.Rows[0]["BUSQUEDA_HISTORICO"].ToString() ?? "NULL");
                userdata.BusquedaEditar = Int32.Parse(dt.Rows[0]["BUSQUEDA_EDITAR"].ToString() ?? "NULL");
                userdata.Mover = Int32.Parse(dt.Rows[0]["MOVER"].ToString() ?? "NULL");
                userdata.MoverExpediente = Int32.Parse(dt.Rows[0]["MOVER_EXPEDIENTE"].ToString() ?? "NULL");
                userdata.MoverDocumento = Int32.Parse(dt.Rows[0]["MOVER_DOCUMENTO"].ToString() ?? "NULL");
                userdata.Valija = Int32.Parse(dt.Rows[0]["VALIJA"].ToString() ?? "NULL");
                userdata.ValijaNuevo = Int32.Parse(dt.Rows[0]["VALIJA_NUEVO"].ToString() ?? "NULL");
                userdata.ValijaReingreso = Int32.Parse(dt.Rows[0]["VALIJA_REINGRESO"].ToString() ?? "NULL");
                userdata.ValijaConfirmar = Int32.Parse(dt.Rows[0]["VALIJA_CONFIRMAR"].ToString() ?? "NULL");
                userdata.ValijaManual = Int32.Parse(dt.Rows[0]["VALIJA_MANUAL"].ToString() ?? "NULL");
                userdata.Pagare = Int32.Parse(dt.Rows[0]["PAGARE"].ToString() ?? "NULL");
                userdata.PagareBuscar = Int32.Parse(dt.Rows[0]["PAGARE_BUSCAR"].ToString() ?? "NULL");
                userdata.PagareRecibir = Int32.Parse(dt.Rows[0]["PAGARE_RECIBIR"].ToString() ?? "NULL");
                userdata.PagareEntregar = Int32.Parse(dt.Rows[0]["PAGARE_ENTREGAR"].ToString() ?? "NULL");
                userdata.Letra = Int32.Parse(dt.Rows[0]["LETRA"].ToString() ?? "NULL");
                userdata.LetraNuevo = Int32.Parse(dt.Rows[0]["LETRA_NUEVO"].ToString() ?? "NULL");
                userdata.LetraEntregar = Int32.Parse(dt.Rows[0]["LETRA_ENTREGAR"].ToString() ?? "NULL");
                userdata.LetraReingreso = Int32.Parse(dt.Rows[0]["LETRA_REINGRESO"].ToString() ?? "NULL");
                userdata.LetraBuscar = Int32.Parse(dt.Rows[0]["LETRA_BUSCAR"].ToString() ?? "NULL");
                userdata.Mantenimiento = Int32.Parse(dt.Rows[0]["MANTENIMIENTO"].ToString() ?? "NULL");
                userdata.MantenimientoCredito = Int32.Parse(dt.Rows[0]["MANTENIMIENTO_CREDITO"].ToString() ?? "NULL");
                userdata.MantenimientoUsuarioExterno = Int32.Parse(dt.Rows[0]["MANTENIMIENTO_USUARIO_EXTERNO"].ToString() ?? "NULL");
                userdata.MantenimientoSocio = Int32.Parse(dt.Rows[0]["MANTENIMIENTO_SOCIO"].ToString() ?? "NULL");
                userdata.Pendiente = Int32.Parse(dt.Rows[0]["PENDIENTE"].ToString() ?? "NULL");
                userdata.PendienteRegularizar = Int32.Parse(dt.Rows[0]["PENDIENTE_REGULARIZAR"].ToString() ?? "NULL");
                userdata.Reporte = Int32.Parse(dt.Rows[0]["REPORTE"].ToString() ?? "NULL");
                userdata.ReporteCajas = Int32.Parse(dt.Rows[0]["REPORTE_CAJAS"].ToString() ?? "NULL");
                userdata.Prestar = Int32.Parse(dt.Rows[0]["PRESTAR"].ToString() ?? "NULL");
                userdata.PrestarPrestar = Int32.Parse(dt.Rows[0]["PRESTAR_PRESTAR"].ToString() ?? "NULL");
                userdata.PrestarRecibir = Int32.Parse(dt.Rows[0]["PRESTAR_RECIBIR"].ToString() ?? "NULL");

                userdata.Nivel = Int32.Parse(dt.Rows[0]["NIVEL"].ToString() ?? "NULL");

                userdata.Token = token;

                return Ok(userdata);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
