
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi.Auth.MicroServer.Model;
using WebApi.Auth.MicroServer.Service.IService;
using WebApi.Auth.MicroServer.Service;
using WebApi.Auth.MicroServer.Model.Dto;

namespace WebApi.Auth.MicroServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddDbContext<ApplicationDbContext>((options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))));
            builder.Services.AddControllers();
            builder.Services.AddTransient<ResponseDto>();

            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("ApiSettings:JwtOptions"));
            builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection("AdminSettings"));
            builder.Services.AddIdentity<ApplicationUser,IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            builder.Services.AddScoped<IAuthService, AuthService>();

           

            var app = builder.Build();

            // Configure the HTTP request pipeline.
           
            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseAuthentication();
            app.UseRouting();
            app.MapControllers();


            applyMigration();

            // Routing
            //Model Binding
            //Model Validation
            //Auth and Authorization
            //Exception Handling
            //Result formating



            app.Run();

            void applyMigration()
            {
                using (var scope = app.Services.CreateScope())
                {
                    var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    if (_db.Database.GetPendingMigrations().Count() > 0)
                    {
                        _db.Database.Migrate();
                    }
                }

            }
        }
    }
}
