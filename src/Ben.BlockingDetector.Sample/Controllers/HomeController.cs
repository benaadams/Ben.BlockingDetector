using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ben.BlockingDetector.Sample.Models;
using Microsoft.AspNetCore.Http;

namespace Ben.BlockingDetector.Sample.Controllers
{
    public class HomeController : Controller
    {
        readonly IHttpContextAccessor _contextAccessor;

        public HomeController(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            await BlockingWrite(_contextAccessor.HttpContext);
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public Task BlockingWrite(HttpContext httpContext)
        {
            var response = httpContext.Response;
            response.StatusCode = 200;
            response.ContentType = "text/plain";

            var s = new string('n', 160000);
            response.ContentLength = s.Length * 3;
            response.Body.Write(Encoding.ASCII.GetBytes(s), 0, s.Length);
            response.Body.Write(Encoding.ASCII.GetBytes(s), 0, s.Length);
            response.Body.Write(Encoding.ASCII.GetBytes(s), 0, s.Length);

            return Task.CompletedTask;
        }
    }
}
