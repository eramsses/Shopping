using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Helpers;
using Shopping.Models;
using System.Diagnostics;

namespace Shopping.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly INotyfService _notyf;

        public HomeController(ILogger<HomeController> logger, DataContext context, IUserHelper userHelper, INotyfService notyf)
        {
            _logger = logger;
            _context = context;
            _userHelper = userHelper;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index()
        {
            List<Product>? products = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .OrderBy(p => p.Description)
                .ToListAsync();

            HomeViewModel model = new() { Products = products };
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            model.Quantity = 0;

            if (user != null)
            {
                model.Quantity = await _context.TemporalSales
                    .Where(ts => ts.User == user)
                    .SumAsync(ts => ts.Quantity);
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("error/404")]
        public IActionResult Error404()
        {
            return View();
        }

        public async Task<IActionResult> Add(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!User.Identity.IsAuthenticated)
            {
                _notyf.Information("Debe iniciar sesión antes de agregar un producto", 10);
                return RedirectToAction("Login", "Account");
            }

            Product product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _notyf.Error("Ocurrió un error con el producto seleccionado.");
                return NotFound();
            }

            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null)
            {
                _notyf.Error("Ocurrió un error con el usuario, por favor vuelva a iniciar sesión.");
                return NotFound();
            }

            TemporalSale temporalSale = new()
            {
                Product = product,
                Quantity = 1,
                User = user
            };

            _context.TemporalSales.Add(temporalSale);
            await _context.SaveChangesAsync();
            _notyf.Success($"El producto {product.Name} fue agregado al carrito de compras exitosamente.", 8);
            return RedirectToAction(nameof(Index));
        }


    }
}