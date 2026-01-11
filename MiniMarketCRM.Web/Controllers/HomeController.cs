using Microsoft.AspNetCore.Mvc;
using MiniMarketCRM.Application.DTO;
using System.Net.Http.Json;

namespace MiniMarketCRM.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string SessionKey = "SelectedMusteriId";

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var selectedMusteriId = HttpContext.Session.GetInt32(SessionKey);

            var client = _httpClientFactory.CreateClient("ApiClient");
            DashboardSummaryDTO summary;

            try
            {
               
                summary = await client.GetFromJsonAsync<DashboardSummaryDTO>(
                    $"api/dashboard/summary?musteriId={selectedMusteriId}&stokEsik=5"
                ) ?? new DashboardSummaryDTO();
            }
            catch
            {
                summary = new DashboardSummaryDTO();
            }

            summary.SelectedMusteriId = selectedMusteriId;

            return View(summary);
        }
    }
}
