using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.API.Data;

namespace RaceDay.Api.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"RaceDayTest_{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Provide JWT settings for the test application.
            builder.ConfigureAppConfiguration((context, configuration) =>
                {
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                        //pseudo like-thingy for the one i  have in my appsettings.json....they are NOT related. this one os for the in-memory thing and is in no way linked to mu database
                            ["Jwt:Key"] = "RaceDay_Test_Key_12345678901234567890",
                            ["Jwt:Issuer"] = "RaceDay.Tests",
                            ["Jwt:Audience"] = "RaceDay.Tests"
                        });
                });

            builder.ConfigureServices(services =>
            {
                // Remove the normal database registration.
                services.RemoveAll<RaceDayDbContext>();

                services.RemoveAll<DbContextOptions<RaceDayDbContext>>();

                // Give this test factory its own database.
                services.AddDbContext<RaceDayDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(_databaseName);
                    });
            });
        }
    }
}

//this class is for creating an in-memory database so it doesnt interefere with the actual database.