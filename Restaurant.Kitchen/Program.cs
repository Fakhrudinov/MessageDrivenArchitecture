using System;
using System.Security.Authentication;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Restaurant.Kitchen.Consumers;

namespace Restaurant.Kitchen
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfigurationRoot config = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json").Build();
                    IConfigurationSection sect = config.GetSection("HostConfig");

                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<KitchenTableBookedConsumer>();

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(
                                sect.GetSection("HostName").Value,
                                ushort.Parse(sect.GetSection("Port").Value),
                                sect.GetSection("VirtualHost").Value,
                                h =>
                                {
                                    h.UseSsl(s =>
                                    {
                                        s.Protocol = SslProtocols.Tls12;
                                    });

                                    h.Username(sect.GetSection("UserName").Value);
                                    h.Password(sect.GetSection("Password").Value);
                                });

                            cfg.UseMessageRetry(r =>
                            {
                                r.Exponential(5,
                                    TimeSpan.FromSeconds(1),
                                    TimeSpan.FromSeconds(100),
                                    TimeSpan.FromSeconds(5));
                                r.Ignore<StackOverflowException>();
                                r.Ignore<ArgumentNullException>(x => x.Message.Contains("Consumer"));
                            });

                            cfg.ConfigureEndpoints(context);
                        });
                    });

                    services.AddSingleton<Manager>();
                    
                    services.AddMassTransitHostedService(true);
                });
    }
}