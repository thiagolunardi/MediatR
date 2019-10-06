using System.Threading;

namespace MediatR.Tests.Pipeline
{
    using System.Threading.Tasks;
    using MediatR.Pipeline;
    using Shouldly;
    using StructureMap;
    using Xunit;

    public class RequestPostProcessorTests
    {
        public class Ping : IRequest<Pong>
        {
            public string Message { get; set; }
        }

        public class Pong
        {
            public string Message { get; set; }
        }

        public class PingHandler : IRequestHandler<Ping, Pong>
        {
            public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new Pong { Message = request.Message + " Pong" });
            }
        }

        public class PingPongPostProcessor : IRequestPostProcessor<Ping, Pong>
        {
            public Task Process(Ping request, Pong response, CancellationToken cancellationToken)
            {
                response.Message = response.Message + " " + request.Message;

                return Task.FromResult(0);
            }
        }

        [Fact]
        public async Task Should_run_postprocessors()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType(typeof(PublishTests));
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                    scanner.AddAllTypesOf(typeof(IRequestPostProcessor<,>));
                });
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestPostProcessorBehavior<,>));
                cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var response = await mediator.Send(new Ping { Message = "Ping" });

            response.Message.ShouldBe("Ping Pong Ping");
        }

    }
}