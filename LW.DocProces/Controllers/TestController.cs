using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LW.DocProces.Controllers
{
	[ApiController]
	[Route("[controller]")]
	[Authorize]
	public class TestController : ControllerBase
	{
		[HttpGet]
		public IActionResult Index()
		{
			return Ok();
		}
	}
}
