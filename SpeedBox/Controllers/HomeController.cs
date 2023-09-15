using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpeedBox.Interfases;
using SpeedBox.Models;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;

namespace SpeedBox.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDelivery _delivery;

        public HomeController(ILogger<HomeController> logger, IDelivery delivery)
        {
            _logger = logger;
            _delivery = delivery;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        } 
        public async Task<IActionResult> CalculateShippingCost(int weight, int length, int width, int height, string cityFrom, string cityTo)
        {
            ViewBag.Weight = weight;
            ViewBag.Length = length;
            ViewBag.Width = width;
            ViewBag.Height = height;
            ViewBag.CityFrom = cityFrom;
            ViewBag.CityTo = cityTo;
            ViewBag.CostDelivrey = await _delivery.GetShippingCost(weight, length, width, height, cityFrom, cityTo);
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}