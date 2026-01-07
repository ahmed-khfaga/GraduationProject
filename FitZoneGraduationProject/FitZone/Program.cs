using System;
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
