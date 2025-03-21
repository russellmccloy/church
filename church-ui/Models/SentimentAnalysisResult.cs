using System.Text.Json.Serialization;

namespace Controllers;

public class SentimentAnalysisResult
{
    [JsonPropertyName("sentiment")]
    public required string Sentiment { get; set; }

    [JsonPropertyName("prayerSuggestion")]
    public required string PrayerSuggestion { get; set; }

    [JsonPropertyName("reasoning")]
    public required string Reasoning { get; set; }
}