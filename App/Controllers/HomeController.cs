using System.Linq;
using System.Threading.Tasks;
using Logic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using IAwwAppLogic = App.Logic.Interfaces.IAwwAppLogic;

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
        
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("{count}")]
        public async Task<IActionResult> GenerateLinks([FromRoute] int count)
        {
            return Ok(await _appLogic.GenerateLinks(count).ToHashSetAsync());
        }
    }
}