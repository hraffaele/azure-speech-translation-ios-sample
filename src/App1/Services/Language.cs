using System;

namespace App1
{
    public class Language
    {
        public Language(string name, string shortCode, string voice)
        {
            ShortCode = shortCode;
            Name = name;
            Voice = voice;
        }
        public string ShortCode { get; set; }
        public string Name { get; set; }
        public string Voice { get; set; }

        public static string GetIso6391Name(string languageName)
        {
            if (string.IsNullOrWhiteSpace(languageName) || !languageName.Contains("-"))
            {
                throw new ArgumentException("languageName must contain a '-'");
            }
            return languageName.Split('-')[0];
        }

    }
}
