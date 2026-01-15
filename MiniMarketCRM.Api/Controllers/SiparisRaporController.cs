using Microsoft.AspNetCore.Mvc;
using MiniMarketCRM.Application.Interfaces;

namespace MiniMarketCRM.Api.Controllers
{
    [ApiController]
    [Route("api/rapor/siparisler")]
    public class SiparisRaporController : ControllerBase
    {
        private readonly ISiparisRaporService _service;

        public SiparisRaporController(ISiparisRaporService service)
        {
            _service = service;
        }

        
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? musteriId)
        {
            var list = await _service.GetAsync(from, to, musteriId);
            return Ok(list);
        }

        // GET /api/rapor/siparisler/5
        [HttpGet("{siparisId:int}")]
        public async Task<IActionResult> GetById(int siparisId)
        {
            var item = await _service.GetByIdAsync(siparisId);
            return item == null ? NotFound() : Ok(item);
        }
    }
}
