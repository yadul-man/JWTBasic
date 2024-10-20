
using JWTBasic.Configurations;
using JWTBasic.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

            /*
                Sets up the default ASP.NET Core Identity system using the IdentityUser class.
                It disables the requirement for users to confirm their accounts via email before logging in.
                It stores identity information using Entity Framework with the specified ApiDbContext. 
            */
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ApiDbContext>();

            /*
                This configures the application to use JWT authentication as the default method for verifying users, meaning all incoming requests will be authenticated based on the JWT bearer token. 
                ValidateIssuerSigningKey = true: This ensures that the JWT token is validated using the signing key. In this case, the signing key is the symmetric security key created from the jwtSecret.
                IssuerSigningKey = new SymmetricSecurityKey(key): This specifies the symmetric key (derived from the jwtSecret) that will be used to verify the JWT's signature.
                ValidateIssuer = true: This ensures that the issuer of the token is validated (i.e., the token must come from a trusted source).
                ValidateAudience = false: Audience validation is disabled, meaning the token does not need to have a specific audience (e.g., a specific app or user).
                RequireExpirationTime = false: This disables the requirement for tokens to have an expiration time. (You would normally want to set this to true in production for security.)
                ValidateLifetime = false: This disables the validation of the token's expiration time.
            */
            builder.Services.AddAuthentication(configureOptions: options =>
            {
                options.DefaultAuthenticateScheme = options.DefaultScheme = options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwt =>
            {
                byte[] key = Array.Empty<byte>();

                var jwtSecret = builder.Configuration.GetSection(key: "JwtConfig:Secret").Value;
                if (jwtSecret != null)
                {
                    key = Encoding.ASCII.GetBytes(jwtSecret);
                }

                jwt.SaveToken = true;
                jwt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = false,
                    ValidateLifetime = false
                };
            });

            builder.Services.AddCors(options => options.AddPolicy(name: "Frontend", configurePolicy: policy =>
            {
                policy.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader();
            }));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseCors("Frontend");

            app.MapControllers();

            app.Run();
        }
    }
}
