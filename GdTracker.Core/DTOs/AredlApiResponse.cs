using System.Text.Json.Serialization;

namespace GdTracker.Core.DTOs;


public class AredlApiResponse
{
    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("nlw_tier")]
    public string? NlwTier { get; set; }

}