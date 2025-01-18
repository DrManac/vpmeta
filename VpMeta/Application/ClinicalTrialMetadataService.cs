using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using VpMeta.Models;
using VpMeta.Repositories;

namespace VpMeta.Application;

public record SelectOptions
{
    public int? Start;
    public int? Limit;
    public ClinicalTrialStatus? Status;
    public DateOnly? StartedBefore;
    public DateOnly? StartedAfter;
    public DateOnly? EndedBefore;
    public DateOnly? EndedAfter;
}

public class DuplicateKeyException: Exception {}

public class ClinicalTrialMetadataService
{
    public ClinicalTrialMetadataService(ClinicalTrialMetadataRepository repository)
    {
        this.repository = repository;
    }
    private readonly ClinicalTrialMetadataRepository repository;

    public async Task Create(JsonNode data)
    {
        var obj = Map(data);
        Transform(obj);
        await repository.AddAsync(obj);
        try
        {
            await repository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new DuplicateKeyException();
        }
    }

    public async Task<IEnumerable<ClinicalTrialMetadata>> Select(SelectOptions selectOptions)
    {
        var query = repository.ClinicalTrials.AsQueryable();
        if (selectOptions.Status.HasValue) query = query.Where(x => x.Status == selectOptions.Status.Value);
        if (selectOptions.StartedAfter.HasValue) query = query.Where(x => x.StartDate >= selectOptions.StartedAfter.Value);
        if (selectOptions.StartedBefore.HasValue) query = query.Where(x => x.StartDate <= selectOptions.StartedBefore.Value);
        if (selectOptions.EndedAfter.HasValue) query = query.Where(x => x.StartDate >= selectOptions.EndedAfter.Value);
        if (selectOptions.EndedBefore.HasValue) query = query.Where(x => x.StartDate <= selectOptions.EndedBefore.Value);
        if (selectOptions.Start.HasValue) query = query.Skip(selectOptions.Start.Value);
        if (selectOptions.Limit.HasValue) query = query.Take(selectOptions.Limit.Value);
        var res = await query.ToListAsync();
        return res;
    }

    public async Task<ClinicalTrialMetadata?> FindById(string id)
    {
        return await repository.ClinicalTrials.FindAsync(id);
    }

    public static ClinicalTrialMetadata Map(JsonNode data)
    {
        return JsonSerializer.Deserialize<ClinicalTrialMetadata>(data, jsonOpts)!;
    }
    private static readonly JsonSerializerOptions jsonOpts = new () {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
    public static void Transform(ClinicalTrialMetadata obj)
    {
        if (!obj.EndDate.HasValue)
        {
            obj.EndDate = obj.StartDate.AddMonths(1);
            obj.Status = ClinicalTrialStatus.Ongoing;
        }
        var tmpTime = new TimeOnly();
        obj.Duration = (obj.EndDate.Value.ToDateTime(tmpTime) - obj.StartDate.ToDateTime(tmpTime)).Days;
    }
}