using MelbergFramework.Core.Redis;
using MelbergFramework.Infrastructure.Redis;

namespace Infrastructure.Redis.Contexts;

public class PlaneHistoryContext : RedisContext
{
    public PlaneHistoryContext(IRedisConfigurationProvider provider) : base(provider) { }
}