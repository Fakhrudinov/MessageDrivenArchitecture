using System.Security.Authentication;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Restaurant.Kitchen.Consumers;

namespace Restaurant.Kitchen
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
                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<KitchenTableBookedConsumer>();

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host("puffin-01.rmq2.cloudamqp.com", 5671, "mdfgqynj", h =>
                            {
                                h.UseSsl(s =>
                                {
                                    s.Protocol = SslProtocols.Tls12;
                                });

                                h.Username("mdfgqynj");
                                h.Password("KP_9GlzSM3mSMJcmcTSV6frQakgLX19V");
                            });


                            cfg.ConfigureEndpoints(context);
                        });
                    });

                    services.AddSingleton<Manager>();
                    
                    services.AddMassTransitHostedService(true);
                });
    }
}