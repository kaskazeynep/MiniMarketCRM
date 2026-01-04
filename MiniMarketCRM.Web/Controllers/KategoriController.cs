using Microsoft.AspNetCore.Mvc;
using MiniMarketCRM.Application.DTO;
using System.Net.Http.Json;

namespace MiniMarketCRM.Web.Controllers
{
    public class KategorilerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public KategorilerController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // LISTE
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            try
            {
                var res = await client.GetAsync("api/kategoriler");

                if (!res.IsSuccessStatusCode)
                {
                    TempData["Error"] = await res.Content.ReadAsStringAsync();
                    return View(new List<KategoriDTO>());
                }

                var list = await res.Content.ReadFromJsonAsync<List<KategoriDTO>>()
                           ?? new List<KategoriDTO>();

                return View(list);
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = $"API bağlantı hatası: {ex.Message}";
                return View(new List<KategoriDTO>());
            }
        }

        // CREATE (GET)
        public IActionResult Create()
        {
            return View(new KategoriUpsertDTO());
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KategoriUpsertDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var res = await client.PostAsJsonAsync("api/kategoriler", dto);

            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = await res.Content.ReadAsStringAsync();
                return View(dto);
            }

            TempData["Success"] = "Kategori eklendi ✅";
            return RedirectToAction(nameof(Index));
        }

        // EDIT (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var res = await client.GetAsync($"api/kategoriler/{id}");
            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = await res.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(Index));
            }

            var kategori = await res.Content.ReadFromJsonAsync<KategoriDTO>();
            if (kategori == null)
            {
                TempData["Error"] = "Kategori bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var dto = new KategoriUpsertDTO
            {
                KategoriAdi = kategori.KategoriAdi,
                Aciklama = kategori.Aciklama
            };

            ViewBag.KategoriId = kategori.KategoriId;
            return View(dto);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KategoriUpsertDTO dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.KategoriId = id;
                return View(dto);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var res = await client.PutAsJsonAsync($"api/kategoriler/{id}", dto);

            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = await res.Content.ReadAsStringAsync();
                ViewBag.KategoriId = id;
                return View(dto);
            }

            TempData["Success"] = "Kategori güncellendi ✅";
            return RedirectToAction(nameof(Index));
        }

        // DELETE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var res = await client.DeleteAsync($"api/kategoriler/{id}");

            if (!res.IsSuccessStatusCode)
                TempData["Error"] = await res.Content.ReadAsStringAsync();
            else
                TempData["Success"] = "Kategori silindi ✅";

            return RedirectToAction(nameof(Index));
        }
    }
}
