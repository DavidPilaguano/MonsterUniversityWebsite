using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Gr01MonsterUniversity.Models;
using System.Data.Entity;
using System.Net;
using System.Net.Mail;

namespace Gr01MonsterUniversity.Controllers
{
    public class AccountsController : Controller
    {
        private monster_universityEntities db = new monster_universityEntities();

        [HttpGet]
        public ActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Buscamos al usuario y cargamos sus relaciones necesarias
                // Nota: Asegúrate que XEUSU_PASWD sea el nombre correcto de la columna en tu DB
                var user = db.XEUSU_USUAR
                             .Include(u => u.PEEEMP_EMPLE)
                             .Include(u => u.XEUXP_USUPE)
                             .FirstOrDefault(u => u.PEEEMP_CODIGO == model.Usuario && u.XEUSU_PASWD == model.Password);

                if (user != null)
                {
                    var registroPerfil = user.XEUXP_USUPE.FirstOrDefault();

                    if (registroPerfil != null)
                    {
                        // Convertimos a string el código del perfil y quitamos espacios en blanco
                        string codPerfil = registroPerfil.XEPER_CODIGO.ToString().Trim();

                        // 2. Buscamos la descripción del perfil para identificar el ROL
                        var perfilInfo = db.XEPER_PERFI.Find(codPerfil);
                        string rol = perfilInfo?.XEPER_DESCRI?.Trim().ToUpper() ?? "INVITADO";

                        // VARIABLES DE SESIÓN CRÍTICAS
                        Session["UserRole"] = rol;
                        Session["PerfilId"] = codPerfil; // Esta la usaremos para comparar en AdminController

                        // 3. Obtener Menú Dinámico mediante SQL Directo
                        // Traemos solo los códigos de las opciones permitidas para este perfil
                        string sql = "SELECT XEOPC_CODIGO FROM XEOXP_OPCPE WHERE XEPER_CODIGO = @p0";
                        var idsOpciones = db.Database.SqlQuery<string>(sql, codPerfil).ToList();

                        // 4. Buscamos los objetos de las opciones permitidas
                        // Limpiamos espacios para que el .Contains no falle
                        var opcionesPermitidas = db.XEOPC_OPCIO
                                                   .ToList() // Traemos a memoria para limpiar strings si es necesario
                                                   .Where(o => idsOpciones.Any(id => id.Trim() == o.XEOPC_CODIGO.Trim()))
                                                   .ToList();

                        Session["MenuDinamico"] = opcionesPermitidas;
                    }

                    // 5. Datos personales del empleado para el Layout
                    if (user.PEEEMP_EMPLE != null)
                    {
                        Session["UserName"] = user.PEEEMP_EMPLE.PEEEMP_NOMBRES + " " + user.PEEEMP_EMPLE.PEEEMP_APELLIDOS;
                        Session["UserFoto"] = !string.IsNullOrEmpty(user.PEEEMP_EMPLE.PEEMP_FOTO)
                                              ? user.PEEEMP_EMPLE.PEEMP_FOTO
                                              : "/Uploads/Fotos/default.png";
                    }

                    // --- LÓGICA DE REDIRECCIÓN ---
                    string userRol = Session["UserRole"]?.ToString();

                    if (userRol == "ADMINISTRADOR")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (userRol == "DOCENTE")
                    {
                        return RedirectToAction("Index", "Docente");
                    }
                    else if (userRol == "ESTUDIANTE")
                    {
                        return RedirectToAction("Index", "Estudiante");
                    }

                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Credenciales incorrectas o usuario inactivo";
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Recuperar() { return View(); }

        [HttpPost]
        public ActionResult RecuperarPassword(string emailDestino)
        {
            try
            {
                var empleado = db.PEEEMP_EMPLE.FirstOrDefault(e => e.PEEEMP_EMAIL == emailDestino);
                if (empleado != null)
                {
                    var codigoEmp = empleado.PEEEMP_CODIGO;
                    var usuario = db.XEUSU_USUAR.FirstOrDefault(u => u.PEEEMP_CODIGO == codigoEmp);

                    if (usuario != null)
                    {
                        string nuevaClave = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                        db.Database.ExecuteSqlCommand(
                            "UPDATE XEUSU_USUAR SET XEUSU_PASWD = @p0 WHERE PEEEMP_CODIGO = @p1",
                            nuevaClave, codigoEmp
                        );

                        EnviarEmailFuncional(empleado.PEEEMP_EMAIL, empleado.PEEEMP_NOMBRES, nuevaClave);

                        TempData["Mensaje"] = "¡Éxito! Revisa tu bandeja de entrada.";
                        return RedirectToAction("Recuperar");
                    }
                }
                TempData["Error"] = "El correo no coincide con nuestros registros.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al procesar la solicitud: " + ex.Message;
            }
            return RedirectToAction("Recuperar");
        }

        private void EnviarEmailFuncional(string destino, string nombreUsuario, string clave)
        {
            string correoEmisor = "soporte.monsteruniversitygr1@gmail.com";
            string passwordAplicacion = "vsmy xozt pdqv xybo";

            MailMessage mm = new MailMessage(correoEmisor, destino);
            mm.Subject = "Restablecimiento de Contraseña - Monster University";
            mm.Body = $"<h2>Hola {nombreUsuario}</h2><p>Tu nueva clave de acceso es: <b>{clave}</b></p><br><p>Por favor, cámbiala al ingresar.</p>";
            mm.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(correoEmisor, passwordAplicacion),
                EnableSsl = true
            };
            smtp.Send(mm);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}