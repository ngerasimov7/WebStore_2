using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStore.Interfaces;
using WebStore.Domain.ViewModels;

namespace WebStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartServices _CartServices;

        public CartController(ICartServices CartServices) => _CartServices = CartServices;

        public IActionResult Index() => View(new CartOrderViewModel { Cart = _CartServices.GetViewModel() });

        public IActionResult Add(int id)
        {
            _CartServices.Add(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int id)
        {
            _CartServices.Remove(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Decrement(int id)
        {
            _CartServices.Decrement(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Clear()
        {
            _CartServices.Clear();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> CheckOut(OrderViewModel OrderModel, [FromServices] IOrderService OrderService)
        {
            if (!ModelState.IsValid)
                return View(nameof(Index), new CartOrderViewModel
                {
                    Cart = _CartServices.GetViewModel(),
                    Order = OrderModel
                });

            var order = await OrderService.CreateOrder(
                User.Identity!.Name,
                _CartServices.GetViewModel(),
                OrderModel
                );

            _CartServices.Clear();

            return RedirectToAction(nameof(OrderConfirmed), new { order.Id });
        }

        public IActionResult OrderConfirmed(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }
    }
}
