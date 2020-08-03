using Newtonsoft.Json;
using System.Collections.Generic;

namespace Geolocation.Utilities.Google.Entities
{
    public class DistanceMatrixResponseDto
    {
        [JsonProperty("rows")]
        public IEnumerable<DistanceRowDto> Rows { get; set; }
    }

    public class DistanceRowDto
    {
        [JsonProperty("elements")]
        public IEnumerable<DistanceRowElementDto> Elements { get; set; }
    }

    public class DistanceRowElementDto
    {
        [JsonProperty("distance")]
        public DistanceDetailsDto Distance { get; set; }
    }

    public class DistanceDetailsDto
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }
    }
}
