using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniMarketCRM.Application.DTO;
using System.Net.Http.Json;

namespace MiniMarketCRM.Web.Controllers
{
    public class UrunlerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UrunlerController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // LIST (müşteri seçmeden de girilebilir)
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var urunler = await client.GetFromJsonAsync<List<UrunDTO>>("api/urunler")
                         ?? new List<UrunDTO>();

            return View(urunler);
        }

        // CREATE (GET)
        public async Task<IActionResult> Create()
        {
            await LoadKategorilerToViewBag();
            return View(new UrunUpsertDTO());
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UrunUpsertDTO dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadKategorilerToViewBag();
                return View(dto);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var res = await client.PostAsJsonAsync("api/urunler", dto);

            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = await res.Content.ReadAsStringAsync();
                await LoadKategorilerToViewBag();
                return View(dto);
            }

            TempData["Success"] = "Ürün eklendi ✅";
            return RedirectToAction(nameof(Index));
        }

        // EDIT (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var urun = await client.GetFromJsonAsync<UrunDTO>($"api/urunler/{id}");
            if (urun == null)
            {
                TempData["Error"] = "Ürün bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var dto = new UrunUpsertDTO
            {
                Ad = urun.Ad,
                KategoriId = urun.KategoriId,
                Fiyat = urun.Fiyat,
                Stok = urun.Stok,
                AktifMi = urun.AktifMi
            };

            ViewBag.UrunId = urun.UrunId;
            await LoadKategorilerToViewBag();
            return View(dto);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UrunUpsertDTO dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UrunId = id;
                await LoadKategorilerToViewBag();
                return View(dto);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var res = await client.PutAsJsonAsync($"api/urunler/{id}", dto);

            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = await res.Content.ReadAsStringAsync();
                ViewBag.UrunId = id;
                await LoadKategorilerToViewBag();
                return View(dto);
            }

            TempData["Success"] = "Ürün güncellendi ✅";
            return RedirectToAction(nameof(Index));
        }

        // DELETE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var res = await client.DeleteAsync($"api/urunler/{id}");

            if (!res.IsSuccessStatusCode)
                TempData["Error"] = await res.Content.ReadAsStringAsync();
            else
                TempData["Success"] = "Ürün silindi ✅";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadKategorilerToViewBag()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var kategoriler = await client.GetFromJsonAsync<List<KategoriDTO>>("api/kategoriler")
                            ?? new List<KategoriDTO>();

            ViewBag.Kategoriler = kategoriler
                .OrderBy(x => x.KategoriAdi)
                .Select(x => new SelectListItem
                {
                    Value = x.KategoriId.ToString(),
                    Text = x.KategoriAdi
                })
                .ToList();
        }
    }
}
