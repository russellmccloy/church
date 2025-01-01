using Azure.Storage.Blobs;
using church_api.Services.Abstractions;
using church_api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Cosmos;

namespace church_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Church API", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token in the text box below.\nExample:  eyJhbGciOiJIUzI1NiIsInR..."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                
                options.OperationFilter<SwaggerFileOperationFilter>();
            });


            // Register BlobServiceClient
            builder.Services.AddSingleton(x => new BlobServiceClient(builder.Configuration.GetConnectionString("AzureBlobStorage")));

            // Register IImageUploader and its implementation
            builder.Services.AddTransient<IFileUploader, FileUploader>();

            // Step 1: Add CosmosClient to DI container
            builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
            {
                var connectionString = builder.Configuration.GetValue<string>("CosmosDbSettings:Connection");
                return new CosmosClient(connectionString);
            });

            // Step 2: Register CosmosDbClient with the generic interface
            builder.Services.AddScoped(typeof(ICosmosDbClient<>), typeof(CosmosDbClient<>));

            // Step 3: Optionally, configure CosmosDbSettings for database/container names
            builder.Services.Configure<CosmosDbSettings>(builder.Configuration.GetSection("CosmosDbSettings"));

            var app = builder.Build();

            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
                {
                    options.InjectStylesheet("/swagger-ui/custom.css");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
