using Gr01MonsterUniversity.Models;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Gr01MonsterUniversity.Controllers
{
    public class AdminController : Controller
    {
        private monster_universityEntities db = new monster_universityEntities();

        // ==========================================
        // VISTA PRINCIPAL / DASHBOARD
        // ==========================================
        public ActionResult Index()
        {
            var usuarios = db.XEUSU_USUAR.Include("PEEEMP_EMPLE").ToList();
            return View(usuarios);
        }

        // ==========================================
        // GESTIÓN DE USUARIOS
        // ==========================================
        public ActionResult Usuarios()
        {
            var usuarios = db.XEUSU_USUAR
                             .Include(u => u.PEEEMP_EMPLE)
                             .Include(u => u.XEUXP_USUPE.Select(p => p.XEPER_PERFI))
                             .ToList();
            return View(usuarios);
        }

        public ActionResult Create()
        {
            CargarDesplegables();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UsuarioCompletoViewModel model, HttpPostedFileBase fotoFile)
        {
            if (ModelState.IsValid)
            {
                using (var trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        var emp = new PEEEMP_EMPLE
                        {
                            PEEEMP_CODIGO = model.PEEEMP_CODIGO,
                            PEEEMP_CEDULA = model.PEEEMP_CEDULA,
                            PEEEMP_NOMBRES = model.PEEEMP_NOMBRES,
                            PEEEMP_APELLIDOS = model.PEEEMP_APELLIDOS,
                            PEEEMP_EMAIL = model.PEEEMP_EMAIL,
                            PEEEMP_FECHANAC = model.PEEEMP_FECHANAC,
                            PEEEMP_DIRECCION = model.PEEEMP_DIRECCION,
                            PEEEMP_TELEFONO = model.PEEEMP_TELEFONO,
                            PEEEMP_CELULAR = model.PEEEMP_CELULAR,
                            PEEEMP_HIJOS = model.PEEEMP_HIJOS ?? 0,
                            PEPSEX_CODIGO = model.PEPSEX_CODIGO,
                            PEESC_CODIGO = model.PEESC_CODIGO
                        };

                        if (fotoFile != null && fotoFile.ContentLength > 0)
                        {
                            string fileName = model.PEEEMP_CODIGO + Path.GetExtension(fotoFile.FileName);
                            string carpeta = Server.MapPath("~/Uploads/Fotos/");
                            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);
                            fotoFile.SaveAs(Path.Combine(carpeta, fileName));
                            emp.PEEMP_FOTO = "/Uploads/Fotos/" + fileName;
                        }
                        else emp.PEEMP_FOTO = "/Uploads/Fotos/default.png";

                        var usu = new XEUSU_USUAR
                        {
                            PEEEMP_CODIGO = model.PEEEMP_CODIGO,
                            XEUSU_PASWD = model.XEUSU_PASWD,
                            XEEST_CODIGO = Request.Form["XEEST_CODIGO"] ?? "1",
                            XEUSU_PIEFIR = model.XEUSU_PIEFIR,
                            XEUSU_FECCRE = DateTime.Now,
                            XEUSU_REQ_CAMBIO = 0
                        };

                        var uxp = new XEUXP_USUPE
                        {
                            PEEEMP_CODIGO = model.PEEEMP_CODIGO,
                            XEUSU_PASWD = model.XEUSU_PASWD,
                            XEPER_CODIGO = model.XEPER_CODIGO,
                            XEUXP_FECASI = DateTime.Now
                        };

                        db.PEEEMP_EMPLE.Add(emp);
                        db.XEUSU_USUAR.Add(usu);
                        db.XEUXP_USUPE.Add(uxp);
                        db.SaveChanges();
                        trans.Commit();
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        ModelState.AddModelError("", "Error: " + ex.Message);
                    }
                }
            }
            CargarDesplegables(model);
            return View(model);
        }

        // ==========================================
        // DUAL LIST BOX (ASIGNAR ROLES A USUARIOS)
        // ==========================================
        public ActionResult AsignarPerfiles()
        {
            ViewBag.Perfiles = new SelectList(db.XEPER_PERFI, "XEPER_CODIGO", "XEPER_DESCRI");
            return View(new AsignacionPerfilesViewModel());
        }

        [HttpPost]
        public JsonResult ObtenerListasUsuarios(string perfilId)
        {
            if (string.IsNullOrEmpty(perfilId))
                return Json(new { disponibles = new List<object>(), asignados = new List<object>() });

            var asignadosIds = db.XEUXP_USUPE
                .Where(x => x.XEPER_CODIGO.Trim() == perfilId.Trim())
                .Select(x => x.PEEEMP_CODIGO)
                .ToList();

            var disponibles = db.XEUSU_USUAR
                .Where(u => !asignadosIds.Contains(u.PEEEMP_CODIGO))
                .Select(u => new {
                    id = u.PEEEMP_CODIGO.Trim(),
                    nombre = u.PEEEMP_CODIGO.Trim() + " - " + (u.PEEEMP_EMPLE.PEEEMP_NOMBRES ?? "Sin Nombre")
                }).ToList();

            var asignados = db.XEUSU_USUAR
                .Where(u => asignadosIds.Contains(u.PEEEMP_CODIGO))
                .Select(u => new {
                    id = u.PEEEMP_CODIGO.Trim(),
                    nombre = u.PEEEMP_CODIGO.Trim() + " - " + (u.PEEEMP_EMPLE.PEEEMP_NOMBRES ?? "Sin Nombre")
                }).ToList();

            return Json(new { disponibles, asignados });
        }

        // Reemplaza estos métodos en tu AdminController.cs

        [HttpPost]
        public JsonResult GuardarAsignacion(string perfilId, List<string> usuarios)
        {
            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    string idPerfilDestino = perfilId.Trim();

                    // 1. Limpiamos a los usuarios que actualmente están en ESTE perfil
                    var actualesEnEstePerfil = db.XEUXP_USUPE
                                                 .Where(x => x.XEPER_CODIGO.Trim() == idPerfilDestino)
                                                 .ToList();
                    db.XEUXP_USUPE.RemoveRange(actualesEnEstePerfil);
                    db.SaveChanges();

                    if (usuarios != null)
                    {
                        foreach (var userCode in usuarios)
                        {
                            string code = userCode.Trim();

                            // 2. CORRECCIÓN CLAVE: Borrar al usuario de CUALQUIER otro perfil previo
                            // Esto evita el error de que un usuario aparezca con 2 o más roles.
                            var asignacionesViejas = db.XEUXP_USUPE
                                                       .Where(x => x.PEEEMP_CODIGO.Trim() == code)
                                                       .ToList();
                            db.XEUXP_USUPE.RemoveRange(asignacionesViejas);
                            db.SaveChanges();

                            var usuarioOriginal = db.XEUSU_USUAR.FirstOrDefault(u => u.PEEEMP_CODIGO.Trim() == code);

                            if (usuarioOriginal != null)
                            {
                                db.XEUXP_USUPE.Add(new XEUXP_USUPE
                                {
                                    XEPER_CODIGO = idPerfilDestino,
                                    PEEEMP_CODIGO = code,
                                    XEUSU_PASWD = usuarioOriginal.XEUSU_PASWD,
                                    XEUXP_FECASI = DateTime.Now
                                });
                            }
                        }
                    }

                    db.SaveChanges();
                    trans.Commit();
                    return Json(new { success = true, message = "Asignación actualizada correctamente." });
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return Json(new { success = false, message = "Error: " + ex.Message });
                }
            }
        }


        // ============================================================
        // GESTIÓN DE PERMISOS (ACCESO A MÓDULOS POR ROL)
        // ============================================================

        // 1. Carga la página principal de permisos
        public ActionResult GestionarPermisos()
        {
            var perfiles = db.XEPER_PERFI.ToList();
            return View(perfiles);
        }

        // 2. AJAX: Obtiene los módulos y marca los que el perfil ya tiene
        [HttpPost]
        public JsonResult ObtenerEstructuraPermisos(string idPerfil)
        {
            // 1. Obtener todas las opciones disponibles (esto sí suele estar en el modelo)
            var todasLasOpciones = db.XEOPC_OPCIO.ToList();

            // 2. Obtener los códigos asignados usando SQL DIRECTO (evita el error de InvalidOperation)
            var asignadas = db.Database.SqlQuery<string>(
                "SELECT CAST(XEOPC_CODIGO AS VARCHAR) FROM XEOXP_OPCPE WHERE XEPER_CODIGO = @p0",
                idPerfil
            ).ToList().Select(s => s.Trim()).ToList();

            // 3. Cruzar los datos
            var listaViewModel = todasLasOpciones.Select(o => new {
                CodigoOpcion = o.XEOPC_CODIGO.Trim(),
                Descripcion = o.XEOPC_DESCRI,
                Asignada = asignadas.Contains(o.XEOPC_CODIGO.Trim())
            }).ToList();

            return Json(new
            {
                NombrePerfil = db.XEPER_PERFI.Find(idPerfil)?.XEPER_DESCRI ?? idPerfil,
                Opciones = listaViewModel
            });
        }

        // 3. POST: Guarda los cambios (Borra actuales e inserta nuevos seleccionados)
        [HttpPost]
        public JsonResult GuardarPermisosModulos(string CodigoPerfil, List<string> opcionesSeleccionadas)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Limpiamos espacios por si vienen de la DB como CHAR
                    string idPerfilLimpio = CodigoPerfil.Trim();

                    // 2. LIMPIEZA Y GUARDADO EN DB (SQL Puro)
                    db.Database.ExecuteSqlCommand("DELETE FROM XEOXP_OPCPE WHERE XEPER_CODIGO = @p0", idPerfilLimpio);

                    if (opcionesSeleccionadas != null)
                    {
                        foreach (var codOpc in opcionesSeleccionadas)
                        {
                            db.Database.ExecuteSqlCommand(
                                "INSERT INTO XEOXP_OPCPE (XEPER_CODIGO, XEOPC_CODIGO) VALUES (@p0, @p1)",
                                idPerfilLimpio, codOpc.Trim());
                        }
                    }

                    // Guardamos cambios en la base de datos
                    dbContextTransaction.Commit();

                    // 3. ACTUALIZACIÓN DE SESIÓN (Tiempo Real)
                    // IMPORTANTE: Usamos "PerfilId" porque así lo definimos en AccountsController
                    string miPerfilActual = Session["PerfilId"]?.ToString();

                    if (miPerfilActual == idPerfilLimpio)
                    {
                        // Volvemos a consultar los permisos reales de la base de datos
                        var menuActualizado = db.Database.SqlQuery<Gr01MonsterUniversity.Models.XEOPC_OPCIO>(@"
                    SELECT O.* FROM XEOPC_OPCIO O
                    INNER JOIN XEOXP_OPCPE P ON O.XEOPC_CODIGO = P.XEOPC_CODIGO
                    WHERE P.XEPER_CODIGO = @p0", idPerfilLimpio).ToList();

                        // REEMPLAZAMOS la sesión vieja por la nueva
                        Session["MenuDinamico"] = menuActualizado;
                    }

                    return Json(new { success = true, mensaje = "Cambios aplicados globalmente. El menú se actualizará al finalizar." });
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return Json(new { success = false, mensaje = "Error: " + ex.Message });
                }
            }
        }
        // ==========================================
        // EDICIÓN, REPORTES Y ELIMINACIÓN
        // ==========================================

        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");
            var emp = db.PEEEMP_EMPLE.Find(id);
            var usu = db.XEUSU_USUAR.FirstOrDefault(u => u.PEEEMP_CODIGO == id);
            var uxp = db.XEUXP_USUPE.FirstOrDefault(u => u.PEEEMP_CODIGO == id);
            if (emp == null) return HttpNotFound();

            var model = new UsuarioCompletoViewModel
            {
                PEEEMP_CODIGO = emp.PEEEMP_CODIGO,
                PEEEMP_CEDULA = emp.PEEEMP_CEDULA,
                PEEEMP_NOMBRES = emp.PEEEMP_NOMBRES,
                PEEEMP_APELLIDOS = emp.PEEEMP_APELLIDOS,
                PEEEMP_EMAIL = emp.PEEEMP_EMAIL,
                XEUSU_PASWD = usu?.XEUSU_PASWD,
                XEUSU_PIEFIR = usu?.XEUSU_PIEFIR,
                XEPER_CODIGO = uxp?.XEPER_CODIGO,
                PEPSEX_CODIGO = emp.PEPSEX_CODIGO,
                PEESC_CODIGO = emp.PEESC_CODIGO
            };
            CargarDesplegables(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UsuarioCompletoViewModel model, HttpPostedFileBase fotoFile)
        {
            ModelState.Remove("PEEEMP_DIRECCION");
            ModelState.Remove("PEEEMP_TELEFONO");

            if (ModelState.IsValid)
            {
                using (var trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        var emp = db.PEEEMP_EMPLE.Find(model.PEEEMP_CODIGO);
                        emp.PEEEMP_CEDULA = model.PEEEMP_CEDULA;
                        emp.PEEEMP_NOMBRES = model.PEEEMP_NOMBRES;
                        emp.PEEEMP_APELLIDOS = model.PEEEMP_APELLIDOS;
                        emp.PEEEMP_EMAIL = model.PEEEMP_EMAIL;

                        if (fotoFile != null && fotoFile.ContentLength > 0)
                        {
                            string fileName = model.PEEEMP_CODIGO + Path.GetExtension(fotoFile.FileName);
                            fotoFile.SaveAs(Path.Combine(Server.MapPath("~/Uploads/Fotos/"), fileName));
                            emp.PEEMP_FOTO = "/Uploads/Fotos/" + fileName;
                        }

                        db.Database.ExecuteSqlCommand("UPDATE XEUSU_USUAR SET XEUSU_PASWD = @p1, XEUSU_PIEFIR = @p2 WHERE PEEEMP_CODIGO = @p0", model.PEEEMP_CODIGO, model.XEUSU_PASWD, model.XEUSU_PIEFIR);
                        db.Database.ExecuteSqlCommand("UPDATE XEUXP_USUPE SET XEUSU_PASWD = @p1, XEPER_CODIGO = @p2 WHERE PEEEMP_CODIGO = @p0", model.PEEEMP_CODIGO, model.XEUSU_PASWD, model.XEPER_CODIGO);

                        db.Entry(emp).State = EntityState.Modified;
                        db.SaveChanges();
                        trans.Commit();
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        ModelState.AddModelError("", "Error: " + ex.Message);
                    }
                }
            }
            CargarDesplegables(model);
            return View(model);
        }

        public ActionResult VerReportePDF(string id)
        {
            var emp = db.PEEEMP_EMPLE.Find(id);
            var uxp = db.XEUXP_USUPE.Include(x => x.XEPER_PERFI).FirstOrDefault(u => u.PEEEMP_CODIGO == id);
            if (emp == null) return HttpNotFound();

            ViewBag.RutaFotoOriginal = !string.IsNullOrEmpty(emp.PEEMP_FOTO) ? emp.PEEMP_FOTO : "/Uploads/Fotos/default.png";
            var model = new UsuarioCompletoViewModel
            {
                PEEEMP_CODIGO = emp.PEEEMP_CODIGO,
                PEEEMP_CEDULA = emp.PEEEMP_CEDULA,
                PEEEMP_NOMBRES = emp.PEEEMP_NOMBRES,
                PEEEMP_APELLIDOS = emp.PEEEMP_APELLIDOS,
                PEEEMP_EMAIL = emp.PEEEMP_EMAIL,
                XEPER_CODIGO = uxp?.XEPER_PERFI?.XEPER_CODIGO
            };

            return new ViewAsPdf("DetalleUsuarioPDF", model)
            {
                FileName = "Ficha_" + id + ".pdf",
                PageSize = Rotativa.Options.Size.A4
            };
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                var usuario = db.XEUSU_USUAR.FirstOrDefault(u => u.PEEEMP_CODIGO == id);
                var empleado = db.PEEEMP_EMPLE.Find(id);
                if (usuario != null) db.XEUSU_USUAR.Remove(usuario);
                if (empleado != null) db.PEEEMP_EMPLE.Remove(empleado);
                db.SaveChanges();
                return Json(new { success = true, message = "Eliminado." });
            }
            catch { return Json(new { success = false, message = "Error al eliminar." }); }
        }


        [HttpPost]
        public JsonResult MU_RegistrarPerfil(string nombrePerfil)
        {
            try
            {
                if (string.IsNullOrEmpty(nombrePerfil))
                    return Json(new { success = false, mensaje = "El nombre es obligatorio." });

                string nombreUpper = nombrePerfil.Trim().ToUpper();

                // Validar si existe (comparamos strings)
                if (db.XEPER_PERFI.Any(p => p.XEPER_DESCRI.ToUpper() == nombreUpper))
                    return Json(new { success = false, mensaje = "Este perfil ya existe." });

                // Lógica optimizada para el siguiente código
                int nuevoId = 1;
                var codigosExistentes = db.XEPER_PERFI.Select(p => p.XEPER_CODIGO).ToList();

                if (codigosExistentes.Any())
                {
                    // Intentamos convertir a int para buscar el máximo, manejando posibles valores no numéricos
                    nuevoId = codigosExistentes
                                .Select(c => { int.TryParse(c, out int n); return n; })
                                .Max() + 1;
                }

                string nuevoCodigoString = nuevoId.ToString();

                if (nuevoCodigoString.Length > 5)
                    return Json(new { success = false, mensaje = "Se superó el límite de caracteres (5)." });

                // Insertar
                var nuevoPerfil = new XEPER_PERFI
                {
                    // FIX CS0029: Si tu modelo pide int, usa nuevoId. Si pide string, usa nuevoCodigoString.
                    // Según tu SQL (varchar), aquí debería ser string:
                    XEPER_CODIGO = nuevoCodigoString,
                    XEPER_DESCRI = nombreUpper
                };

                db.XEPER_PERFI.Add(nuevoPerfil);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult MU_GuardarMatrizAccesos(string CodigoPerfil, List<string> opcionesSeleccionadas)
        {
            try
            {
                // Validamos que el código no venga nulo para evitar errores al convertir
                if (string.IsNullOrEmpty(CodigoPerfil))
                    return Json(new { success = false, mensaje = "Código de perfil no válido." });

                // Convertimos el string a int una sola vez para usarlo en todo el método
                int perfilId = int.Parse(CodigoPerfil);

                // 1. Corregimos CS0019: Comparamos int con int
                var actuales = db.XEOXP_OPCPE.Where(x => x.XEPER_CODIGO == perfilId).ToList();
                db.XEOXP_OPCPE.RemoveRange(actuales);

                if (opcionesSeleccionadas != null)
                {
                    foreach (var codOpcion in opcionesSeleccionadas)
                    {
                        db.XEOXP_OPCPE.Add(new XEOXP_OPCPE
                        {
                            // 2. Corregimos CS0029: Asignamos int a una propiedad int
                            XEPER_CODIGO = perfilId,
                            XEOPC_CODIGO = codOpcion // Este sigue siendo string según tu SQL
                        });
                    }
                }

                db.SaveChanges();
                return Json(new { success = true, mensaje = "Accesos actualizados correctamente." });
            }
            catch (FormatException)
            {
                return Json(new { success = false, mensaje = "El código de perfil debe ser un número válido." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = "Error: " + ex.Message });
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        private void CargarDesplegables(UsuarioCompletoViewModel model = null)
        {
            ViewBag.PEPSEX_CODIGO = new SelectList(db.PESEX_SEXO, "PEPSEX_CODIGO", "PEPSEX_DESCRI", model?.PEPSEX_CODIGO);
            ViewBag.PEESC_CODIGO = new SelectList(db.PEESC_ESTCIV, "PEESC_CODIGO", "PEESC_DESCRI", model?.PEESC_CODIGO);
            ViewBag.XEPER_CODIGO = new SelectList(db.XEPER_PERFI, "XEPER_CODIGO", "XEPER_DESCRI", model?.XEPER_CODIGO);
        }
    }
}