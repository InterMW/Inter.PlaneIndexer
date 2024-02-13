namespace Domain;

public class PlaneDataRecordLink
{
    public IEnumerable<TimestampedPlaneRecord> Planes {get; set;} = Array.Empty<TimestampedPlaneRecord>();
    public long? PreviousLink {get; set;}
}