using Application.Pillars;
using LightBDD.Framework.Scenarios;
using LightBDD.MsTest3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Features;

[TestClass]
public partial class IndexerTests : BaseTestFrame
{
    [Scenario]
    [TestMethod]
    public async Task Request_for_epoch_is_empty()
    {
        await Runner.RunScenarioAsync(
            _ => Request_second_zero(),
            _ => Response_is_empty()
        );
    }

    // at 65, get the last 5 seconds
    [Scenario]
    [TestMethod]
    public async Task Request_for_early_time()
    {
        await Runner.RunScenarioAsync(
            _ => Plane_comes_in_at_(120),
            _ => Request(125, 121),
            _ => Verify_link_is_to_value(0)
                );
    }

    [Scenario]
    [TestMethod]
    public async Task Request_for_race_condition_when_processing_not_yet_no_history()
    {
        await Runner.RunScenarioAsync(
            _ => Plane_comes_in_at_(120),
            _ => Index_planes_for_minute_starting_at(120),
            _ => Request(120,180),
            _ => Verify_link_is_to_value(null)
                );
    }
    
    [Scenario]
    [TestMethod]
    public async Task Request_for_race_condition_when_processing_with_history()
    {
        await Runner.RunScenarioAsync(
            _ => Plane_comes_in_at_(60),
            _ => Plane_comes_in_at_(120),
            _ => ConsumeIngressMessage(),
            _ => Index_planes_for_minute_starting_at(60),
            _ => ConsumeIngressMessage(),
            _ => Index_planes_for_minute_starting_at(120), 
            _ => Request(120,180),
            _ => Verify_link_is_to_value(60),
            _ => Verify_planes_from_time_are_present(120)
        );
    }

    [Scenario]
    [TestMethod]
    public async Task Request_for_general_condition_history()
    {
        await Runner.RunScenarioAsync(
            _ => Plane_comes_in_at_(60),
            _ => ConsumeIngressMessage(),
            _ => Index_planes_for_minute_starting_at(60),
            _ => Request(60,180),
            _ => Verify_link_is_to_value(null),
            _ => Verify_planes_from_time_are_present(60)
        );
    }
}
