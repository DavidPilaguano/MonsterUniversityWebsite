using System;
using System.Web.Mvc;

namespace Gr01MonsterUniversity.Controllers
{
    public class DocenteController : Controller
    {
        // GET: Docente
        public ActionResult Index()
        {
            // Validamos que exista una sesión y que el rol sea DOCENTE
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "DOCENTE")
            {
                return RedirectToAction("Login", "Accounts");
            }

            ViewBag.Message = "Panel de Gestión Docente";
            return View();
        }
    }
}

