using System.ComponentModel.DataAnnotations;

namespace Infrastructure.MongoDB;

public class MongoDBOptions
{
    public const string Section = "MongoDB";

    [Required]
    public string LongTermPlaneHistory {get; set;} = "";
}
