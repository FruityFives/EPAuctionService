using System;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Models;

public class EffectDTO
{
    [JsonPropertyName("storageEffectId")]
    public Guid EffectId { get; set; }
    public string Title { get; set; }
    public decimal AssessmentPrice { get; set; }
    public string ConditionReportUrl { get; set; } = string.Empty;
    public string Picture { get; set; }
    public string Category { get; set; }
    [BsonRepresentation(BsonType.String)]
    public EffectDTOStatus Status { get; set; }
}

public enum EffectDTOStatus
{
    Sold, // Effekt er solgt
    NotSold, // Effekt er ikke solgt
    InAuction // Effekt er i auktion
}



