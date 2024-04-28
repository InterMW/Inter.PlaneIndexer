using Common;
using Domain;
using Infrastructure.Repository.Core;
using MelbergFramework.Core.Time;

namespace DomainService;

public interface IAccessDomainService
{
    Task<PlaneDataRecordLink> RetrievePlaneHistory(string hexValue, long time);
}

public class AccessDomainService : IAccessDomainService
{
    private readonly IPlaneHistoryCacheRepository _planeCacheRepository;
    private readonly IPlaneHistoryRepository _planeHistoryRepository;
    private readonly ILastSeenPointerRepository _lastSeenPointerRepository;
    private readonly IClock _clock;

    public AccessDomainService(
        IPlaneHistoryCacheRepository planeCacheRepository,
        IPlaneHistoryRepository planeHistoryRepository,
        ILastSeenPointerRepository lastSeenPointerRepository,
        IClock clock)
    {
        _planeCacheRepository = planeCacheRepository;
        _lastSeenPointerRepository = lastSeenPointerRepository;
        _planeHistoryRepository = planeHistoryRepository;
        _clock = clock;
    }

    public async Task<PlaneDataRecordLink> RetrievePlaneHistory(string hexValue, long time)
    {
        if (time == 0) 
        { 
            return new();
        }

        var now = _clock.GetUtcNow();
        var lastMin = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

        var lastMinuteSecond = (long)(lastMin - DateTime.UnixEpoch).TotalSeconds;

        //Case 1: Too early for couchbase, we aren't yet scheduled to process the minute
        if (time >= lastMinuteSecond)
        {
            return await EarlyCase(hexValue,lastMinuteSecond);
        }
        
        //Case 2: We might be too early for couchbase
        if (time >= lastMinuteSecond - 60)
        {
            return await RaceCase(hexValue,lastMinuteSecond - 60);
        }

        return await _planeHistoryRepository.GetPlaneHistory(hexValue,time.ToLastMinInSec());
    }
    //First case is early, we get the plane data and find out from redis 
    private async Task<PlaneDataRecordLink> EarlyCase(string hexValue, long lastMinuteSecond)
    {
        long lastMinuteSecondBefore = lastMinuteSecond - 60;
        //check if the previous minute has these planes
        var planesFromLastMinute = await _planeCacheRepository.GetPlanesInMinute(lastMinuteSecondBefore);
        var planesImmediatelyBefore = planesFromLastMinute.Any(_ => _ == hexValue);
        long lastSeen = planesImmediatelyBefore ?
            lastMinuteSecondBefore :
            (await _lastSeenPointerRepository.GetLastSeenRecordAsync(hexValue)).Time;

        return new()
        {
            Planes = await _planeCacheRepository.GetPlaneMinute(hexValue, lastMinuteSecond),
            PreviousLink = lastSeen
        };
    }

    private async Task<PlaneDataRecordLink> RaceCase(string hexValue, long lastMinuteSecond)
    {
        long lastSeen = (await _lastSeenPointerRepository.GetLastSeenRecordAsync(hexValue)).Time;

        if (lastSeen == lastMinuteSecond) // We already have the info
        {
            return await _planeHistoryRepository.GetPlaneHistory(hexValue,lastMinuteSecond);
        }
        
        //We don't have the info, check redis
        long lastMinuteSecondBefore = lastMinuteSecond - 60;

        var planesFromLastMinute = await _planeCacheRepository.GetPlanesInMinute(lastMinuteSecondBefore);

        return new()
        {
            Planes = await _planeCacheRepository.GetPlaneMinute(hexValue, lastMinuteSecond), 
            PreviousLink = planesFromLastMinute.Any(_ => _ == hexValue) ? lastMinuteSecondBefore : lastSeen
        };
    }
}
