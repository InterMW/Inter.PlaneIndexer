using Domain;

namespace Infrastructure.Repository.Core;

public interface IPlaneHistoryRepository
{
    Task StorePlaneHistory(string hexValue, long minuteInSeconds, PlaneDataRecordLink model);
    Task<PlaneDataRecordLink> GetPlaneHistory(string hexValue, long minuteInSeconds);
}