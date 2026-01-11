using Microsoft.AspNetCore.Mvc;
using MiniMarketCRM.Application.DTO;
using MiniMarketCRM.Web.Infrastructure;
using System.Net.Http.Json;

namespace MiniMarketCRM.Web.Controllers
{
    public class SepetController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SepetController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private int? GetSelectedMusteriId() => HttpContext.Session.GetInt32(SessionKeys.SelectedMusteriId);

        private void ClearSelectedMusteri()
        {
            HttpContext.Session.Remove(SessionKeys.SelectedMusteriId);
            HttpContext.Session.Remove(SessionKeys.SelectedMusteriName);
        }

        // GET: /Sepet
        public async Task<IActionResult> Index()
        {
            var musteriId = GetSelectedMusteriId();
            if (musteriId == null)
            {
                TempData["Error"] = "Sepeti görmek için önce müşteri seçmelisin.";
                return RedirectToAction("Select", "Musteriler");
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var cart = await client.GetFromJsonAsync<CartDTO>($"api/cart/{musteriId.Value}");

            return View(cart); 
        }

        // POST: /Sepet/Ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(int urunId, int adet = 1)
        {
            var musteriId = GetSelectedMusteriId();
            if (musteriId == null)
            {
                TempData["Error"] = "Sepete eklemek için önce müşteri seçmelisin.";
                return RedirectToAction("Select", "Musteriler");
            }

            if (urunId <= 0 || adet <= 0)
            {
                TempData["Error"] = "Geçersiz ürün/adet.";
                return RedirectToAction("Index", "Urunler");
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            var response = await client.PostAsJsonAsync(
                $"api/cart/{musteriId.Value}/items",
                new CartItemAddDTO { UrunId = urunId, Adet = adet }
            );

            if (!response.IsSuccessStatusCode)
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            else
                TempData["Success"] = "Sepete eklendi ✅";

            return RedirectToAction("Index", "Urunler");
        }

        // POST: /Sepet/Guncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guncelle(int kalemId, int adet)
        {
            var musteriId = GetSelectedMusteriId();
            if (musteriId == null) return RedirectToAction("Select", "Musteriler");

            if (kalemId <= 0 || adet <= 0)
            {
                TempData["Error"] = "Geçersiz kalem/adet.";
                return RedirectToAction(nameof(Index));
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            var response = await client.PutAsJsonAsync(
                $"api/cart/{musteriId.Value}/items/{kalemId}",
                new CartItemUpdateDTO { Adet = adet }
            );

            if (!response.IsSuccessStatusCode)
                TempData["Error"] = await response.Content.ReadAsStringAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Sepet/Sil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int kalemId)
        {
            var musteriId = GetSelectedMusteriId();
            if (musteriId == null) return RedirectToAction("Select", "Musteriler");

            if (kalemId <= 0)
            {
                TempData["Error"] = "Geçersiz kalem.";
                return RedirectToAction(nameof(Index));
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            var response = await client.DeleteAsync($"api/cart/{musteriId.Value}/items/{kalemId}");

            if (!response.IsSuccessStatusCode)
                TempData["Error"] = await response.Content.ReadAsStringAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Sepet/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var musteriId = GetSelectedMusteriId();
            if (musteriId == null)
            {
                TempData["Error"] = "Önce müşteri seçmelisin.";
                return RedirectToAction("Select", "Musteriler");
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsync($"api/cart/{musteriId.Value}/checkout", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Checkout tamamlandı ✅";
                ClearSelectedMusteri(); // müşteri sıfırla
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = await response.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: /Sepet/Iptal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Iptal()
        {
            var musteriId = GetSelectedMusteriId();
            if (musteriId == null)
            {
                TempData["Error"] = "Önce müşteri seçmelisin.";
                return RedirectToAction("Select", "Musteriler");
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            // API: POST /api/cart/{musteriId}/cancel
            var response = await client.PostAsync($"api/cart/{musteriId.Value}/cancel", content: null);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Sipariş iptal edildi ✅";
                ClearSelectedMusteri(); // seçili müşteriyi sıfırla
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = await response.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
