using System.Security.Cryptography;
using Application.Controllers;
using Application.Messages;
using Domain;
using DomainService;
using Infrastructure.Repository.Core;
using MelbergFramework.ComponentTesting.Rabbit;
using MelbergFramework.Core.ComponentTesting;
using MelbergFramework.Core.Time;
using MelbergFramework.Infrastructure.Rabbit.Extensions;
using MelbergFramework.Infrastructure.Rabbit.Messages;
using MelbergFramework.Infrastructure.Rabbit.Translator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Tests.Features;

public partial class IndexerTests : BaseTestFrame
{
    private PlaneDataRecordLink _response;
    private Dictionary<long, Plane[]> _planes = 
        Enumerable
            .Range(60,121)
            .Select(_ => new Plane[] {new Plane() { Longitude = _, Latitude = 1, Altitude = 2, HexValue = "h"}})
            .ToDictionary(_ => (long)_.First().Longitude.Value);

    public async Task Plane_comes_in_at_(long value)
    {
         var mockTranslator = 
            (MockTranslator<CompletedPlaneFrameMessage>)
            GetClass<IJsonToObjectTranslator<CompletedPlaneFrameMessage>>();

         mockTranslator
            .Messages.Add(new CompletedPlaneFrameMessage()
            {
              Now = value,
               Planes = _planes[value]
            });
        await GetIngressService()
            .ConsumeMessageAsync(new Message(),CancellationToken.None);
    }

    public async Task Index_planes_for_minute_starting_at(long time)
    {
        var mess = new Message();
        mess.SetTimestamp(DateTime.UnixEpoch.AddSeconds(time + 60));

         await GetIndexService().ConsumeMessageAsync(mess, CancellationToken.None);
    }
    
    public async Task Verify_link_is_to_value(long? value)
    {
        Assert.AreEqual(value,_response.PreviousLink);
    }
    
    public async Task Verify_no_planes()
    {
        Assert.IsFalse(_response.Planes.Any());
    }
    public async Task Verify_planes_from_time_are_present(long value)
    {
        foreach(var plane in _response.Planes )
        {
            Assert.AreEqual(ToMinimal(_planes[plane.Time].First(),plane.Time).Altitude,plane.Altitude);
        }
    }
    
    public async Task Verify_last_seen(long time)
    {
        var seen = await GetClass<ILastSeenPointerRepository>().GetLastSeenRecordAsync("h");

        Assert.AreEqual(time, seen.Time);
        Assert.AreEqual(60, seen.Longitude);
        Assert.AreEqual(2, seen.Latitude);
        Assert.AreEqual(3, seen.Altitude);
    }

    public async Task Request(long? time, long currentTime)
    {
        var clock = (MockClock)GetClass<IClock>();
        clock.NewCurrentTime = DateTime.UnixEpoch.AddSeconds(currentTime);
        var controller = new IndexerController(GetClass<IAccessDomainService>(), GetClass<IClock>());
        _response = await controller.GetHistoryForPlane("h",time);
    }

    public async Task Request_second_zero()
    {
        var controller = new IndexerController(GetClass<IAccessDomainService>(), GetClass<IClock>());
        _response = await controller.GetHistoryForPlane("a",0);
    }
    
    private PlaneMinimal ToMinimal(Plane plane, long time) => new PlaneMinimal()
    {
        Altitude = plane.Altitude.Value,
        Longitude = plane.Longitude.Value,
        HexValue = plane.HexValue,
        Latitude = plane.Latitude.Value,
        Time = time
    };
}
