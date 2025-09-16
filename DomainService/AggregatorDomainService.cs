using Domain;
using Infrastructure.Repository.Core;
using Microsoft.Extensions.Logging;

namespace DomainService;

public interface IAggregaterDomainService 
{
    Task AggregatePlanes(long now);
}

public class AggregaterDomainService : IAggregaterDomainService
{
    private readonly IPlaneHistoryCacheRepository _planeHistoryCache;
    private readonly IPlaneHistoryRepository _planeHistoryRepository;
    
    public AggregaterDomainService(
        IPlaneHistoryCacheRepository planeHistoryCacheRepository,
        IPlaneHistoryRepository planeHistoryRepository
    )
    {
        _planeHistoryCache = planeHistoryCacheRepository;
        _planeHistoryRepository = planeHistoryRepository;
    }

    public async Task AggregatePlanes(long now)
    {
        var offsetTime = now - 60;
        var relevantPlanes = await _planeHistoryCache.GetPlanesInMinute(offsetTime);
        await Task.WhenAll(relevantPlanes.Select(_ => CompilePlane(_, offsetTime)));
    }

    private async Task CompilePlane(string hexValue, long offsetTime)
    {
        var previousLink = await _planeHistoryRepository.GetPlanePointer(hexValue) ?? new();

        var planes = await _planeHistoryCache.GetPlaneMinute(hexValue, offsetTime);

        var filteredPlanes = GetDeduplicatedPlaneMinute(previousLink, planes);

        if(filteredPlanes.Any())
        {
            var data = new PlaneDataRecordLink()
            {
                Time = offsetTime,
                Hex = hexValue,
                PreviousLink = previousLink.Time,
                Planes = filteredPlanes
            };

            await _planeHistoryRepository.StorePlaneHistory(data);
            await _planeHistoryRepository.UpdatePlanePointer(filteredPlanes.Last());
        }
    }

    private IEnumerable<PlaneMinimal> GetDeduplicatedPlaneMinute(PlaneMinimal initial, IEnumerable<PlaneMinimal> planeMinute)
    {
        PlaneMinimal current = initial;

        foreach(var plane in planeMinute)
        {
            if(IsDifferent(current, plane))
            {
                yield return plane;
                current = plane;
            }
        }
    }

    private bool IsDifferent(PlaneMinimal current, PlaneMinimal next)
    {
        return current.Altitude != next.Altitude ||
               current.Latitude != next.Latitude ||
               current.Longitude != next.Longitude;
    }
}
