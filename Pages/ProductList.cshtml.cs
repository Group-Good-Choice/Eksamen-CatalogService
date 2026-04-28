using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CatalogService.Pages
{
    public class ProductListModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        public List<ProductItemDTO>? Products { get; set; }

        public ProductListModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public void OnGet()
        {
            using HttpClient client = _clientFactory.CreateClient("GatewayClient");
            try
            {
                Products = client.GetFromJsonAsync<List<ProductItemDTO>>("api/products/").Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class ProductItemDTO
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public string? Category { get; set; }
        public decimal Price { get; set; }
    }
}
