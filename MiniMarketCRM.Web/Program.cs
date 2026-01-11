namespace MiniMarketCRM.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC
            builder.Services.AddControllersWithViews();

            // HttpContextAccessor (Layout'ta Session/Request Path okumak için)
            builder.Services.AddHttpContextAccessor();

            // API Client
            builder.Services.AddHttpClient("ApiClient", client =>
            {
                var baseUrl = builder.Configuration["Api:BaseUrl"];
                client.BaseAddress = new Uri(baseUrl!);
            });

            // Session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Static files (wwwroot)
            app.UseStaticFiles();

            app.UseRouting();

            // Session MUST be after routing and before endpoints
            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
