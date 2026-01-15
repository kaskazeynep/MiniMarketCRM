using Microsoft.AspNetCore.Mvc;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Application.Interfaces;

namespace MiniMarketCRM.Api.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;

        public CartController(ICartService service)
        {
            _service = service;
        }

        // GET /api/cart/{musteriId}
        // sepet yoksa boş CartDTO döner (DB’ye yazmaz)
        [HttpGet("{musteriId:int}")]
        public async Task<IActionResult> Get(int musteriId)
        {
            try
            {
                var cart = await _service.GetOrCreateAsync(musteriId);
                return Ok(cart);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST /api/cart/{musteriId}/items
        [HttpPost("{musteriId:int}/items")]
        public async Task<IActionResult> AddItem(int musteriId, [FromBody] CartItemAddDTO dto)
        {
            try
            {
                var cart = await _service.AddItemAsync(musteriId, dto);
                return Ok(cart);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // PUT /api/cart/{musteriId}/items/{kalemId}
        [HttpPut("{musteriId:int}/items/{kalemId:int}")]
        public async Task<IActionResult> UpdateItem(int musteriId, int kalemId, [FromBody] CartItemUpdateDTO dto)
        {
            try
            {
                var cart = await _service.UpdateItemAsync(musteriId, kalemId, dto);
                return Ok(cart);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // DELETE /api/cart/{musteriId}/items/{kalemId}
        [HttpDelete("{musteriId:int}/items/{kalemId:int}")]
        public async Task<IActionResult> RemoveItem(int musteriId, int kalemId)
        {
            try
            {
                var cart = await _service.RemoveItemAsync(musteriId, kalemId);
                return Ok(cart);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST /api/cart/{musteriId}/checkout
        [HttpPost("{musteriId:int}/checkout")]
        public async Task<IActionResult> Checkout(int musteriId)
        {
            try
            {
                var order = await _service.CheckoutAsync(musteriId);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST /api/cart/{musteriId}/cancel
        // sipariş DB’de kalsın ama Durum=Iptal ve stok iade edilsin
        [HttpPost("{musteriId:int}/cancel")]
        public async Task<IActionResult> Cancel(int musteriId)
        {
            try
            {
                var order = await _service.CancelAsync(musteriId);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
