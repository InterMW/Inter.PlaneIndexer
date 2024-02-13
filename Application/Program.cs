using Application.Messages;
using Application.Pillars;
using Common;
using DomainService;
using Infrastructure.Couchbase;
using Infrastructure.Redis.Contexts;
using Infrastructure.Redis.Repository;
using Infrastructure.Repository.Core;
using MelbergFramework.Application;
using MelbergFramework.Infrastructure.Couchbase;
using MelbergFramework.Infrastructure.Rabbit;
using MelbergFramework.Infrastructure.Rabbit.Messages;
using MelbergFramework.Infrastructure.Redis;

namespace Application;

public class Program
{
    public static void Main(string[] args)
    {
        ThreadPool.SetMinThreads(8,8);
        
        var builder = WebApplication.CreateBuilder();
        
        Register(builder.Services,builder.Environment.IsDevelopment());
        
        builder.Services.AddControllers().AddNewtonsoftJson();
        var app = builder.Build();

        if(app.Environment.IsDevelopment())
        {
            app.Configuration["Rabbit:ClientDeclarations:Connections:0:Password"] = app.Configuration["rabbit_pass"];
        } 
        
        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseCors(_ => _
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials()
            );
        app.UseSwagger();
        app.UseSwaggerUI();

        app.Run();
    }
    
    private static void Register(IServiceCollection collection,bool isDevelopment)
    {
        collection.RegisterRequired();
        RabbitModule.RegisterMicroConsumer<IngressProcessor, CompletedPlaneFrameMessage>(collection,!isDevelopment);
        RabbitModule.RegisterMicroConsumer<IndexProcessor, TickMessage>(collection,!isDevelopment);

        collection.AddTransient<IIngressDomainService,IngressDomainService>();
        collection.AddTransient<IAggregaterDomainService,AggregaterDomainService>();
        collection.AddTransient<IAccessDomainService, AccessDomainService>();
        
        CouchbaseModule.RegisterCouchbaseBucket<ILastSeenPointerRepository,LastSeenPointerRepository>(collection);
        CouchbaseModule.RegisterCouchbaseBucket<IPlaneHistoryRepository,PlaneHistoryRepository>(collection);
        RedisModule.LoadRedisRepository<IPlaneHistoryCacheRepository,PlaneShortTermHistoryRepository,PlaneHistoryContext>(collection);

        collection.AddOptions<TimingsOptions>()
            .BindConfiguration(TimingsOptions.Timing)
            .ValidateDataAnnotations();
                collection.AddSwaggerGen();
    }
}
