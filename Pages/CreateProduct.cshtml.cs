using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace CatalogService.Pages
{
    public class CreateProductModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        public bool Success { get; set; } = false;

        public CreateProductModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public void OnGet() { }

        public void OnPost()
        {
            var newProduct = new
            {
                Name = Request.Form["Name"].ToString(),
                Brand = Request.Form["Brand"].ToString(),
                Category = Request.Form["Category"].ToString(),
                Price = decimal.TryParse(Request.Form["Price"].ToString(), out var price) ? price : 0,
                Description = Request.Form["Description"].ToString(),
                ReleaseDate = DateTime.UtcNow
            };

            using HttpClient client = _clientFactory.CreateClient("GatewayClient");
            try
            {
                var json = JsonSerializer.Serialize(newProduct);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PostAsync("api/products/", content).Result;
                Success = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
