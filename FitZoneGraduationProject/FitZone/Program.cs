using System;
using FitZone.APIs.Helper;
using FitZone.Core.Repository.Contract;
using FitZone.Repository;
using FitZone.Repository.Data;
using Microsoft.EntityFrameworkCore;

namespace FitZone
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();
            builder.Services.AddOpenApi();



            #region connectionString and services 

            builder.Services.AddDbContext<FitContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("cs"));
            }



           );

            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //builder.Services.AddAutoMapper(M => M.AddProfile(new MappingMemberShip()));
             builder.Services.AddAutoMapper(typeof(MappingMemberShip));


            #endregion






            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
