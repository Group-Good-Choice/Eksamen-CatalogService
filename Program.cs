using CatalogService.Repositories;
using NLog;
using NLog.Web;
using Scalar.AspNetCore;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();
logger.Debug("start catalog service");

try
{
    var builder = WebApplication.CreateBuilder(args);

    var gatewayUrl = builder.Configuration["GatewayUrl"] ?? "http://gateway";
    builder.Services.AddHttpClient("GatewayClient", client =>
    {
        client.BaseAddress = new Uri(gatewayUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

    builder.Services.AddRazorPages();
    builder.Services.AddControllers();
    builder.Services.AddMemoryCache();
    builder.Services.AddCors();
    builder.Services.AddSingleton<IProductRepository, ProductRepository>();
    builder.Services.AddOpenApi();

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();

    app.UseCors(policy => policy
        .SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrEmpty(origin)) return false;
            try { return new Uri(origin).Host == "localhost"; }
            catch { return false; }
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.UseHttpsRedirection();
    }

    app.UseAuthorization();
    app.MapControllers();
    app.UseStaticFiles();
    app.MapGet("/", () => Results.Redirect("/api/products/list"));
    app.MapRazorPages();
    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}
