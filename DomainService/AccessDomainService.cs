using Common;
using Domain;
using Infrastructure.Repository.Core;

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

    public AccessDomainService(
        IPlaneHistoryCacheRepository planeCacheRepository,
        IPlaneHistoryRepository planeHistoryRepository,
        ILastSeenPointerRepository lastSeenPointerRepository)
    {
        _planeCacheRepository = planeCacheRepository;
        _lastSeenPointerRepository = lastSeenPointerRepository;
        _planeHistoryRepository = planeHistoryRepository;
    }

    public async Task<PlaneDataRecordLink> RetrievePlaneHistory(string hexValue, long time)
    {
        if (time == 0) { return new(); }//Nothing to find
        var now = DateTime.UtcNow;
        var lastMin = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

        var lastMinuteSecond = (long)(lastMin - DateTime.UnixEpoch).TotalSeconds;
        var lastMinuteSecondBefore = lastMinuteSecond - 60;

        //Case 1: Too early for couchbase
        if (time > lastMinuteSecond)
        {
            return await EarlyCase(hexValue,lastMinuteSecond);
        }

        //Case 2: Maybe too early for couchbase
        if (time == lastMinuteSecondBefore)
        {
            return await RaceCase(hexValue,time);
        }

        return await _planeHistoryRepository.GetPlaneHistory(hexValue,time.ToLastMinInSec());
    }
    //First case is early, we get the plane data and find out from redis 
    private async Task<PlaneDataRecordLink> EarlyCase(string hexValue, long lastMinuteSecond)
    {
        long lastMinuteSecondBefore = lastMinuteSecond - 60;
        var planesFromLastMinute = await _planeCacheRepository.GetPlanesInMinute(lastMinuteSecondBefore);
        long lastSeen = 0;

        if (planesFromLastMinute.Any(_ => _ == hexValue))
        {
            lastSeen = lastMinuteSecondBefore;
        }
        else
        {
            lastSeen = await _lastSeenPointerRepository.GetLastSeenTimeAsync(hexValue);
        }

        return new()
        {
            Planes = await _planeCacheRepository.GetPlaneMinute(hexValue, lastMinuteSecond),
            PreviousLink = lastSeen
        };
    }

    private async Task<PlaneDataRecordLink> RaceCase(string hexValue, long lastMinuteSecond)
    {
        long lastSeen = await _lastSeenPointerRepository.GetLastSeenTimeAsync(hexValue);

        if (lastSeen == lastMinuteSecond) // We already have the info
        {
            return await _planeHistoryRepository.GetPlaneHistory(hexValue,lastMinuteSecond);
        }
        
        //We don't have the info, check redis
        long lastMinuteSecondBefore = lastMinuteSecond - 60;

        var planesFromLastMinute = await _planeCacheRepository.GetPlanesInMinute(lastMinuteSecondBefore);
        
        if(planesFromLastMinute.Any(_ => _ == hexValue))
        {
            return new()
            {
                Planes = await _planeCacheRepository.GetPlaneMinute(hexValue, lastMinuteSecond),
                PreviousLink = lastMinuteSecondBefore
            };
        }
        
        return new();//We don't have anything
    }
}