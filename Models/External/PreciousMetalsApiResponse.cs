using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.External;

public class PreciousMetalsApiResponse
{
    [JsonPropertyName("success")]
    public bool Succes { get; set; }
    [JsonPropertyName("base")]
    public required string Base { get; set; }
    [JsonPropertyName("rates")]
    public required JsonNode Rates { get; set; }
}
