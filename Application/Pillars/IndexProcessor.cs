using System.Globalization;
using DomainService;
using MelbergFramework.Infrastructure.Rabbit.Consumers;
using MelbergFramework.Infrastructure.Rabbit.Extensions;
using MelbergFramework.Infrastructure.Rabbit.Messages;

namespace Application.Pillars;

public class IndexProcessor : IStandardConsumer
{
    private readonly IAggregaterDomainService _domainService;
    private readonly ILogger<IndexProcessor> _logger;
    public IndexProcessor(
        IAggregaterDomainService domainService,
        ILogger<IndexProcessor> logger
        )
    {
        _domainService = domainService;
        _logger = logger;
    }
    public async Task ConsumeMessageAsync(Message message, CancellationToken ct)
    {
        var time = ExtractTimestamp(message.GetTimestamp());
        
        _logger.LogInformation("Trying");
        try
        {
            await _domainService.AggregatePlanes(time); 
        }
        catch (System.Exception ex)
        {
            
            _logger.LogInformation("whell");
            Console.Write(ex.Message);
        }
        
        _logger.LogInformation("Aggregated for minute {time}", time-60);

    }
    private long ExtractTimestamp(DateTime time) => 
        (long) Math.Floor(
            time.Subtract(DateTime.UnixEpoch).TotalSeconds
            );
}
