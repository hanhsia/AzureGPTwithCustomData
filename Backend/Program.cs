using Azure;
using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents;
using Backend.Adapters;
using Backend.Bots;
using Backend.Dialogs;
using Backend.Model;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Serilog;
using Services.Azure.Storage;
using Azure.AI.OpenAI;
using Backend.Service;

namespace Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConfigureMinimumThreads();

            var builder = WebApplication.CreateBuilder(args);

            ConfigureAppConfiguration(builder, args);

            ConfigureServices(builder);

            builder.Host.UseSerilog((hostingContext, services, loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
                loggerConfiguration.WriteTo.ApplicationInsights(
                    services.GetRequiredService<TelemetryConfiguration>(),
                    TelemetryConverter.Traces);
            });

            var app = builder.Build();
            Configure(app);

            app.Run();
        }

        private static void ConfigureAppConfiguration(WebApplicationBuilder builder, string[] args)
        {
            //use keyvaule if possible.
            var endpoint = builder.Configuration["KeyVault:URI"];
            var tenant = builder.Configuration["KeyVault:TenantId"];
            var id = builder.Configuration["KeyVault:ClientId"];
            var secret = builder.Configuration["KeyVault:ClientSecret"];

            builder.Configuration.Sources.Clear();

            if (!string.IsNullOrEmpty(endpoint))
            {
                if (string.IsNullOrEmpty(tenant) || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(secret))
                {
                    builder.Configuration.AddAzureKeyVault(new Uri(endpoint), new DefaultAzureCredential());
                }
                else
                {
                    builder.Configuration.AddAzureKeyVault(new Uri(endpoint), new ClientSecretCredential(tenant, id, secret));
                }
            }

            //enable appsettings
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

            //enable environment variables
            builder.Configuration.AddEnvironmentVariables();

            //enable command line parameters
            Dictionary<string, string> switchMappings = new Dictionary<string, string>
                    {
                            { "-i", "Input" },
                            { "--input","Input" }
                    };
            builder.Configuration.AddCommandLine(args, switchMappings);
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddCors();

            var aiOptions = new ApplicationInsightsServiceOptions();
            aiOptions.EnableAdaptiveSampling = false;
            builder.Services.AddApplicationInsightsTelemetry(aiOptions);
            builder.Services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();
            builder.Services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();
            builder.Services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();
            builder.Services.AddSingleton<TelemetryInitializerMiddleware>();

            builder.Services.AddSingleton<TelemetryLoggerMiddleware>(s => new TelemetryLoggerMiddleware(s.GetService<IBotTelemetryClient>(), true));

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(swagger =>
            {
                //This is to generate the Default UI of Swagger Documentation
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Demopro.net Web API",
                    Description = "Demopro.net Web API Swagger"
                });

            });

            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient("botframework", c =>
            {
                c.BaseAddress = new Uri("https://directline.botframework.com");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            var qdrantEndpoint= builder.Configuration["QdrantEndpoint"];
            if (!string.IsNullOrEmpty(qdrantEndpoint))
            {
                builder.Services.AddHttpClient("qdrant", c =>
                {
                    c.BaseAddress = new Uri(qdrantEndpoint);
                    c.DefaultRequestHeaders.Add("Accept", "application/json");
                });
            }
            

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            builder.Services.AddSingleton<IBotFrameworkHttpAdapter, DefaultAdapter>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            builder.Services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's _dialog implementation.)
            builder.Services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the _dialog system itself.)
            builder.Services.AddSingleton<ConversationState>();

            // The MainDialog that will be run by the bot.
            builder.Services.AddSingleton<MainDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            builder.Services.AddTransient<IBot, DialogBot<MainDialog>>();

            //builder.Services.Configure<BlobStorageClientSettings>(builder.Configuration.GetSection("BlobStorageClientSettings"));
            //builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();

            
            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddSearchIndexClient(builder.Configuration.GetSection("SearchIndexClientSettings"));
                clientBuilder.AddSearchClient(builder.Configuration.GetSection("SearchClientSettings"));
            });

            builder.Services.Configure<OpenAIClientSettings>(builder.Configuration.GetSection("OpenAIClientSettings"));
            builder.Services.Configure<AzureOpenAIClientSettings>(builder.Configuration.GetSection("AzureOpenAIClientSettings"));
            
            builder.Services.AddSingleton<AIService>();
            //builder.Services.AddOpenAIService();
        }

        private static void Configure(WebApplication app)
        {
            app.UseSerilogRequestLogging();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(setup =>
                {
                    setup.SwaggerEndpoint("/swagger/v1/swagger.json", "Demopro.net Web API v1");
                });
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(option =>
            {
                option.AllowAnyOrigin().
                AllowAnyMethod().
                AllowAnyHeader();
            });

            app.UseDefaultFiles();

            app.UseStaticFiles();

            app.UseWebSockets();

            app.UseRouting();

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();
        }

        private static void ConfigureMinimumThreads()
        {
            const int minThreadsPerLogicalProcessor = 10;
            int minThreadCount = Math.Min(Environment.ProcessorCount * minThreadsPerLogicalProcessor, 600);
            ThreadPool.SetMinThreads(minThreadCount, minThreadCount);
            System.Net.ServicePointManager.DefaultConnectionLimit=minThreadCount;
        }
    }
}