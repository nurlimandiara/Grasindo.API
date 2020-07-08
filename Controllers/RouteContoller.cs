using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Grasindo.API.Controllers
{
    [ApiController]
    [Route("/")]
    public class RouteController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            var g = Guid.NewGuid();
            var gString = Convert.ToBase64String(g.ToByteArray());
            gString = gString.TrimEnd('+','=');
            return Ok(gString);
        }
    }
}