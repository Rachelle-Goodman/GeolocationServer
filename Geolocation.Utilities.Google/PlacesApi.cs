using Geolocation.Utilities.Google.Entities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Geolocation.Utilities.Google
{
    public static class PlacesApi
    {
        private static readonly HttpClient _client;
        private static readonly JsonSerializer _jsonSerializer;
        private const string _baseUrl = "https://maps.googleapis.com/maps/api/place";

        static PlacesApi()
        {
            _client = new HttpClient();
            _jsonSerializer = new JsonSerializer();
        }

        public static async Task<AutoCompleteResultDto> AutoComplete(string input)
        {
            string url = $"{_baseUrl}/autocomplete/{GoogleUtil.output}?key={GoogleUtil.googleApiKey}&input={input}&language=en";
            HttpResponseMessage response = await _client.GetAsync(url);
            string jsonString = await response.Content.ReadAsStringAsync();
            return _jsonSerializer.Deserialize<AutoCompleteResultDto>(new JsonTextReader(new StringReader(jsonString)));
        }
    }
}
