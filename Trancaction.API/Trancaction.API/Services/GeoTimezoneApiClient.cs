using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Transaction.API.Services
{
    public class GeoTimezoneApiClient
    {
        private readonly HttpClient _httpClient;

        public GeoTimezoneApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.geotimezone.com/public/timezone");
        }

        public async Task<string> GetIanaTimezone(double latitude, double longitude)
        {
            var apiUrl = $"?latitude={latitude}&longitude={longitude}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to call API. Status code: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();

            return GetTimeZone(content);
        }

        private string GetTimeZone(string location) 
        {

            var responseObject = JsonConvert.DeserializeObject<JObject>(location);

            var ianaTimezone = responseObject.Value<string>("offset");

            return ianaTimezone;
        }
    }
}
