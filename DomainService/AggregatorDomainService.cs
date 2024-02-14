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
        var relevantPlanes = await _planeHistoryCache.GetPlanesInMinute(offsetTime);
        
        foreach( var hexValue in relevantPlanes)
        {
            var data = new PlaneDataRecordLink()
            {
                PreviousLink = await _lastSeenRepository.GetLastSeenTimeAsync(hexValue),
                Planes = await _planeHistoryCache.GetPlaneMinute(hexValue,offsetTime)
            };
            
            await _planeHistoryRepository.StorePlaneHistory(hexValue,offsetTime, data);
            
            await _lastSeenRepository.SetLastSeenTimeAsync(hexValue,offsetTime);
        }
    }
}