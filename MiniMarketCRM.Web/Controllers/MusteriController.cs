using Microsoft.AspNetCore.Mvc;
using MiniMarketCRM.Application.DTO;
using System.Net.Http.Json;

namespace MiniMarketCRM.Web.Controllers
{
    public class MusterilerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MusterilerController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // LISTE
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            try
            {
                var res = await client.GetAsync("api/musteriler");

                if (!res.IsSuccessStatusCode)
                {
                    TempData["Error"] = await res.Content.ReadAsStringAsync();
                    return View(new List<MusteriDTO>());
                }

                var list = await res.Content.ReadFromJsonAsync<List<MusteriDTO>>()
                           ?? new List<MusteriDTO>();

                return View(list);
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = $"API bağlantı hatası: {ex.Message}";
                return View(new List<MusteriDTO>());
            }
        }


        public async Task<IActionResult> Select()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var list = await client.GetFromJsonAsync<List<MusteriDTO>>("api/musteriler")
                       ?? new List<MusteriDTO>();

            return View(list);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Select(int musteriId)
        {
            HttpContext.Session.SetInt32("SelectedMusteriId", musteriId);
            TempData["Success"] = $"Müşteri seçildi ✅ (ID: {musteriId})";
            return RedirectToAction("Index", "Urunler", new { musteriId });
        }

        // CREATE (GET)
        public IActionResult Create()
        {
            return View(new MusteriUpsertDTO());
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MusteriUpsertDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var res = await client.PostAsJsonAsync("api/musteriler", dto);

            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = await res.Content.ReadAsStringAsync();
                return View(dto);
            }

            TempData["Success"] = "Müşteri eklendi ✅";
            return RedirectToAction(nameof(Index));
        }

        // EDIT (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var res = await client.GetAsync($"api/musteriler/{id}");
            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = await res.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(Index));
            }

            var musteri = await res.Content.ReadFromJsonAsync<MusteriDTO>();
            if (musteri == null)
            {
                TempData["Error"] = "Müşteri bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var dto = new MusteriUpsertDTO
            {
                Ad = musteri.Ad,
                Soyad = musteri.Soyad,
                Email = musteri.Email
            };

            ViewBag.MusteriId = musteri.MusteriId;
            return View(dto);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MusteriUpsertDTO dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MusteriId = id;
                return View(dto);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var res = await client.PutAsJsonAsync($"api/musteriler/{id}", dto);

            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = await res.Content.ReadAsStringAsync();
                ViewBag.MusteriId = id;
                return View(dto);
            }

            TempData["Success"] = "Müşteri güncellendi ✅";
            return RedirectToAction(nameof(Index));
        }

        // DELETE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var res = await client.DeleteAsync($"api/musteriler/{id}");

            if (!res.IsSuccessStatusCode)
                TempData["Error"] = await res.Content.ReadAsStringAsync();
            else
                TempData["Success"] = "Müşteri silindi ✅";

            return RedirectToAction(nameof(Index));
        }
    }
}
