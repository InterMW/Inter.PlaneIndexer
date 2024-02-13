using System.ComponentModel.DataAnnotations;

namespace Common;

public class TimingsOptions
{
    public const string Timing = "Timing";
    
    [Required]
    [Range(75,double.MaxValue)]//it doesn't make sense to have the value be less than 2 min + wiggle room
    public int PlaneDocLifetimesSecs {get; set;}
    
}