using Microsoft.AspNetCore.Mvc;

namespace LW.DocProces.Controllers
{
	public class HomeController : ControllerBase
	{
		public IActionResult Index()
		{
			return Ok();
		}
	}
}
