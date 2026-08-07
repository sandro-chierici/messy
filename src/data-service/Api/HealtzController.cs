using DataService.Business.IO;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Api
{
    [Route("api/healtz")]
    [ApiController]
    public class HealtzController(IDbConnectionFactory dbConnectionFactory) : Controller
    {
        private readonly IDbConnectionFactory dbConnectionFactory = dbConnectionFactory;

        [Route("readiness")]
        public async Task<IActionResult> Readiness()
        {
            try
            {
                using var conn = await dbConnectionFactory.CreateConnectionAsync();
                return Ok();
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("liveness")]
        public IActionResult Liveness() => Ok();
    }
}
