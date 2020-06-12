using System.Collections.Generic;

namespace App1.Core.TextToTextDto
{
    public class Result
    {
        public DetectedLanguage DetectedLanguage { get; set; }
        public IList<Translation> Translations { get; set; }
        public Error Error { get; set; }
    }
}
