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

    public Task RecordPlanes(IEnumerable<Plane> planes, long now) =>
        Task.WhenAll(planes.Select(_ => _historyCache.RecordPlane(_, now)));
}
