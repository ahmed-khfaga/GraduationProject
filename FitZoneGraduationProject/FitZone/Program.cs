using System;
using FitZone.APIs.Errors;
using FitZone.APIs.Helper;
using FitZone.APIs.Middlewares;
using FitZone.Core.Repository.Contract;
using FitZone.Repository;
using FitZone.Repository.Data;
using Microsoft.AspNetCore.Mvc;
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

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });


            builder.Services.Configure<ApiBehaviorOptions>(option => 
            {
                option.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState.Where(p => p.Value.Errors.Count() > 0)
                                                            .SelectMany(p => p.Value.Errors)
                                                            .Select(E => E.ErrorMessage)
                                                            .ToArray();

                    var validationErrorResponse = new ApiValidationErrorRes() 
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(validationErrorResponse);
                };                                  
            });

            builder.Services.AddSwaggerGen();

            #endregion

            var app = builder.Build();
            app.UseMiddleware<ExceptionMiddleware>();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
         
            app.UseCors("MyPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
