using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace VpMeta.Models;

[JsonConverter(typeof(JsonStringEnumConverterWithAttributeSupport))]
public enum ClinicalTrialStatus {
    [EnumMember(Value = "Not Started")]
    NotStarted,
    Ongoing,
    Completed
}

public class ClinicalTrialMetadata
{
    [Key]
    public required string TrialId { get; set; }
    public required string Title { get; set; }
    public required DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [Range(1, int.MaxValue)]
    public int? Participants { get; set; }
    public required ClinicalTrialStatus Status { get; set; }
    public int Duration { get; set; }
}