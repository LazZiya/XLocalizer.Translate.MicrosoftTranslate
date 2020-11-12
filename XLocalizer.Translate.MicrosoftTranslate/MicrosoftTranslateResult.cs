using Newtonsoft.Json;

namespace XLocalizer.Translate.SystranTranslate
{
    /// <summary>
    /// Microsoft translate result
    /// </summary>
    public class MicrosoftTranslateResult
    {
        /// <summary>
        /// Translations
        /// </summary>
        [JsonProperty("translations")]
        public MicrosoftResultTranslation[] Translations { get; set; }
    }

    /// <summary>
    /// translate result output
    /// </summary>
    public class MicrosoftResultTranslation
    {
        /// <summary>
        /// translated text
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Target culture
        /// </summary>
        [JsonProperty("to")]
        public string To { get; set; }
    }
}
