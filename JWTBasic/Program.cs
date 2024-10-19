
using JWTBasic.Configurations;
using JWTBasic.Data;
using Microsoft.EntityFrameworkCore;

namespace JWTBasic
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            /*
                Configuring the JWTConfig class to automatically read the Secret value from the configuration file and make it available throughout the application.
                Binding the Secret value from the JWTConfig section in appsettings.json and is automatically bound to the JWTConfig class, which is injected into services or controllers. 
            */
            builder.Services.Configure<JWTConfig>(builder.Configuration.GetSection(key: nameof(JWTConfig)));

            builder.Services.AddDbContext<ApiDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString(name: "DefaultConnection")));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
