using Newtonsoft.Json;

namespace church_api.Controllers
{
    public class PrayerCard : DocumentBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
    }
}