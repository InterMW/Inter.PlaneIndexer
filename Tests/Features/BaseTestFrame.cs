using Application;
using Application.Messages;
using Application.Pillars;
using Infrastructure.Redis.Contexts;
using LightBDD.MsTest3;
using MelbergFramework.Application;
using MelbergFramework.ComponentTesting.Couchbase;
using MelbergFramework.ComponentTesting.Rabbit;
using MelbergFramework.ComponentTesting.Redis;
using MelbergFramework.Core.ComponentTesting;
using MelbergFramework.Core.DependencyInjection;
using MelbergFramework.Core.Time;
using MelbergFramework.Infrastructure.Rabbit.Messages;
using MelbergFramework.Infrastructure.Rabbit.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests.Features;

public class BaseTestFrame : FeatureFixture
{
    public BaseTestFrame()
    {
        App = MelbergHost.CreateHost<AppRegistrator>()
            .AddServices(_ => 
            {
                _.OverrideRedisContext<PlaneHistoryContext>();
                _.OverrideCouchbaseDatabase();
                _.PrepareConsumer<IngressProcessor>();
                _.OverrideTranslator<CompletedPlaneFrameMessage>();
                _.PrepareConsumer<IndexProcessor>();
                _.OverrideTranslator<TickMessage>();
                _.OverrideWithSingleton<IClock,MockClock>();
            })
            .AddControllers()
            .Build();
    }
    public WebApplication App;

    public T GetClass<T>() => (T)App
        .Services
        .GetRequiredService(typeof(T));
    
    public RabbitMicroService<IndexProcessor> GetIndexService() =>
        (RabbitMicroService<IndexProcessor>)App
            .Services
            .GetServices<IHostedService>()
            .Where(_ => _.GetType() == typeof(RabbitMicroService<IndexProcessor>))
            .First();

    public RabbitMicroService<IngressProcessor> GetIngressService() =>
        (RabbitMicroService<IngressProcessor>)App
            .Services
            .GetServices<IHostedService>()
            .Where(_ => _.GetType() == typeof(RabbitMicroService<IngressProcessor>))
            .First();

}
