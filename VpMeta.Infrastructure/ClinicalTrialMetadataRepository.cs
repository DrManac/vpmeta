using Microsoft.EntityFrameworkCore;
using VpMeta.Models;

namespace VpMeta.Repositories;
public class ClinicalTrialMetadataRepository : DbContext
{
    public ClinicalTrialMetadataRepository(DbContextOptions<ClinicalTrialMetadataRepository> options)
    : base(options)
    {
    }

    public DbSet<ClinicalTrialMetadata> ClinicalTrials { get; set; } = null!;
}