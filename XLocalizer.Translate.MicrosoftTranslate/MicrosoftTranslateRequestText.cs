namespace XLocalizer.Translate.MicrosoftTranslate
{
    /// <summary>
    /// Microsoft translate request object.
    /// This object will be converted to json to be submitted with the post request
    /// </summary>
    public class MicrosoftTranslateRequestText
    {
        /// <summary>
        /// The text to be translated
        /// </summary>
        public string Text { get; set; }
    }
}
