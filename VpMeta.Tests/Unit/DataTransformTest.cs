using System.Text.Json.Nodes;
using VpMeta.Application;
using VpMeta.Models;

namespace VpMeta.Tests.Unit;

public class DataTransformTest
{
    public DataTransformTest()
    {
        json = JsonNode.Parse("""
        {
            "trialId": "test",
            "title": "test",
            "startDate": "2025-01-13",
            "status": "Not Started"
        }
        """) ?? throw new Exception("Should not happen with not 'null' string");
    }
    readonly JsonNode json;

    [Fact]
    public void MapTest()
    {
        var res = ClinicalTrialMetadataService.Map(json);
        Assert.IsType<ClinicalTrialMetadata>(res);
    }

    [Fact]
    public void TransformTest()
    {
        var md = new ClinicalTrialMetadata
        {
            TrialId = "test",
            Title = "test",
            StartDate = new DateOnly(2025, 2, 1),
            Status = ClinicalTrialStatus.Completed
        };
        ClinicalTrialMetadataService.Transform(md);
        Assert.Equal(28, md.Duration);
        Assert.Equal(ClinicalTrialStatus.Ongoing, md.Status);
        Assert.True(md.EndDate.HasValue);
        Assert.Equal(md.EndDate, md.StartDate.AddMonths(1));
    }
}