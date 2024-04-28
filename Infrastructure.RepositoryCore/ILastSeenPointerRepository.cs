using Domain;

namespace Infrastructure.Repository.Core;

public interface ILastSeenPointerRepository
{
    Task<PlaneMinimal> GetLastSeenRecordAsync(string hexValue);
    Task SetLastSeenRecordAsync(PlaneMinimal record);
}
