using MelbergFramework.Infrastructure.Redis;
using Microsoft.Extensions.Options;

namespace Infrastructure.Redis.Contexts;

public class PlaneHistoryContext : RedisContext
{
    public PlaneHistoryContext( 
            IOptions<RedisConnectionOptions<PlaneHistoryContext>> options,
            IConnector connector) : base(options.Value, connector) { }
}
