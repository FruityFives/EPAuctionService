using System;

namespace Models;
public class EffectDTO
{
    public Guid EffectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal AssessmentPrice { get; set; }
    public string ConditionReport { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public EffectDTOStatus Status { get; set; }// Denne kommer fra storage service. Når vi produktet er solgt sender vi en besked til storage swervice, som fortæller den at den skal ændre status
}

public enum EffectDTOStatus
{
    Available, // Effekt er tilgængelig for bud
    Sold, // Effekt er solgt
    NotAvailable // Effekt er ikke længere tilgængelig for bud
}



