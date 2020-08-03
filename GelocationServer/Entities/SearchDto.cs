using Newtonsoft.Json;

namespace GelocationServer.Entities
{
    public class SearchDto
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("destination")]
        public string Destination { get; set; }

        [JsonProperty("hits")]
        public int Hits { get; set; }
    }
}
