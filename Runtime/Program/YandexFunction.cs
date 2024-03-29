using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmsAuthLibrary.DTO;

namespace SmsAuthLibrary.Program
{
    internal class YandexFunction
    {
        private readonly string _functionId;
        private readonly HttpClient _client;

        public YandexFunction(string finctionId)
        {
            _functionId = finctionId;
            _client = new HttpClient();
        }

        public void Authorize(string authToken)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }

        public async Task<Response> Post(Request request)
        {
            var body = new StringContent(JsonConvert.SerializeObject(request));
            var response = await _client.PostAsync($"https://functions.yandexcloud.net/{_functionId}?integration=raw", body);

            var responseString = await response.Content.ReadAsStringAsync();

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException exception)
            {
                throw new HttpRequestException($"Response: {responseString}", exception);
            }

            return JsonConvert.DeserializeObject<Response>(responseString);
        }
    }
}
