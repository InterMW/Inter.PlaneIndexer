using Common;
using Domain;
using Infrastructure.Redis.Contexts;
using Infrastructure.Repository.Core;
using MelbergFramework.Infrastructure.Redis;
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
    public async Task RecordPlane(PlaneMinimal plane, long now)
    {
        try
        {
            var key = ToKey(plane,now);
            var checkinKey = ToCheckinKey(now);
            await DB.SetAddAsync(checkinKey,plane.HexValue);
            await DB.KeyExpireAsync(checkinKey,_documentLifetime);
            Console.WriteLine(key);
            await DB.ListRightPushAsync(key,JsonConvert.SerializeObject(plane));
            await DB.KeyExpireAsync(key,_documentLifetime);
        }
        catch (System.Exception)
        {
            
            throw;
        }
    } 

    public async Task<IEnumerable<string>> GetPlanesInMinute(long min)
    {
        return (await DB.SetMembersAsync(ToCheckinKey(min))).Select(_ => (string)_);
    }
    
    public async Task<IEnumerable<PlaneMinimal>> GetPlaneMinute(string hexValue, long min)
    {
        var planes = await DB.ListRangeAsync(ToKey(hexValue,min));
        if(planes.Any() && planes[0].HasValue)
        {
            return planes.Select(_ => JsonConvert.DeserializeObject<PlaneMinimal>(_));
        }
        return Array.Empty<PlaneMinimal>();
    }
    
    static string ToCheckinKey(long now) => $"plane_indexer_checkin_{now}";
    static string ToKey(string hexValue, long now) =>$"plane_indexer_{hexValue}_{now}";
    static string ToKey(PlaneMinimal plane, long now) => ToKey(plane.HexValue,now);

}
