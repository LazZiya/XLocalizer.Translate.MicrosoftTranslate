using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using XLocalizer.Translate.MicrosoftTranslate;

namespace XLocalizer.Translate.SystranTranslate
{
    /// <summary>
    /// Microsoft Translate service over RapidApi.
    /// for subscription visit <a href="https://rapidapi.com/microsoft-azure-org-microsoft-cognitive-services/api/microsoft-translator-text/endpoints">Microsoft Translator Text</a>
    /// </summary>
    public class MicrosoftTranslateServiceRapidApi : ITranslator
    {
        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName => "Microsoft Translator RapidApi";

        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        /// <summary>
        /// Initialize Microsoft translate service, for subscription visit <a href="https://rapidapi.com/microsoft-azure-org-microsoft-cognitive-services/api/microsoft-translator-text/endpoints">Microsoft Translator Text</a>
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public MicrosoftTranslateServiceRapidApi(HttpClient httpClient, IConfiguration configuration, ILogger<MicrosoftTranslateServiceRapidApi> logger)
        {
            _httpClient = httpClient ?? throw new NullReferenceException(nameof(httpClient));
            
            var _rapidApiKey = configuration["XLocalizer.Translate:RapidApiKey"] ?? throw new NullReferenceException("Configuration key for RapidApi was not found! For more details see https://docs.ziyad.info/en/XLocalizer/v1.0/translate-services-microsoft.md");

            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", _rapidApiKey);
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "microsoft-translator-text.p.rapidapi.com");
            
            _logger = logger;
        }

        /// <summary>
        /// Run async translation task
        /// </summary>
        /// <param name="source">Source language e.g. en</param>
        /// <param name="target">Target language e.g. tr</param>
        /// <param name="text">Text to be translated</param>
        /// <param name="format">Text format: html or text</param>
        /// <returns><see cref="TranslationResult"/></returns>
        public async Task<TranslationResult> TranslateAsync(string source, string target, string text, string format)
        {
            var textArray = new MicrosoftTranslateRequestText[]
            {
                new MicrosoftTranslateRequestText{ Text = text }
            };

            var jsonTextArray = JsonConvert.SerializeObject(textArray);

            var content = new StringContent(jsonTextArray)
            {
                Headers = {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
            };

            var ttype = format == "text" ? "plain" : "html";

            try
            {
                var response = await _httpClient.PostAsync($"https://microsoft-translator-text.p.rapidapi.com/translate?to={target}&api-version=3.0&from={source}&profanityAction=NoAction&textType={ttype}", content);

                _logger.LogInformation($"Response: {ServiceName} - {response.StatusCode}");
                /*
                 * Sample response :
                 * [
                 *     0:{ 
                 *         "translations": [ 
                 *              0:{
                 *                  "text": "Merhaba",
                 *                  "to": "tr"
                 *              }
                 *         ] 
                 *     }
                 * ]
                 */
                var responseContent = await response.Content.ReadAsStringAsync();

                var responseDto = JsonConvert.DeserializeObject<MicrosoftTranslateResult[]>(responseContent);

                return new TranslationResult
                {
                    Text = responseDto[0].Translations[0].Text,
                    StatusCode = response.StatusCode,
                    Target = target,
                    Source = source
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error {ServiceName} - {e.Message}");
            }

            return new TranslationResult
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                Text = text,
                Target = target,
                Source = source
            };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="text"></param>
        /// <param name="translation"></param>
        /// <returns></returns>
        public bool TryTranslate(string source, string target, string text, out string translation)
        {
            var trans = Task.Run(() => TranslateAsync(source, target, text, "text")).GetAwaiter().GetResult();

            if (trans.StatusCode == HttpStatusCode.OK)
            {
                translation = trans.Text;
                return true;
            }

            translation = text;
            return false;
        }
    }
}
