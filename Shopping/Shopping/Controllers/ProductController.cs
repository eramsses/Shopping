using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Helpers;
using Shopping.Models;

namespace Shopping.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly INotyfService _notyf;

        public ProductController(DataContext context, ICombosHelper combosHelper, IBlobHelper blobHelper, INotyfService notyf)
        {
            _context = context;
            _combosHelper = combosHelper;
            _blobHelper = blobHelper;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            CreateProductViewModel model = new()
            {
                Categories = await _combosHelper.GetComboCategoriesAsync(),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Asumir que no hay imagen
                Guid imageId = Guid.Empty;
                if (model.ImageFile != null)
                {
                    //En caso que sí hay imagen subirla a productos y asignar en Guid al imageId
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "products");
                }

                //Crear el objeto Product
                Product product = new()
                {
                    Description = model.Description,
                    Name = model.Name,
                    Price = model.Price,
                    Stock = model.Stock,
                };

                //Recuperar el objeto categories porque no se puede enviar solo el id de al categoría
                //Ya que es una relación a muchas categorías
                product.ProductCategories = new List<ProductCategory>()
                {
                    new ProductCategory
                    {
                        Category = await _context.Categories.FindAsync(model.CategoryId)
                    }
                };

                //Igual que la categoría las imagenes pueden ser muchas y si hay imagen seleccionada
                //entonces se envia la lista de imagenes.
                if (imageId != Guid.Empty)
                {
                    product.ProductImages = new List<ProductImage>()
                    {
                        new ProductImage { ImageId = imageId }
                    };
                }

                try
                {
                    _context.Add(product);
                    await _context.SaveChangesAsync();

                    _notyf.Success($"Producto {model.Name} fue agregado exitosamente.", 2);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _notyf.Error("Ya existe un producto con el mismo nombre.");
                        ModelState.AddModelError(string.Empty, "Ya existe un producto con el mismo nombre.");
                    }
                    else
                    {
                        _notyf.Error(dbUpdateException.InnerException.Message);
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            model.Categories = await _combosHelper.GetComboCategoriesAsync();
            return View(model);
        }


    }
}
