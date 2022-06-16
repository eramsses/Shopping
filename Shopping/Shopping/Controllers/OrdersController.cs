using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Enums;
using Shopping.Helpers;

namespace Shopping.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly DataContext _context;
        private readonly INotyfService _notyf;
        private readonly IOrdersHelper _ordersHelper;

        public OrdersController(DataContext context, INotyfService notyf, IOrdersHelper ordersHelper)
        {
            _context = context;
            _notyf = notyf;
            _ordersHelper = ordersHelper;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sales
                .Include(s => s.User)
                .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Sale sale = await _context.Sales
                .Include(s => s.User)
                .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
                .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        public async Task<IActionResult> Dispatch(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Sale sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }

            if (sale.OrderStatus != OrderStatus.Nuevo)
            {
                _notyf.Error("Solo se pueden despachar pedidos que estén en estado \"nuevo\".");
            }
            else
            {
                sale.OrderStatus = OrderStatus.Despachado;
                _context.Sales.Update(sale);
                await _context.SaveChangesAsync();
                _notyf.Success("El estado del pedido ha sido cambiado a \"Despachado\".");
            }

            return RedirectToAction(nameof(Details), new { Id = sale.Id });
        }

        public async Task<IActionResult> Send(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Sale sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }

            if (sale.OrderStatus != OrderStatus.Despachado)
            {
                _notyf.Error("Solo se pueden enviar pedidos que estén en estado \"Despachado\".");
            }
            else
            {
                sale.OrderStatus = OrderStatus.Enviado;
                _context.Sales.Update(sale);
                await _context.SaveChangesAsync();
                _notyf.Success("El estado del pedido ha sido cambiado a \"Enviado\".");
            }

            return RedirectToAction(nameof(Details), new { Id = sale.Id });
        }

        public async Task<IActionResult> Confirm(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Sale sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }

            if (sale.OrderStatus != OrderStatus.Enviado)
            {
                _notyf.Error("Solo se pueden confirmar pedidos que estén en estado \"Enviado\".");
            }
            else
            {
                sale.OrderStatus = OrderStatus.Confirmado;
                _context.Sales.Update(sale);
                await _context.SaveChangesAsync();
                _notyf.Success("El estado del pedido ha sido cambiado a \"Confirmado\".");
            }

            return RedirectToAction(nameof(Details), new { Id = sale.Id });
        }

        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Sale sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }

            if (sale.OrderStatus == OrderStatus.Cancelado)
            {
                _notyf.Error("No se puede cancelar un pedido que esté en estado \"Cancelado\".");
            }
            else
            {
                await _ordersHelper.CancelOrderAsync(sale.Id);
                _notyf.Success("El estado del pedido ha sido cambiado a \"Cancelado\".", 4);
            }

            return RedirectToAction(nameof(Details), new { Id = sale.Id });
        }



    }
}
