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

                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();
                conn.cerrar();

                return Ok();
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public IActionResult Login(User request)
        {
            DataTable dt = new DataTable("User");
            string strSQL = "SELECT U.PASSWORDHASH, U.PASSWORDSALT, U.ID_AREA_FK, U.ID_USUARIO, U.CAMBIAR_PASSWORD, U.ACCESO_PERMITIDO, U.CERRAR_SESION, NVL(P.BUSQUEDA, 0) AS BUSQUEDA, NVL(P.BUSQUEDA_HISTORICO, 0) AS BUSQUEDA_HISTORICO, NVL(P.BUSQUEDA_EDITAR, 0) AS BUSQUEDA_EDITAR, NVL(P.MOVER, 0) MOVER, NVL(P.MOVER_EXPEDIENTE, 0) MOVER_EXPEDIENTE, NVL(P.MOVER_DOCUMENTO, 0) MOVER_DOCUMENTO, NVL(P.MOVER_MASIVO, 0) MOVER_MASIVO, NVL(P.VALIJA, 0) VALIJA, NVL(P.VALIJA_NUEVO, 0) VALIJA_NUEVO , NVL(P.VALIJA_REINGRESO, 0) VALIJA_REINGRESO, NVL(P.VALIJA_CONFIRMAR, 0) VALIJA_CONFIRMAR, NVL(P.VALIJA_MANUAL, 0) VALIJA_MANUAL, NVL(P.PAGARE, 0) PAGARE, NVL(P.PAGARE_BUSCAR, 0) PAGARE_BUSCAR, NVL(P.PAGARE_RECIBIR, 0) PAGARE_RECIBIR, NVL(P.PAGARE_ENTREGAR, 0) PAGARE_ENTREGAR, NVL(P.LETRA, 0) LETRA, NVL(P.LETRA_NUEVO, 0) LETRA_NUEVO, NVL(P.LETRA_ENTREGAR, 0) LETRA_ENTREGAR, NVL(P.LETRA_REINGRESO, 0) LETRA_REINGRESO, NVL(P.LETRA_BUSCAR, 0) LETRA_BUSCAR, NVL(P.IRONMOUNTAIN, 0) IRONMOUNTAIN, NVL(P.IRONMOUNTAIN_SOLICITAR, 0) IRONMOUNTAIN_SOLICITAR, NVL(P.IRONMOUNTAIN_RECIBIR, 0) IRONMOUNTAIN_RECIBIR, NVL(P.IRONMOUNTAIN_ARMAR, 0) IRONMOUNTAIN_ARMAR, NVL(P.IRONMOUNTAIN_ENVIAR, 0) IRONMOUNTAIN_ENVIAR, NVL(P.IRONMOUNTAIN_ENTREGAR, 0) IRONMOUNTAIN_ENTREGAR, NVL(P.IRONMOUNTAIN_CARGO, 0) IRONMOUNTAIN_CARGO, NVL(P.BOVEDA, 0) BOVEDA, NVL(P.BOVEDA_CAJA_RETIRAR, 0) BOVEDA_CAJA_RETIRAR, NVL(P.BOVEDA_CAJA_GUARDAR, 0) BOVEDA_CAJA_GUARDAR, NVL(P.BOVEDA_DOCUMENTO_RETIRAR, 0) BOVEDA_DOCUMENTO_RETIRAR, NVL(P.BOVEDA_DOCUMENTO_GUARDAR, 0) BOVEDA_DOCUMENTO_GUARDAR, NVL(P.IMPORTAR, 0) IMPORTAR, NVL(P.IMPORTAR_ACTIVAS, 0) IMPORTAR_ACTIVAS, NVL(P.IMPORTAR_PASIVAS, 0) IMPORTAR_PASIVAS, NVL(P.MANTENIMIENTO, 0) MANTENIMIENTO, NVL(P.MANTENIMIENTO_USUARIO_EXTERNO, 0) MANTENIMIENTO_USUARIO_EXTERNO, NVL(P.MANTENIMIENTO_SOCIO, 0) MANTENIMIENTO_SOCIO, NVL(P.MANTENIMIENTO_CREDITO, 0) MANTENIMIENTO_CREDITO, NVL(P.PENDIENTE, 0) PENDIENTE, NVL(P.PENDIENTE_REGULARIZAR, 0) PENDIENTE_REGULARIZAR, NVL(P.REPORTE, 0) REPORTE, NVL(P.REPORTE_CAJAS, 0) REPORTE_CAJAS, NVL(P.PRESTAR, 0) PRESTAR, NVL(P.PRESTAR_PRESTAR, 0) PRESTAR_PRESTAR, NVL(P.PRESTAR_RECIBIR, 0) PRESTAR_RECIBIR,  NVL(P.NIVEL, 0) AS NIVEL FROM ADMIN.USUARIO U LEFT JOIN ADMIN.PERMISO P ON U.ID_USUARIO = P.ID_USUARIO_FK WHERE U.NOMBRE_USUARIO = '" + request.Username + "'";
            string token;
            Conexion conn = new Conexion();
            try
            {
                conn = new Conexion(_configuration.GetConnectionString("UserCheck"));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();
                dt = conn.llenarDataTable();
                conn.cerrar();

                if (dt.Rows.Count <= 0)
                {
                    return BadRequest("Usuario no Encontrado");
                }

                if (!VerifyPasswordHash(request.Password, dt.Rows[0]["PASSWORDHASH"].ToString(), dt.Rows[0]["PASSWORDSALT"].ToString()))
                {
                    return BadRequest("Contraseña Errada");
                }

                token = CreateToken(request.Username);

                strSQL = "UPDATE ADMIN.USUARIO SET JWT = '" + token + "' WHERE NOMBRE_USUARIO = '" + request.Username + "'";

                conn = new Conexion(_configuration.GetConnectionString("UserCheck"));
                conn.conectar();
                conn.iniciaCommand(strSQL);
                conn.ejecutarQuery();
                conn.cerrar();
            }
            catch (Exception ex)
            {
                conn.cerrar();
                return BadRequest(ex.Message);
            }

            try
            {
                UserData userdata = new UserData();

                userdata.Username = request.Username;
                userdata.IdUser = Int32.Parse(dt.Rows[0]["ID_USUARIO"].ToString());
                userdata.IdArea = Int32.Parse(dt.Rows[0]["ID_AREA_FK"].ToString());
                userdata.CambiarPassword = Int32.Parse(dt.Rows[0]["CAMBIAR_PASSWORD"].ToString());
                userdata.AccesoPermitido = Int32.Parse(dt.Rows[0]["ACCESO_PERMITIDO"].ToString());
                userdata.CerrarSesion = Int32.Parse(dt.Rows[0]["CERRAR_SESION"].ToString());

                userdata.auBusqueda = Int32.Parse(dt.Rows[0]["BUSQUEDA"].ToString());
                userdata.auBusquedaHistorico = Int32.Parse(dt.Rows[0]["BUSQUEDA_HISTORICO"].ToString());
                userdata.auBusquedaEditar = Int32.Parse(dt.Rows[0]["BUSQUEDA_EDITAR"].ToString());
                userdata.auMover = Int32.Parse(dt.Rows[0]["MOVER"].ToString());
                userdata.auMoverExpediente = Int32.Parse(dt.Rows[0]["MOVER_EXPEDIENTE"].ToString());
                userdata.auMoverDocumento = Int32.Parse(dt.Rows[0]["MOVER_DOCUMENTO"].ToString());
                userdata.auValija = Int32.Parse(dt.Rows[0]["VALIJA"].ToString());
                userdata.auValijaNuevo = Int32.Parse(dt.Rows[0]["VALIJA_NUEVO"].ToString());
                userdata.auValijaReingreso = Int32.Parse(dt.Rows[0]["VALIJA_REINGRESO"].ToString());
                userdata.auValijaConfirmar = Int32.Parse(dt.Rows[0]["VALIJA_CONFIRMAR"].ToString());
                userdata.auValijaManual = Int32.Parse(dt.Rows[0]["VALIJA_MANUAL"].ToString());
                userdata.auPagare = Int32.Parse(dt.Rows[0]["PAGARE"].ToString());
                userdata.auPagareBuscar = Int32.Parse(dt.Rows[0]["PAGARE_BUSCAR"].ToString());
                userdata.auPagareRecibir = Int32.Parse(dt.Rows[0]["PAGARE_RECIBIR"].ToString());
                userdata.auPagareEntregar = Int32.Parse(dt.Rows[0]["PAGARE_ENTREGAR"].ToString());
                userdata.auLetra = Int32.Parse(dt.Rows[0]["LETRA"].ToString());
                userdata.auLetraNuevo = Int32.Parse(dt.Rows[0]["LETRA_NUEVO"].ToString());
                userdata.auLetraEntregar = Int32.Parse(dt.Rows[0]["LETRA_ENTREGAR"].ToString());
                userdata.auLetraReingreso = Int32.Parse(dt.Rows[0]["LETRA_REINGRESO"].ToString());
                userdata.auLetraBuscar = Int32.Parse(dt.Rows[0]["LETRA_BUSCAR"].ToString());
                userdata.auIronMountain = Int32.Parse(dt.Rows[0]["IRONMOUNTAIN"].ToString());
                userdata.auIronMountainSolicitar = Int32.Parse(dt.Rows[0]["IRONMOUNTAIN_SOLICITAR"].ToString());
                userdata.auIronMountainRecibir = Int32.Parse(dt.Rows[0]["IRONMOUNTAIN_RECIBIR"].ToString());
                userdata.auIronMountainArmar = Int32.Parse(dt.Rows[0]["IRONMOUNTAIN_ARMAR"].ToString());
                userdata.auIronMountainEnviar = Int32.Parse(dt.Rows[0]["IRONMOUNTAIN_ENVIAR"].ToString());
                userdata.auIronMountainEntregar = Int32.Parse(dt.Rows[0]["IRONMOUNTAIN_ENTREGAR"].ToString());
                userdata.auIronMountainCargo = Int32.Parse(dt.Rows[0]["IRONMOUNTAIN_CARGO"].ToString());
                userdata.auBoveda = Int32.Parse(dt.Rows[0]["BOVEDA"].ToString());
                userdata.auBovedaCajaRetirar = Int32.Parse(dt.Rows[0]["BOVEDA_CAJA_RETIRAR"].ToString());
                userdata.auBovedaCajaGuardar = Int32.Parse(dt.Rows[0]["BOVEDA_CAJA_GUARDAR"].ToString());
                userdata.auBovedaDocumentoRetirar = Int32.Parse(dt.Rows[0]["BOVEDA_DOCUMENTO_RETIRAR"].ToString());
                userdata.auBovedaDocumentoGuardar = Int32.Parse(dt.Rows[0]["BOVEDA_DOCUMENTO_GUARDAR"].ToString());
                userdata.auImportar = Int32.Parse(dt.Rows[0]["IMPORTAR"].ToString());
                userdata.auImportarActivas = Int32.Parse(dt.Rows[0]["IMPORTAR_ACTIVAS"].ToString());
                userdata.auImportarPasivas = Int32.Parse(dt.Rows[0]["IMPORTAR_PASIVAS"].ToString());
                userdata.auMantenimiento = Int32.Parse(dt.Rows[0]["MANTENIMIENTO"].ToString());
                userdata.auMantenimientoCredito = Int32.Parse(dt.Rows[0]["MANTENIMIENTO_CREDITO"].ToString());
                userdata.auMantenimientoUsuarioExterno = Int32.Parse(dt.Rows[0]["MANTENIMIENTO_USUARIO_EXTERNO"].ToString());
                userdata.auMantenimientoSocio = Int32.Parse(dt.Rows[0]["MANTENIMIENTO_SOCIO"].ToString());
                userdata.auPendiente = Int32.Parse(dt.Rows[0]["PENDIENTE"].ToString());
                userdata.auPendienteRegularizar = Int32.Parse(dt.Rows[0]["PENDIENTE_REGULARIZAR"].ToString());
                userdata.auReporte = Int32.Parse(dt.Rows[0]["REPORTE"].ToString());
                userdata.auReporteCajas = Int32.Parse(dt.Rows[0]["REPORTE_CAJAS"].ToString());
                userdata.auPrestar = Int32.Parse(dt.Rows[0]["PRESTAR"].ToString());
                userdata.auPrestarPrestar = Int32.Parse(dt.Rows[0]["PRESTAR_PRESTAR"].ToString());
                userdata.auPrestarRecibir = Int32.Parse(dt.Rows[0]["PRESTAR_RECIBIR"].ToString());

                userdata.auNivel = Int32.Parse(dt.Rows[0]["NIVEL"].ToString());

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

        private void CreatePasswordHash (string password, out string passwordHash, out string passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = Convert.ToBase64String(hmac.Key);
                passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }

        private bool VerifyPasswordHash(string password, string passwordHash, string passwordSalt)
        {
            using (var hmac = new HMACSHA512(Convert.FromBase64String(passwordSalt)))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(Convert.FromBase64String(passwordHash));
            }
        }

        [HttpPost("getpasswordhash")]
        public string getPasswordHash(string password)
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
    }
}
