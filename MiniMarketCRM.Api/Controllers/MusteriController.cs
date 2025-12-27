using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Application.Interfaces;

namespace MiniMarketCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MusterilerController : ControllerBase
    {
        private readonly IMusteriService _service;

        public MusterilerController(IMusteriService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<MusteriDTO>>> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MusteriDTO>> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MusteriUpsertDTO dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.MusteriId }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message); // email unique
            }
            catch (DbUpdateException)
            {
                return Conflict("Bu email ile kayıtlı müşteri zaten var."); // DB unique index patlarsa
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] MusteriUpsertDTO dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                return updated == null ? NotFound() : Ok(updated); 
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (DbUpdateException)
            {
                return Conflict("Bu email ile kayıtlı müşteri zaten var.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
