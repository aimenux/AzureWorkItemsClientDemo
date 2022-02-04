using App.Extensions;
using Bullseye;
using Lib;
using Lib.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using AppHost = Microsoft.Extensions.Hosting.Host;

namespace App;

public static class Program
{
    public static async Task Main(string[] args)
    {
        using (var host = CreateHostBuilder(args).Build())
        {
            var targets = new Targets();
            var client = host.Services.GetRequiredService<IAzureDevopsClient>();

            targets.Add(TargetTypes.Default, dependsOn: new List<string>
            {
                TargetTypes.GetWorkItemsBySdk,
                TargetTypes.GetWorkItemsByRest,
            });

            targets.Add(TargetTypes.GetWorkItemsBySdk, async () =>
            {
                var results = await client.GetAzureWorkItemsAsync(AzureDevopsChoice.Sdk);
                var resultsDump = ObjectDumper.Dump(results);
                Console.WriteLine(resultsDump);
            });

            targets.Add(TargetTypes.GetWorkItemsByRest, async () =>
            {
                var results = await client.GetAzureWorkItemsAsync(AzureDevopsChoice.Rest);
                var resultsDump = ObjectDumper.Dump(results);
                Console.WriteLine(resultsDump);
            });

            await targets.RunAndExitAsync(args);
        }

        Console.WriteLine("Press any key to exit !");
        Console.ReadKey();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        AppHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile();
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureLogging((hostingContext, loggingBuilder) =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsoleLogger();
                loggingBuilder.AddNonGenericLogger();
                loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            })
            .ConfigureServices((hostingContext, services) =>
            {
                services.AddHttpClient<IAzureDevopsClient, AzureDevopsClient>();
                services.AddTransient<WorkItemTrackingHttpClient>(serviceProvider =>
                {
                    var settings = serviceProvider.GetRequiredService<IOptions<Settings>>().Value;
                    ObjectDumper.Dump(settings);
                    var credentials = new VssBasicCredential(string.Empty, settings.PersonalAccessToken);
                    var connection = new VssConnection(new Uri($"{settings.AzureDevopsUrl}/{settings.OrganizationName}"), credentials);
                    return connection.GetClient<WorkItemTrackingHttpClient>();
                });
                services.Configure<Settings>(hostingContext.Configuration.GetSection(nameof(Settings)));
            })
            .UseConsoleLifetime();

    private static class TargetTypes
    {
        public const string Default = "Default";
        public const string GetWorkItemsBySdk = "GetWorkItemsBySdk";
        public const string GetWorkItemsByRest = "GetWorkItemsByRest";
    }
}