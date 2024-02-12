namespace Domain;

public class TimestampedPlaneRecord
{
    public long Timestamp {get; set;}
    public Plane Data {get; set;} = new();
}