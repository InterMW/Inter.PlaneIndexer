namespace Infrastructure.Repository.Core;

public interface ILastSeenPointerRepository
{
    Task<long> GetLastSeenTimeAsync(string hexValue);
    Task SetLastSeenTimeAsync(string hexValue, long lastSeenMinute);
}