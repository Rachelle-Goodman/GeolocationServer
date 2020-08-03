using Newtonsoft.Json;

namespace GelocationServer.Entities
{
    public class DistanceDto
    {
        [JsonProperty("distance")]
        public double Distance { get; set; }
    }
}
