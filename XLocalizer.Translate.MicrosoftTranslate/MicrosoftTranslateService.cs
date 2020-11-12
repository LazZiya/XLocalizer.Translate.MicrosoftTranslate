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
    /// Microsoft Translate service over Azure cognitive services
    /// for subscription visit <a href="https://azure.microsoft.com/en-us/services/cognitive-services/translator/">Microsoft Translator Azure Services</a>
    /// </summary>
    public class MicrosoftTranslateService : ITranslator
    {
        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName => "Microsoft Translator Azure";

        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        /// <summary>
        /// Initialize Microsoft translate service, for subscription visit <a href="https://azure.microsoft.com/en-us/services/cognitive-services/translator/">Microsoft Translator Text Azure Services</a>
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public MicrosoftTranslateService(HttpClient httpClient, IConfiguration configuration, ILogger<MicrosoftTranslateServiceRapidApi> logger)
        {
            _httpClient = httpClient ?? throw new NullReferenceException(nameof(httpClient));

            var _key = configuration["XLocalizer.Translate:Microsoft:Key"] ?? throw new NullReferenceException("RapidApi key not found");
            var _region = configuration["XLocalizer.Translate:Microsoft:Region"];

            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _key);

            if (!string.IsNullOrWhiteSpace(_region))
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", _region);

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
                var response = await _httpClient.PostAsync($"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={target}&from={source}&textType={ttype}", content);

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
