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
                    }
                    else
                    {
                        _notyf.Error(dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception exception)
                {
                    _notyf.Error(exception.InnerException.Message);
                }
            }

            model.Categories = await _combosHelper.GetComboCategoriesAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            //validar que el Id esta presente
            if (id == null)
            {
                return NotFound();
            }

            //Validar que existe un producto con ese id
            Product product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            //Crear el objeto EditProductViewModel para la vista
            EditProductViewModel model = new()
            {
                Description = product.Description,
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
            };

            //Pasamos el objeto a la vista
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateProductViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            try
            {
                Product product = await _context.Products.FindAsync(model.Id);
                product.Description = model.Description;
                product.Name = model.Name;
                product.Price = model.Price;
                product.Stock = model.Stock;
                _context.Update(product);
                await _context.SaveChangesAsync();
                _notyf.Success($"El producto {product.Name} fue actualizado exitosamente.", 2);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                {
                    _notyf.Error("Ya existe un producto con el mismo nombre.");
                }
                else
                {
                    _notyf.Error(dbUpdateException.InnerException.Message);
                }
            }
            catch (Exception exception)
            {
                _notyf.Error(exception.Message);
            }

            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public async Task<IActionResult> AddImage(int? id)
        {
            //Validar que el Id está presente
            if (id == null)
            {
                return NotFound();
            }

            //Validar que el producto con ese Id existe
            Product product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            //Crear el modelo que espera la vista con la información que necesita
            AddProductImageViewModel model = new()
            {
                ProductId = product.Id,
            };

            //Pasar al modelo
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(AddProductImageViewModel model)
        {
            if (ModelState.IsValid)
            {
                Guid imageId = Guid.Empty;
                if (model.ImageFile != null)
                {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "products");
                }

                Product product = await _context.Products.FindAsync(model.ProductId);
                ProductImage productImage = new()
                {
                    Product = product,
                    ImageId = imageId,
                };

                try
                {
                    _context.Add(productImage);
                    await _context.SaveChangesAsync();
                    _notyf.Success("Imagen agregada exitosamente.", 2);
                    return RedirectToAction(nameof(Details), new { Id = product.Id });
                }
                catch (Exception exception)
                {
                    _notyf.Error(exception.Message);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> DeleteImage(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductImage productImage = await _context.ProductImages
                .Include(pi => pi.Product)
                .FirstOrDefaultAsync(pi => pi.Id == id);
            if (productImage == null)
            {
                return NotFound();
            }

            await _blobHelper.DeleteBlobAsync(productImage.ImageId, "products");
            _context.ProductImages.Remove(productImage);
            await _context.SaveChangesAsync();
            _notyf.Success("Imagen eliminada exitosamente.", 3);
            return RedirectToAction(nameof(Details), new { Id = productImage.Product.Id });
        }

        public async Task<IActionResult> AddCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            AddCategoryProductViewModel model = new()
            {
                ProductId = product.Id,
                Categories = await _combosHelper.GetComboCategoriesAsync(),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(AddCategoryProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                Product product = await _context.Products.FindAsync(model.ProductId);
                ProductCategory productCategory = new()
                {
                    Category = await _context.Categories.FindAsync(model.CategoryId),
                    Product = product,
                };

                try
                {
                    _context.Add(productCategory);
                    await _context.SaveChangesAsync();
                    _notyf.Success("Categoría agregada exitosamente.", 3);
                    return RedirectToAction(nameof(Details), new { Id = product.Id });
                }
                catch (Exception exception)
                {
                    if (exception.InnerException.Message.Contains("The duplicate key value"))
                    {
                        _notyf.Error($"Ya existe la categoría {productCategory.Category.Name} en el producto {product.Name}");
                    }
                    else
                    {
                        _notyf.Error(exception.InnerException.Message);
                    }
                }
            }

            model.Categories = await _combosHelper.GetComboCategoriesAsync();
            return View(model);
        }

        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductCategory productCategory = await _context.ProductCategories
                .Include(pc => pc.Product)
                .FirstOrDefaultAsync(pc => pc.Id == id);
            if (productCategory == null)
            {
                return NotFound();
            }

            _context.ProductCategories.Remove(productCategory);
            await _context.SaveChangesAsync();
            _notyf.Success("Categoría eliminada exitosamente.");
            return RedirectToAction(nameof(Details), new { Id = productCategory.Product.Id });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product product = await _context.Products
                .Include(p => p.ProductCategories)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Product model)
        {
            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            foreach (ProductImage productImage in product.ProductImages)
            {
                await _blobHelper.DeleteBlobAsync(productImage.ImageId, "products");
            }

            _notyf.Success("Producto eliminado exitosamente");
            return RedirectToAction(nameof(Index));
        }





    }
}
