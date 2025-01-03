using Newtonsoft.Json;

namespace church_api.Models
{
    public class PrayerCard : DocumentBase
    {
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonProperty("userId")]
        public required Guid UserId { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("desc")]
        public required string Desc { get; set; }

        [JsonProperty("imagePath")]
        public required string ImagePath { get; set; }
    }
}