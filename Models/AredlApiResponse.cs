using System.Text.Json.Serialization;

public class AredlApiResponse
{
    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("nlw_tier")]
    public string? NlwTier { get; set; }

}