using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gr01MonsterUniversity.Models;

namespace Gr01MonsterUniversity.Controllers
{
    public class HomeController : Controller
    {
        private monster_universityEntities db = new monster_universityEntities();

        public ActionResult Index()
        {
            // 1. SEGURIDAD: Si no hay sesión, nadie entra al Inicio, van al Login
            if (Session["UserRole"] == null)
            {
                return RedirectToAction("Login", "Accounts");
            }

            string rol = Session["UserRole"].ToString();

            try
            {
                // 2. REDIRECCIÓN POR ROL: 
                // Si el Estudiante o Docente pulsan "Inicio", los mandamos a su portal específico
                // para que no vean una página en blanco o sin sentido.
                if (rol == "ESTUDIANTE")
                {
                    return RedirectToAction("Index", "Estudiante");
                }
                if (rol == "DOCENTE")
                {
                    return RedirectToAction("Index", "Docente");
                }

                // 3. LÓGICA EXCLUSIVA PARA ADMINISTRADOR (Dashboard)
                if (rol == "ADMINISTRADOR")
                {
                    var totalEmpleados = db.PEEEMP_EMPLE.Count();
                    ViewBag.MensajeConexion = "✅ Panel de Control - Total Empleados: " + totalEmpleados;

                    // Agrupamos los usuarios por el nombre de su perfil para la gráfica
                    var estadisticasRoles = db.XEUXP_USUPE
                        .GroupBy(u => u.XEPER_PERFI.XEPER_DESCRI)
                        .Select(grupo => new
                        {
                            NombreRol = grupo.Key,
                            Cantidad = grupo.Count()
                        })
                        .ToList();

                    // Preparamos los datos para Chart.js en la Vista
                    ViewBag.Labels = estadisticasRoles.Select(x => x.NombreRol).ToArray();
                    ViewBag.Valores = estadisticasRoles.Select(x => x.Cantidad).ToArray();
                }
            }
            catch (Exception ex)
            {
                ViewBag.MensajeConexion = "❌ Error al cargar estadísticas: " + ex.Message;
            }

            return View();
        }

        public ActionResult WorkingInProgress()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}