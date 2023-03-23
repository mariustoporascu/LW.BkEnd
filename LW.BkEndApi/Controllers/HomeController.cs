using Microsoft.AspNetCore.Mvc;

namespace LW.BkEndApi.Controllers
{
	public class HomeController : ControllerBase
	{
		public IActionResult Index()
		{
			return Ok();
		}
	}
}
