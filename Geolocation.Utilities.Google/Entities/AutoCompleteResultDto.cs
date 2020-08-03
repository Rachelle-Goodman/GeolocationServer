using Newtonsoft.Json;
using System.Collections.Generic;

namespace Geolocation.Utilities.Google.Entities
{
    public class AutoCompleteResultDto
    {
        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("predictions")]
        public IEnumerable<AutoCompletePredictionsDto> Predictions { get; set; }
    }

    public class AutoCompletePredictionsDto
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("distance_meters")]
        public int DistanceMeters { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("matched_substrings")]
        public IEnumerable<PredictionMathcedSubstringDto> MathcedSubstrings { get; set; }

        [JsonProperty("place_id")]
        public string PlaceId { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("terms")]
        public IEnumerable<PredictionTermsDto> Terms { get; set; }

        [JsonProperty("types")]
        public IEnumerable<string> PredictionTypes { get; set; }
    }

    public class PredictionMathcedSubstringDto
    {
        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }
    }

    public class PredictionTermsDto
    {
        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
