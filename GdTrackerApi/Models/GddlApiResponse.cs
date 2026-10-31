using System.Text.Json.Serialization;

public class GddlApiResponse
{
    [JsonPropertyName("Rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("Enjoyment")]
    public double? Enjoyment { get; set; }

    [JsonPropertyName("RatingCount")]
    public int? RatingCount { get; set; }

    [JsonPropertyName("Showcase")]
    public string? Showcase { get; set; }

    [JsonPropertyName("Meta")]
    public GddlApiMeta? Meta { get; set; }
}

public class GddlApiMeta
{
    [JsonPropertyName("seconds")]
    public double? Seconds { get; set; }

}