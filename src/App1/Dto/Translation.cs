using System;
using System.Collections.Generic;
using System.Text;

namespace App1.Dto
{
    public class Translation
    {
        public Translation(string sourceText, string targetText, Language sourceLanguage, Language targetLanguage)
        {
            SourceText = sourceText;
            TargetText = targetText;
            SourceLanguage = sourceLanguage;
            TargetLanguage = targetLanguage;
        }
        public string SourceText { get; set; }
        public string TargetText { get; set; }
        public Language SourceLanguage { get; set; }
        public Language TargetLanguage { get; set; }
    }
}
