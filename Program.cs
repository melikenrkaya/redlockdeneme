using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RedlockDeneme.Data.Context;
using RedlockDeneme.Services;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace RedblockDeneme
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            builder.Services.AddControllers();

            builder.Services.AddDbContext<ApplicationDBContext>(options =>
            {

                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            }, ServiceLifetime.Scoped);

            builder.Services.AddHttpClient<HttpClient>();

            // Redis baðlantýsýný tekil olarak ekle
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");
                return ConnectionMultiplexer.Connect(configuration);
            });

            // Redis DB nesnesini ekle
            builder.Services.AddScoped<IDatabase>(sp =>
            {
                var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
                return multiplexer.GetDatabase();
            });

            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                return ConnectionMultiplexer.Connect("localhost"); // ya da configten al
            });

            builder.Services.AddSingleton<IDistributedLockFactory>(sp =>
            {
                var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
                return RedLockFactory.Create(new List<RedLockMultiplexer>
    {
        new RedLockMultiplexer(multiplexer)
    });
            });

            builder.Services.AddScoped<IStok, StokServices>();
            builder.Services.AddScoped<ISepet, SepetServices>();

            //builder.Services.AddRazorPages();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = string.Empty; // Swagger UI'yi kök URL'ye ayarlamak için
                });
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}