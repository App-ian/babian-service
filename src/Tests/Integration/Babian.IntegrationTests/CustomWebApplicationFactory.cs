using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Babian.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var path = System.IO.Path.Combine(System.AppContext.BaseDirectory, "appsettings.Testing.json");
            config.AddJsonFile(path, optional: false, reloadOnChange: false);
        });
    }
}
