using Domain;
using Infrastructure.Repository.Core;

namespace DomainService;

public interface IAggregaterDomainService 
{
    Task AggregatePlanes(long now);
}

public class AggregaterDomainService : IAggregaterDomainService
{
    private readonly IPlaneHistoryCacheRepository _planeHistoryCache;
    private readonly ILastSeenPointerRepository _lastSeenRepository;
    private readonly IPlaneHistoryRepository _planeHistoryRepository;
    
    public AggregaterDomainService(
        IPlaneHistoryCacheRepository planeHistoryCacheRepository,
        ILastSeenPointerRepository lastSeenPointerRepository,
        IPlaneHistoryRepository planeHistoryRepository
    )
    {
        _planeHistoryCache = planeHistoryCacheRepository;
        _lastSeenRepository = lastSeenPointerRepository;
        _planeHistoryRepository = planeHistoryRepository;
    }
    public async Task AggregatePlanes(long now)
    {
        var offsetTime = now - 60;
        var relevantPlanes = await _planeHistoryCache.GetPlanesInRange(offsetTime, offsetTime + 59).ToListAsync();
        //Fix this, make it so that it uses configuration, maybe we should hold onto info for a full 2 min?
        Console.WriteLine(relevantPlanes.Count);
        
        
        var seenPlanes = relevantPlanes.Select(_ => _.Data.HexValue).ToHashSet().ToList();
        foreach( var hexValue in seenPlanes)
        {
            var data = new PlaneDataRecordLink()
            {
                PreviousLink = await _lastSeenRepository.GetLastSeenTimeAsync(hexValue),
                Planes = relevantPlanes.Where(_ => _.Data.HexValue == hexValue)
            };
            
            await _planeHistoryRepository.StorePlaneHistory(hexValue,offsetTime, data);
            
            await _lastSeenRepository.SetLastSeenTimeAsync(hexValue,offsetTime);

        }
    }
    
    private async Task StoreInfo(string hexValue, long now, IEnumerable<TimestampedPlaneRecord> records )
    {
    }

}