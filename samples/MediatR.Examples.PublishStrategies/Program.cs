﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Examples.PublishStrategies
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddScoped<ServiceFactory>(p => p.GetService);

            services.AddSingleton<Publisher>();

            services.AddTransient<INotificationHandler<Pinged>>(sp => new SyncPingedHandler("1"));
            services.AddTransient<INotificationHandler<Pinged>>(sp => new AsyncPingedHandler("2"));
            services.AddTransient<INotificationHandler<Pinged>>(sp => new AsyncPingedHandler("3"));
            services.AddTransient<INotificationHandler<Pinged>>(sp => new SyncPingedHandler("4"));

            var provider = services.BuildServiceProvider();

            var publisher = provider.GetRequiredService<Publisher>();

            var pinged = new Pinged();

            foreach (PublishStrategy strategy in Enum.GetValues(typeof(PublishStrategy)))
            {
                Console.WriteLine($"Strategy: {strategy}");
                Console.WriteLine("----------");

                try
                {
                    await publisher.Publish(pinged, strategy);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.GetType()}: {ex.Message}");
                }

                await Task.Delay(1000);
                Console.WriteLine("----------");
            }

            Console.WriteLine("done");
        }
    }
}
