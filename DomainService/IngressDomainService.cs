using Common;
using Domain;
using Infrastructure.Repository.Core;
namespace DomainService;

public interface IIngressDomainService
{
    Task RecordPlanes(IEnumerable<Plane> planes, long now);
}

public class IngressDomainService : IIngressDomainService
{
    private readonly IPlaneHistoryCacheRepository _historyCache;
    public IngressDomainService(IPlaneHistoryCacheRepository cache)
    {
        _historyCache = cache;
    }
    public Task RecordPlanes(IEnumerable<Plane> planes, long now)
    {
        var lastMin = now.ToLastMinInSec();

        return Task.WhenAll(planes.Select(_ => _historyCache.RecordPlane(ToMinimal(_, now), lastMin)));
    } 
    
    private static PlaneMinimal ToMinimal(Plane plane, long now)
    {
        return new()
        {
            Time = now,
            HexValue = plane.HexValue,
            Altitude = plane.Altitude ?? 0,
            Latitude = plane.Latitude ?? 0,
            Longitude = plane.Longitude ?? 0
        };

    }
}
