using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using VpMeta.Application;
using VpMeta.Repositories;

namespace VpMeta.Tests.Integration;

public class ClinicalTrialMetadataServiceTest
{
    public ClinicalTrialMetadataServiceTest()
    {
        var dbbuilder = new DbContextOptionsBuilder<ClinicalTrialMetadataRepository>();
        dbbuilder.UseInMemoryDatabase("Test");
        var repo = new ClinicalTrialMetadataRepository(dbbuilder.Options);
        service = new ClinicalTrialMetadataService(repo);
    }
    readonly ClinicalTrialMetadataService service;

    [Fact]
    public async Task ShouldSaveData()
    {
        var json = JsonNode.Parse("""
        {
            "trialId": "test",
            "title": "test",
            "startDate": "2025-01-13",
            "status": "Not Started"
        }
        """) ?? throw new Exception("Should not happen with not 'null' string");
        await service.Create(json);

        var saved = await service.FindById("test");
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task ShouldThrowOnDuplicate()
    {
        var json = JsonNode.Parse("""
        {
            "trialId": "test1",
            "title": "test",
            "startDate": "2025-01-13",
            "status": "Not Started"
        }
        """) ?? throw new Exception("Should not happen with not 'null' string");
        await service.Create(json);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Create(json));
    }


    [Fact]
    public async Task ShouldFindByStatus()
    {
        var json = JsonNode.Parse("""
        {
            "trialId": "testfind",
            "title": "test",
            "startDate": "2024-01-13",
            "endDate": "2025-01-13",
            "status": "Completed"
        }
        """) ?? throw new Exception("Should not happen with not 'null' string");
        await service.Create(json);
        var res = await service.Select(new() { Status = Models.ClinicalTrialStatus.Completed });
        Assert.Single(res);
    }

    [Fact]
    public async Task ShouldFilterByDate()
    {
        var json = JsonNode.Parse("""
        {
            "trialId": "testfilter",
            "title": "test",
            "startDate": "1024-01-13",
            "status": "Not Started"
        }
        """) ?? throw new Exception("Should not happen with not 'null' string");
        await service.Create(json);
        var res = await service.Select(new() { StartedBefore = new DateOnly(2000, 1, 1) });
        Assert.Single(res);
    }

    [Fact]
    public async Task ShouldLimitResults()
    {
        for (int i = 0; i < 10; i++)
        {
            var json = JsonNode.Parse("""
                {
                    "trialId": "",
                    "title": "test",
                    "startDate": "2024-01-13",
                    "status": "Not Started"
                }
                """) ?? throw new Exception("Should not happen with not 'null' string");
            json["trialId"] = "testlimit" + i;
            await service.Create(json);
        }
        var res = await service.Select(new() { Start = 2, Limit = 3 });
        Assert.Equal(3, res.Count());
    }
}