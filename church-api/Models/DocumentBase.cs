using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace church_api.Models;

public abstract class DocumentBase
{
    protected DocumentBase()
    {
        DataType = GetType().Name;
    }

    //public Guid Id { get; set; }

    [JsonProperty("dataType")]
    public string DataType { get; init; }

    //public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.Now;

    //[JsonConverter(typeof(UnixDateTimeConverter))]
    //public DateTimeOffset CreatedTimestamp { get; set; } = DateTimeOffset.UtcNow;

    //public DateTimeOffset DateModified { get; set; } = DateTimeOffset.Now;

    //[JsonConverter(typeof(UnixDateTimeConverter))]
    //public DateTimeOffset ModifiedTimestamp { get; set; } = DateTimeOffset.UtcNow;
}