using System;
using System.Security.Authentication;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace Restaurant.Booking
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(x =>
                    {
                        x.UsingRabbitMq((context,cfg) =>
                        {
                            cfg.Host("puffin-01.rmq2.cloudamqp.com", 5671, "mdfgqynj",  h =>
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
                    services.AddMassTransitHostedService(true);

                    services.AddTransient<Restaurant>();

                    services.AddHostedService<Worker>();
                });
    }
}