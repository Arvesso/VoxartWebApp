using Newtonsoft.Json.Linq;
using RestSharp;
using VoxartWebApp.Shared.Modules.Synthesize;

namespace VoxartWebApp.Server.Services
{
    public class NeuralAvatarApiService // Service providing API calls to speech convertat
    {
        private const string ApiCreateTalkEndpoint = "https://api.d-id.com/talks";
        private const string ApiGetTalkEndpoint = "https://api.d-id.com/talks/{id}";
        private const string Token = "Basic WVcxbGJHVnVhMjlrUUdkdFlXbHNMbU52YlE6RFdOWmViZ2Y5QWRuUUJwOUNxNXNw";

        private static readonly Dictionary<SynthesizeGender, string> SpeechStyleCode = new()
        {
            { SynthesizeGender.Male, "ru-RU-DmitryNeural" },
            { SynthesizeGender.Female, "ru-RU-SvetlanaNeural" }
        };

        private readonly ILogger _logger;
        public NeuralAvatarApiService(ILogger<NeuralAvatarApiService> logger)
        {
            _logger = logger;
        }

        public async Task<ResponseModel?> GetResultAsync(string text, string avatar, SynthesizeGender synthesizeGender)
        {
            var options = new RestClientOptions(ApiCreateTalkEndpoint);
            var client = new RestClient(options);
            var request = new RestRequest("");

            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", Token);
            request.AddJsonBody($"{{\"script\":{{\"type\":\"text\",\"subtitles\":\"false\",\"provider\":{{\"type\":\"microsoft\",\"voice_id\":\"{SpeechStyleCode[synthesizeGender]}\"}},\"ssml\":false,\"input\":\"{text}\"}},\"source_url\":\"{avatar}\",\"persist\":false}}", false);

            try
            {
                var response = await client.PostAsync(request);

                if (string.IsNullOrEmpty(response.Content))
                {
                    return default;
                }
                else
                {
                    dynamic content = JObject.Parse(response.Content);
                    var id = (string)content.id;

                    if (string.IsNullOrEmpty(id))
                        return default;

                    options.BaseUrl = new(ApiGetTalkEndpoint.Replace("{id}", id));

                    client = new RestClient(options);
                    request = new();

                    request.AddHeader("accept", "application/json");
                    request.AddHeader("authorization", Token);

                    await Task.Delay(TimeSpan.FromSeconds(5)); // TODO TO FIX

                    response = await client.GetAsync(request);

                    if (string.IsNullOrEmpty(response.Content))
                    {
                        Console.WriteLine("Second");
                        return default;
                    }

                    content = JObject.Parse(response.Content);

                    var audio = (string)content.audio_url;
                    var video = (string)content.result_url;

                    if (string.IsNullOrEmpty(audio) || string.IsNullOrEmpty(video))
                    {
                        return default;
                    }

                    return new() 
                    { 
                        AudioUri = audio,
                        ResultUri = video
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("An error has occured while processing requests {0}", ex.Message);
                return default;
            }
        }
    }
}
