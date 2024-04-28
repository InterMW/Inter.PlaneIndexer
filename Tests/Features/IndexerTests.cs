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
    public async Task Request_for_right_now()
    {
        await Runner.RunScenarioAsync(
            _ => Plane_comes_in_at_(60),
            _ => Plane_comes_in_at_(120),
            _ => Index_planes_for_minute_starting_at(60),
            _ => Request(null,121),
            _ => Verify_link_is_to_value(60),
            _ => Verify_planes_from_time_are_present(120)
        );
    }

    [Scenario]
    [TestMethod]
    public async Task Request_for_minute_past_early()
    {
        await Runner.RunScenarioAsync(
            _ => Plane_comes_in_at_(60),
            _ => Plane_comes_in_at_(120),
            _ => Request(60,121),
            _ => Verify_link_is_to_value(0),
            _ => Verify_planes_from_time_are_present(60)
        );
    }

    [Scenario]
    [TestMethod]
    public async Task Request_for_minute_past_after_process()
    {
        await Runner.RunScenarioAsync(
            _ => Plane_comes_in_at_(60),
            _ => Plane_comes_in_at_(120),
            _ => Index_planes_for_minute_starting_at(60),
            _ => Request(60,121),
            _ => Verify_link_is_to_value(0),
            _ => Verify_planes_from_time_are_present(60)
        );
    }

    [Scenario]
    [TestMethod]
    public async Task Request_for_one_minute_ago()
    {
        await Runner.RunScenarioAsync(
            _ => Plane_comes_in_at_(60),
            _ => Index_planes_for_minute_starting_at(60),
            _ => Plane_comes_in_at_(120),
            _ => Index_planes_for_minute_starting_at(120), 
            _ => Request(120,180),
            _ => Verify_link_is_to_value(60),
            _ => Verify_planes_from_time_are_present(120)
        );
    }
    
    
    [Scenario]
    [TestMethod]
    public async Task Request_for_epoch_is_empty()
    {
        await Runner.RunScenarioAsync(
            _ => Request_second_zero(),
            _ => Verify_no_planes(),
            _ => Verify_link_is_to_value(0)
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
            _ => Verify_link_is_to_value(0)
                );
    }
    
    [Scenario]
    [TestMethod]
    public async Task Request_for_race_condition_when_processing_with_history()
    {
        await Runner.RunScenarioAsync(
            _ => Plane_comes_in_at_(60),
            _ => Index_planes_for_minute_starting_at(60),
            _ => Plane_comes_in_at_(120),
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
            _ => Index_planes_for_minute_starting_at(60),
            _ => Request(60,180),
            _ => Verify_link_is_to_value(0),
            _ => Verify_planes_from_time_are_present(60)
        );
    }

    [Scenario]
    [TestMethod]
    public async Task Request_without_time()
    {
        await Runner.RunScenarioAsync(
            _ => Plane_comes_in_at_(60),
            _ => Index_planes_for_minute_starting_at(60),
            _ => Plane_comes_in_at_(120),
            _ => Index_planes_for_minute_starting_at(120),
            _ => Request(null,180),
            _ => Verify_link_is_to_value(120),
            _ => Verify_planes_from_time_are_present(60)
        );
    }

    [Scenario]
    [TestMethod]
    public async Task Request_without_time_too_late()
    {
        await Runner.RunScenarioAsync(
            _ => Plane_comes_in_at_(60),
            _ => Index_planes_for_minute_starting_at(60),
            _ => Plane_comes_in_at_(120),
            _ => Index_planes_for_minute_starting_at(120),
            _ => Request(null,181),
            _ => Verify_link_is_to_value(120),
            _ => Verify_no_planes()
        );
    }

    [Scenario]
    [TestMethod]
    public async Task Request_without_time_too_late_long()
    {
        await Runner.RunScenarioAsync(
            _ => Plane_comes_in_at_(60),
            _ => Index_planes_for_minute_starting_at(60),
            _ => Plane_comes_in_at_(120),
            _ => Index_planes_for_minute_starting_at(120),
            _ => Request(null,300),
            _ => Verify_link_is_to_value(120),
            _ => Verify_no_planes()
        );
    }
}
