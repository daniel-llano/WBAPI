using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using WBAPI.Infrastructure.Data;

namespace WBAPI.IntegrationTests.Helpers;

/// <summary>
/// Custom WebApplicationFactory that replaces SQL Server with an in-memory database.
/// Each test class instance gets its own isolated DB.
///
/// IMPORTANT: Program.cs captures Jwt:Secret into a local variable before Build() is called,
/// so ConfigureAppConfiguration alone cannot override the signing key used by the JWT middleware.
/// We must also call PostConfigureAll&lt;JwtBearerOptions&gt; to align both the signing (JwtService)
/// and validation (JwtBearer middleware) to use the same test secret.
/// </summary>
public class WbapiFactory : WebApplicationFactory<Program>
{
    private const string TestSecret = "TEST_ONLY_SECRET_THAT_IS_LONG_ENOUGH_32CHARS";
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Override IConfiguration so JwtService (reads config lazily at runtime) uses TestSecret
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"]          = TestSecret,
                ["Jwt:Issuer"]          = "WBAPI",
                ["Jwt:Audience"]        = "WBAPIClients",
                ["Jwt:ExpirationHours"] = "1",
                ["ConnectionStrings:DefaultConnection"] = "NOT_USED_IN_TESTS"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove real SQL Server DbContext
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll(typeof(AppDbContext));

            // Add isolated in-memory database per factory instance
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Override the JWT bearer validation key.
            // Program.cs captures builder.Configuration["Jwt:Secret"] into a local variable
            // BEFORE ConfigureAppConfiguration runs, so the middleware's IssuerSigningKey
            // would otherwise hold the appsettings.json value while JwtService uses TestSecret.
            // PostConfigureAll runs after all service registrations and corrects the mismatch.
            services.PostConfigureAll<JwtBearerOptions>(options =>
            {
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecret));
                options.TokenValidationParameters.ValidIssuer   = "WBAPI";
                options.TokenValidationParameters.ValidAudience = "WBAPIClients";
            });
        });
    }
}
