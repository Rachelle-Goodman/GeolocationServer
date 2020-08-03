using Geolocation.Utilities.Google.Entities;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Geolocation.Utilities.Google
{
    public static class DistanceMatrixApi
    {
        private static readonly HttpClient _client;
        private static readonly JsonSerializer _jsonSerializer;
        private const string _baseUrl = "https://maps.googleapis.com/maps/api/distancematrix";

        static DistanceMatrixApi()
        {
            _client = new HttpClient();
            _jsonSerializer = new JsonSerializer();
        }

        public static async Task<DistanceMatrixResponseDto> Distance(string sourcePlaceId, string destinationPlaceId)
        {
            string placeIdPrefix = "place_id:";
            string url = $"{_baseUrl}/{GoogleUtil.output}?key={GoogleUtil.googleApiKey}&origins={placeIdPrefix}{sourcePlaceId}&destinations={placeIdPrefix}{destinationPlaceId}&language=en";
            HttpResponseMessage response = await _client.GetAsync(url);
            string jsonString = await response.Content.ReadAsStringAsync();
            return _jsonSerializer.Deserialize<DistanceMatrixResponseDto>(new JsonTextReader(new StringReader(jsonString)));
        }
    }
}
