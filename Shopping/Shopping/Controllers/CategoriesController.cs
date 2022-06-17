using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Helpers;
using static Shopping.Helpers.ModalHelper;

namespace Shopping.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly DataContext _context;
        private readonly INotyfService _notyf;

        public CategoriesController(DataContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories
                .Include(c => c.ProductCategories)
                .ToListAsync());
        }

        [NoDirectAccess]
        public async Task<IActionResult> Delete(int? id)
        {
            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                _notyf.Success("Registro eliminado exitosamente.", 3);
            }
            catch
            {
                _notyf.Error("No se puede borrar la categoría porque tiene registros relacionados.");
            }

            return RedirectToAction(nameof(Index));
        }


        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return View(new Category());
            }
            else
            {
                Category category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                return View(category);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (id == 0) //Insert
                    {
                        _context.Add(category);
                        await _context.SaveChangesAsync();
                        _notyf.Success("Registro creado exitosamente", 3);
                    }
                    else //Update
                    {
                        _context.Update(category);
                        await _context.SaveChangesAsync();
                        _notyf.Success("Registro actualizado exitosamente", 3);
                    }
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _notyf.Error("Ya existe una categoría con el mismo nombre.");
                    }
                    else
                    {
                        _notyf.Error(dbUpdateException.InnerException.Message);
                    }
                    return View(category);
                }
                catch (Exception exception)
                {
                    _notyf.Error(exception.Message);
                    return View(category);
                }

                //Este retorno es necesario cuando viene desde una modal.
                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "Index", _context.Categories.Include(c => c.ProductCategories).ToList()) });

            }
            
            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEdit", category) });
        }


    }
}
