using Domain;

namespace Infrastructure.Repository.Core;

public interface IPlaneHistoryCacheRepository
{
    Task RecordPlane(PlaneMinimal plane, long min);
    Task<IEnumerable<string>> GetPlanesInMinute(long min);
    Task<IEnumerable<PlaneMinimal>> GetPlaneMinute(string hexValue, long min);
}