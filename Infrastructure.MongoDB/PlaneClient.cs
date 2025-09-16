using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Infrastructure.MongoDB;

public class PlaneClient 
{
    private readonly MongoClient _client;

    public PlaneClient(IOptions<MongoDBOptions> options)
    {
        _client = new MongoClient(options.Value.LongTermPlaneHistory);

        var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);

        try
        {
            _client.GetDatabase("plane").CreateCollectionAsync("standard").Wait();
            _client.GetDatabase("plane").CreateCollectionAsync("pointer").Wait();
        }
        catch (System.Exception)
        {
        }
        
    }
    public MongoClient GetClient() => _client;
}
