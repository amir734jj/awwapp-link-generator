using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IAwwAppLogic = App.Interfaces.IAwwAppLogic;

namespace App.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly IAwwAppLogic _appLogic;

        public HomeController(IAwwAppLogic appLogic)
        {
            _appLogic = appLogic;
        }
        
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("link/{count}")]
        public async Task<IActionResult> GenerateLinks([FromRoute] int count)
        {
            return Ok(await _appLogic.GenerateLinks(count));
        }
    }
}