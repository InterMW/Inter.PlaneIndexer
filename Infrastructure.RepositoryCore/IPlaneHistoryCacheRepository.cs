using Domain;

namespace Infrastructure.Repository.Core;

public interface IPlaneHistoryCacheRepository
{
    Task RecordPlane(Plane plane, long now);
    IAsyncEnumerable<TimestampedPlaneRecord> GetPlanesInRange(long start, long end, string hexValue = "*");
}