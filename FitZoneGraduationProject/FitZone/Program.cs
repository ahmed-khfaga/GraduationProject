using System;
using System.Text;
using FitZone.APIs.Helper;
using FitZone.APIs.Middlewares;
using FitZone.APIs.SignalR;
using FitZone.Core.Entitys.Identity;
using FitZone.Core.Repository.Contract;
using FitZone.Repository;
using FitZone.Repository.Data;
using FitZone.Service;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract;
using FitZone.Service.Services.Contract.Chat;
using FitZone.Services.Contract;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FitZone
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddSignalR();
            #region connectionString  

            builder.Services.AddDbContext<FitContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("cs"));
            }
           );
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<FitContext>();
            #endregion

            #region Repository layer

            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            #endregion

            #region Service - AutoMapper

            //builder.Services.AddAutoMapper(M => M.AddProfile(new MappingMemberShip()));
            //builder.Services.AddAutoMapper(typeof(MappingMemberShip));
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            //builder.Services.AddAutoMapper(typeof(MappingCoach));
            //builder.Services.AddAutoMapper(typeof(MappingTrainee));
            //builder.Services.AddAutoMapper(typeof(MappingTrack));
            //builder.Services.AddAutoMapper(typeof(MappingWorkoutProgram));
            //builder.Services.AddAutoMapper(typeof(MappingWorkoutSession));
            //builder.Services.AddAutoMapper(typeof(MappingExercise));
            //builder.Services.AddAutoMapper(typeof(MappingSessionExercise));



            builder.Services.AddScoped(typeof(IMembershipService),typeof (MembershipService));
            builder.Services.AddScoped(typeof(IAuthService),typeof(AuthService));
            builder.Services.AddScoped<ITrackService, TrackService>();
            builder.Services.AddScoped<IProgramService, ProgramService>();
            builder.Services.AddScoped<IExerciseService, ExerciseService>();
            builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
            builder.Services.AddScoped<ICoachService, CoachService>();
            builder.Services.AddScoped<ITraineeService, TraineeService>();

            #endregion


            #region JWT Authentication
            builder.Services.AddAuthentication(option =>
            {
                // check JWT token header 
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                // if go th action [authrize]
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // unauth

                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option => {  // verified key 
                option.SaveToken = true;
                option.RequireHttpsMetadata = false;
                option.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
                };


                option.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/chatHub"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            #endregion

            #region CORS

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });
            #endregion


            #region Validation error formatting

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

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new()
                {
                    Title = "FitZone APIs",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter: Bearer {your JWT token}"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
            });

            #endregion

            #region RegisterMessageService

            builder.Services.AddSignalR();
            builder.Services.AddScoped<IChatService, ChatService>(); 
            #endregion

            // ?? Pipeline ????????????????????

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                await FitZone.Repository.Data.Seed.FitZoneSeeder.SeedAsync(app.Services);
            }

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

            app.MapHub<ChatHub>("/chatHub");

            app.Run();
        }
    }
}
