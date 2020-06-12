using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueshift.EntityFrameworkCore.MongoDB.Infrastructure;
using EventShare.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventShare.Poller
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    services.AddTransient<IEventPoller, FakeEventPoller>();

                    services.AddDbContext<EventShareDbContext>(options =>
                        options.UseMongoDb(hostContext.Configuration.GetConnectionString("MongoDbConnection")));
                });
    }
}
