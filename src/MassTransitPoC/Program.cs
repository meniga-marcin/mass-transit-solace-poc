using MassTransit;
using Microsoft.Extensions.Hosting;
namespace MassTransitPoC;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Try to connect via RabbitMQ transport
        // It will fail because the RabbitMQ transport isn't designed to support Solace broker
        // Why?
        //  Abstraction over the Event Broker is provided by Mass Transit library - currently version 8.2.3 but the same applies to latest 8.3.0
        /// Mass Tansit is using RabbitMQ.Client package - version 6.8.1. https://github.com/MassTransit/MassTransit/blob/v8.3.0/Directory.Packages.props#L92
        ///   This client API is closely modelled on the AMQP 0-9-1 protocol model, with additional abstractions for ease of use. https://www.rabbitmq.com/client-libraries/dotnet-api-guide#major-api-elements
        /// 
        /// RabbitMQ team has released a AMQP 1.0 client library https://www.rabbitmq.com/client-libraries/amqp-client-libraries and is using AMQP 1.0 as default protocol since version 4.0
        //  However, Mass Transit maintainer is not planning to switch to using the AMQP 1.0 RabbitMQ client anytime soon. Last update on this was from 2024-07 https://github.com/MassTransit/MassTransit/discussions/5395

        /// AMQ 0.9.1 and AMQP 1.0 are not interchangable and backward compatible: https://seventhstate.io/amqp-091-vs-1/. Current abstraction layer over Event Broker would need to be extended 
        /// using AMQP 1.0 compatible client libraries.
        await Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {

                // https://masstransit.io/documentation/configuration/transports/rabbitmq#minimal-example
                services.AddMassTransit(x =>
                {
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(new Uri("amqp://localhost:5674/"));
                    });
                });
            })
            .Build()
            .RunAsync();

        // Try to connect via ActiveMQ transport
        // It will fail because the ActiveMQ transport isn't designed to support Solace broker
        // 
        // Why?
        //  Abstraction over the Event Broker is provided by Mass Transit library - currently version 8.2.3 but the same applies to latest 8.3.0
        //   Apache.NMS.ActiveMQ Version="2.1.0" https://github.com/MassTransit/MassTransit/blob/v8.3.0/Directory.Packages.props#L6
        // 
        // The ActiveMQ NMS client is a .NET client that communicates with the ActiveMQ broker using its native Openwire protocol. This is not a protocol supported by Solace

        // await Host.CreateDefaultBuilder(args)
        //     .ConfigureServices(services =>
        //     {
        //         services.AddMassTransit(x =>
        //         {
        //             x.UsingActiveMq((context, cfg) =>
        //             {
        //                 cfg.Host("localhost", h =>
        //                 {
        //                     h.UseSsl();
        //                     h.Username("admin");
        //                     h.Password("admin");
        //                 });
        //             });
        //         });
        //     })
        //     .Build()
        //     .RunAsync();
    }
}