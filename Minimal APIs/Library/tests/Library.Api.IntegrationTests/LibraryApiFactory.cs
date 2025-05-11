using Library.Web;
using Library.Web.Data;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Library.Api.IntegrationTests;

public class LibraryApiFactory : WebApplicationFactory<Marker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IDbConnectionFactory));
            services.AddSingleton<IDbConnectionFactory>(_ =>
                new SqliteConnectionFactory("Data source=file:inmem?mode=memory&cache=shared"));
        });
    }
}
