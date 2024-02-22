using Wallet.Core.Client;
using Newtonsoft.Json.Converters;
using Wallet.Core.Models;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Wallet.Auth.Models;
using AspNetCore.Identity.MongoDbCore.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Wallet {
    public class Startup {

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {

            #region MongoDBForModels
            services.Configure<MongoDBSettings>(
               Configuration.GetSection("MongoDb"));
            services.AddSingleton<CategoryService>();
            services.AddSingleton<SubCategoryService>();
            services.AddSingleton<WalletTransactionService>();
            #endregion


            #region Bson
            BsonSerializer.RegisterSerializer(new GuidSerializer(MongoDB.Bson.BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeSerializer(MongoDB.Bson.BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(MongoDB.Bson.BsonType.String));
            #endregion



            #region MongoDb Config
            var mongoDbIdentityConfig = new MongoDbIdentityConfiguration {
                MongoDbSettings = new MongoDbSettings {
                    ConnectionString = Configuration["MongoDb:ConnectionString"],
                    DatabaseName = Configuration["MongoDb:DatabaseName"]
                },
                IdentityOptionsAction = options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireLowercase = false;

                    //lockout
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                    options.Lockout.MaxFailedAccessAttempts = 5;

                    options.User.RequireUniqueEmail = true;

                }

            };

            services.ConfigureMongoDbIdentity<ApplicationUser, ApplicationRole, Guid>(mongoDbIdentityConfig)
                .AddUserManager<UserManager<ApplicationUser>>()
                .AddSignInManager<SignInManager<ApplicationUser>>()
                .AddRoleManager<RoleManager<ApplicationRole>>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;


            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters { 
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = "https://localhost:5001",
                    ValidAudience = "https://localhost:5001",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1swek3u4uo2u4a6e")),
                    ClockSkew = TimeSpan.Zero

                };
            });
            #endregion









            services.AddControllers();
            services.AddEndpointsApiExplorer();


            services
                .AddControllers()
                .AddNewtonsoftJson(options => {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

            services.AddMemoryCache();


            services.AddSwaggerDocument(config => {
                config.PostProcess = document => {
                    document.Info.Version = "v1.1";
                    document.Info.Title = "Wallet Api";
                    document.Info.Description = "Description";
                };
            });


        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {


            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();



            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();

            }

            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                                                    //.WithOrigins("https://localhost:44351")); // Allow only this origin can also have multiple origins separated with comma
                .AllowCredentials()); // allow credentials


            //app.MapControllers();


            app.UseAuthorization();

            app.UseSwaggerUi3();
            app.UseOpenApi();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });

        }


    }
}
