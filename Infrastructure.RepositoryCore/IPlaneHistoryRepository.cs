using Domain;

namespace Infrastructure.Repository.Core;

public interface IPlaneHistoryRepository
{
    Task StorePlaneHistory(PlaneDataRecordLink model);
    Task<PlaneDataRecordLink> GetPlaneHistory(string hexValue, long minuteInSeconds);
    Task<PlaneMinimal> GetPlanePointer(string hexValue);
    Task UpdatePlanePointer(PlaneMinimal plane);
    Task CleanupOldPlaneLinks(long minuteInSeconds);
}
