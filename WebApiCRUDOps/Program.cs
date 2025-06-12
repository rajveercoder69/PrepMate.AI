using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using WebApiCRUDOps.DataUtility;
using WebApiCRUDOps.DataUtility.DataRepository.IDataRepository;
using WebApiCRUDOps.DataUtility.DataRepository;
using WebApiCRUDOps.ApiVerification;
using AutoMapper;
using Microsoft.AspNetCore.Http.Features;

namespace WebApiCRUDOps
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.AddAppAuthetication();
            builder.Services.AddDbContext<ApplicationDbContext>((options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))));
            builder.Services.AddScoped<ISaveChange, SaveChange>();
            IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
            builder.Services.AddSingleton(mapper);
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            //// Allow up to 100MB file upload
            //builder.Services.Configure<FormOptions>(options =>
            //{
            //    options.MultipartBodyLengthLimit = 104857600; // 100 MB
            //});

            //builder.WebHost.ConfigureKestrel(options =>
            //{
            //    options.Limits.MaxRequestBodySize = 104857600; // Match with form limit
            //});


            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();
            // Add your custom JWT authentication setup


            app.UseRouting();

            app.UseAuthentication();   // must come BEFORE UseAuthorization

            app.UseAuthorization();

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
                using (var scope=app.Services.CreateScope())
                {
                    var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    if (_db.Database.GetPendingMigrations().Count()>0)
                    {
                        _db.Database.Migrate();
                    }
                }

            }
        }
    }
}
