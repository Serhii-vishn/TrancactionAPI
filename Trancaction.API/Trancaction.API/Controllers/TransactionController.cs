using Microsoft.AspNetCore.Mvc;

namespace Transaction.API.Controllers
{
    public class TransactionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
