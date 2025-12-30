using Microsoft.AspNetCore.Mvc;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Application.Interfaces;

namespace MiniMarketCRM.Api.Controllers
{
    [ApiController]
    [Route("api/siparisler/{siparisId:int}/kalemler")]
    public class SiparisKalemiController : ControllerBase
    {
        private readonly ISiparisKalemiService _service;

        public SiparisKalemiController(ISiparisKalemiService service)
        {
            _service = service;
        }

        // GET: /api/siparisler/{siparisId}/kalemler
        [HttpGet]
        public async Task<ActionResult<List<SiparisKalemiDTO>>> GetBySiparisId(int siparisId)
        {
            var list = await _service.GetBySiparisIdAsync(siparisId);
            return Ok(list);
        }

        // POST: /api/siparisler/{siparisId}/kalemler
        // aynı ürün eklenirse: adet artırır
        [HttpPost]
        public async Task<IActionResult> Add(int siparisId, [FromBody] SiparisKalemiUpsertDTO dto)
        {
            try
            {
               
                dto.SiparisId = siparisId;
                var createdOrUpdated = await _service.AddAsync(siparisId, dto);
                return Ok(createdOrUpdated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Sipariş bulunamadı.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: /api/siparisler/{siparisId}/kalemler/{kalemId}
        [HttpPut("{kalemId:int}")]
        public async Task<IActionResult> Update(int siparisId, int kalemId, [FromBody] SiparisKalemiUpsertDTO dto)
        {
            try
            {
                dto.SiparisId = siparisId;

                var updated = await _service.UpdateAsync(siparisId, kalemId, dto);
                return updated is null ? NotFound() : Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: /api/siparisler/{siparisId}/kalemler/{kalemId}
        [HttpDelete("{kalemId:int}")]
        public async Task<IActionResult> Delete(int siparisId, int kalemId)
        {
            try
            {
                var ok = await _service.DeleteAsync(siparisId, kalemId);
                return ok ? NoContent() : NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
