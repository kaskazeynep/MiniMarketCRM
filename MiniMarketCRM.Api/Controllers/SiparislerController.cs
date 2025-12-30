using Microsoft.AspNetCore.Mvc;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Application.Interfaces;

namespace MiniMarketCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiparislerController : ControllerBase
    {
        private readonly ISiparisService _service;

        public SiparislerController(ISiparisService service)
        {
            _service = service;
        }

        // GET: /api/siparisler
        [HttpGet]
        public async Task<ActionResult<List<SiparisDTO>>> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        // GET: /api/siparisler/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SiparisDTO>> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        // POST: /api/siparisler
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SiparisUpsertDTO dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.SiparisId }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: /api/siparisler/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] SiparisUpsertDTO dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                return updated is null ? NotFound() : Ok(updated); 
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: /api/siparisler/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var ok = await _service.DeleteAsync(id);
                return ok ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
        }
    }
}
