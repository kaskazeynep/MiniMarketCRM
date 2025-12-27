using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Application.Interfaces;

namespace MiniMarketCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KategorilerController : ControllerBase
    {
        private readonly IKategoriService _service;

        public KategorilerController(IKategoriService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<KategoriDTO>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<KategoriDTO>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result is null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] KategoriUpsertDTO dto)
        {
            var newId = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newId }, new { id = newId });
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] KategoriUpsertDTO dto)
        {
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var ok = await _service.DeleteAsync(id);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                // Kategoriye bağlı ürün varsa Restrict yüzünden silinmez
                return Conflict("Bu kategoriye bağlı ürünler var. Önce ürünleri taşı/sil, sonra kategoriyi sil.");
            }
        }
    }
}
