using Newtonsoft.Json;

namespace GelocationServer.Entities
{
    public class DistanceDto
    {
        [JsonProperty("distance")]
        public double Distance { get; set; }
    }

    public class DistanceDetailsDto: DistanceDto
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("destination")]
        public string Destination { get; set; }
    }
}
