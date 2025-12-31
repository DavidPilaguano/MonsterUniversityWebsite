using System;
using System.Web.Mvc;

namespace Gr01MonsterUniversity.Controllers
{
    public class EstudianteController : Controller
    {
        public ActionResult Index()
        {
            string rol = Session["UserRole"]?.ToString();

            // Si es Admin, no tiene nada que hacer en el portal de alumnos, 
            // lo mandamos a su gestión de usuarios.
            if (rol == "ADMINISTRADOR")
            {
                return RedirectToAction("Index", "Admin");
            }

            if (string.IsNullOrEmpty(rol) || rol != "ESTUDIANTE")
            {
                return RedirectToAction("Login", "Accounts");
            }

            return View();
        }
    }
}