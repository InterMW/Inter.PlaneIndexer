using Common;
using Domain;
using Infrastructure.Redis.Contexts;
using Infrastructure.Repository.Core;
using MelbergFramework.Infrastructure.Redis.Repository;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Infrastructure.Redis.Repository;

public class PlaneShortTermHistoryRepository : RedisRepository<PlaneHistoryContext>, IPlaneHistoryCacheRepository
{
    private readonly TimeSpan _documentLifetime;
    public PlaneShortTermHistoryRepository(
        IOptions<TimingsOptions> options,
        PlaneHistoryContext context) : base(context)
    {
        _documentLifetime = TimeSpan.FromSeconds(options.Value.PlaneDocLifetimesSecs);
    }

    public Task RecordPlane(Plane plane, long now) =>
        DB.StringSetAsync(
            ToKey(plane, now),
            JsonConvert.SerializeObject(plane),
            _documentLifetime
        );
    
    public async IAsyncEnumerable<TimestampedPlaneRecord> GetPlanesInRange(long start, long end, string hexValue = "*")
    {
        for(long time = start; time <= end; time ++)
        {
            await foreach(var key in Server.KeysAsync(pattern:ToKey(hexValue,time)))
            {
                var result = await DB.StringGetAsync(key);
                yield return new()
                {
                    Timestamp = time,
                    Data = JsonConvert.DeserializeObject<Plane>(result)
                };
            }
        }
    }
    
    static string ToKey(string hexValue, long now) =>$"plane_indexer_{hexValue}_{now}";
    static string ToKey(Plane plane, long now) => ToKey(plane.HexValue,now);

}