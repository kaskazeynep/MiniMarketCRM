using Microsoft.AspNetCore.Mvc;
using MiniMarketCRM.Application.DTO;
using System.Net.Http.Json;

namespace MiniMarketCRM.Web.Controllers
{
    public class RaporlarController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RaporlarController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

    
        public async Task<IActionResult> Satis(DateTime? from, DateTime? to, int? musteriId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var qs = new List<string>();
            if (from.HasValue) qs.Add($"from={Uri.EscapeDataString(from.Value.ToString("yyyy-MM-dd"))}");
            if (to.HasValue) qs.Add($"to={Uri.EscapeDataString(to.Value.ToString("yyyy-MM-dd"))}");
            if (musteriId.HasValue) qs.Add($"musteriId={musteriId.Value}");

            var url = "api/rapor/siparisler" + (qs.Any() ? "?" + string.Join("&", qs) : "");

            var list = await client.GetFromJsonAsync<List<SiparisRaporDTO>>(url) ?? new List<SiparisRaporDTO>();

            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");
            ViewBag.MusteriId = musteriId;

            return View(list);
        }

        public async Task<IActionResult> SatisDetay(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var item = await client.GetFromJsonAsync<SiparisRaporDTO>($"api/rapor/siparisler/{id}");

            if (item == null)
            {
                TempData["Error"] = "Sipariş bulunamadı.";
                return RedirectToAction(nameof(Satis));
            }

            return View(item);
        }
    }
}
