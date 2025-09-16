using Application.Messages;
using Application.Pillars;
using Common;
using DomainService;
using Infrastructure.MongoDB;
using Infrastructure.Redis.Contexts;
using Infrastructure.Redis.Repository;
using Infrastructure.Repository.Core;
using MelbergFramework.Application;
using MelbergFramework.Core.Time;
using MelbergFramework.Infrastructure.Rabbit;
using MelbergFramework.Infrastructure.Rabbit.Messages;
using MelbergFramework.Infrastructure.Redis;

namespace Application;

public class AppRegistrator : Registrator
{
    public override void RegisterServices(IServiceCollection services)
    {
        RabbitModule.RegisterMicroConsumer<IngressProcessor, CompletedPlaneFrameMessage>(services, false);
        RabbitModule.RegisterMicroConsumer<IndexProcessor, TickMessage>(services,true);

        services.AddTransient<IIngressDomainService, IngressDomainService>();
        services.AddTransient<IAggregaterDomainService, AggregaterDomainService>();
        services.AddTransient<IAccessDomainService, AccessDomainService>();

        // CouchbaseModule.RegisterCouchbaseBucket<ILastSeenPointerRepository,LastSeenPointerRepository>(services);
        // CouchbaseModule.RegisterCouchbaseBucket<IPlaneHistoryRepository,PlaneHistoryRepository>(services);
        RedisDependencyModule.LoadRedisRepository<IPlaneHistoryCacheRepository, PlaneShortTermHistoryRepository, PlaneHistoryContext>(services);

        services.AddSingleton<IClock, Clock>();

        services.AddOptions<TimingsOptions>()
            .BindConfiguration(TimingsOptions.Timing)
            .ValidateDataAnnotations();
        services.AddSwaggerGen();

        services.AddTransient<IPlaneHistoryRepository, LongTermPlaneHistoryRepository>();
        services.AddSingleton<PlaneClient>();
        services.AddOptions<MongoDBOptions>()
            .BindConfiguration(MongoDBOptions.Section)
            .ValidateDataAnnotations();
    }
}
