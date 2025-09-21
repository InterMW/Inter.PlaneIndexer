using Domain;
using Infrastructure.Repository.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Infrastructure.MongoDB;

public class LongTermPlaneHistoryRepository(PlaneClient client) : IPlaneHistoryRepository
{
    private readonly IMongoCollection<PlaneDataRecordLinkModel> _standardCollection
        = client.GetClient().GetDatabase("plane").GetCollection<PlaneDataRecordLinkModel>("standard");

    private readonly IMongoCollection<PlaneMinimalModel> _pointerCollection
        = client.GetClient().GetDatabase("plane").GetCollection<PlaneMinimalModel>("pointer");

    public async Task<PlaneMinimal> GetPlanePointer(string hexValue)
    {
        var result = await _pointerCollection.FindAsync(minimal => minimal.HexValue == hexValue);
        var model =await result.FirstOrDefaultAsync();
        return model?.ToDomain() ?? new() {HexValue = hexValue, Time = 0};
    }

    public async Task UpdatePlanePointer(PlaneMinimal plane) 
    {
        var currentPointer = await _pointerCollection.FindAsync(Builders<PlaneMinimalModel>.Filter.Eq("hexValue",plane.HexValue));

        if (await currentPointer.FirstOrDefaultAsync() is null)
        {
            await _pointerCollection.InsertOneAsync(plane.ToModel());
        }
        else
        {
            await _pointerCollection.ReplaceOneAsync(
                Builders<PlaneMinimalModel>.Filter.Eq("hexValue",plane.HexValue),
                plane.ToModel()
                );
        }
    }

    public async Task<PlaneDataRecordLink> GetPlaneHistory(string hexValue, long minuteInSeconds) 
    {
        var j = await _standardCollection.Find(link => link.Hex == hexValue && link.Time == minuteInSeconds).FirstOrDefaultAsync();
        return new(){Planes = j.Planes, Time = j.Time, Hex = j.Hex, PreviousLink = j.PreviousLink};
    }

    public async Task StorePlaneHistory(PlaneDataRecordLink model) 
    {
        await _standardCollection.InsertOneAsync(new() {Hex = model.Hex, Time = model.Time, Planes = model.Planes} );
    }

    public async Task CleanupOldPlaneLinks(long minuteInSeconds)
    {
        await _standardCollection.DeleteManyAsync(
                Builders<PlaneDataRecordLinkModel>.Filter.Lte("time", minuteInSeconds)
                );
    }
}

public class PlaneDataRecordLinkModel
{
    [BsonIgnoreIfDefault]
    public ObjectId Id { get; set; }
    public string Hex = "";
    public long Time;
    public IEnumerable<PlaneMinimal> Planes {get; set;} = Array.Empty<PlaneMinimal>();
    public long? PreviousLink {get; set;} = 0;
}


public static class PlaneMinimalModelMapper
{
    public static PlaneMinimal ToDomain(this PlaneMinimalModel source) => new ()
    {
        Time = source.Time,
        HexValue = source.HexValue,
        Latitude = source.Latitude,
        Longitude = source.Longitude,
        Altitude = source.Altitude
    };

    public static PlaneMinimalModel ToModel(this PlaneMinimal source) => new ()
    {
        Time = source.Time,
        HexValue = source.HexValue,
        Latitude = source.Latitude,
        Longitude = source.Longitude,
        Altitude = source.Altitude
    };

}

public class PlaneMinimalModel
{
    [BsonIgnoreIfDefault]
    public ObjectId Id { get; set; }
    public long Time {get; set;} = 0;
    public string HexValue {get; set;} = string.Empty;
    public float Latitude {get; set;} = 0;
    public float Longitude {get; set;} = 0;
    public float Altitude {get; set;} = 0;
}
