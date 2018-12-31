using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core
{
    public static class Localization
    {
        public static readonly Dictionary<string, string> AvailableLanguageCodes = new Dictionary<string, string>
        {
            ["EN"] = "English",
            ["FR"] = "Français",
            ["JP"] = "日本語",
            ["IT"] = "Italiano",
            ["DE"] = "Deutsch",
            ["KR"] = "한국어",
            ["CN"] = "中文繁體",
        };

        public const string DefaultLanguage = "EN";

        public static event EventHandler LanguageChanged;

        private static string language;
        public static string Language
        {
            get
            {
                return language;
            }
            set
            {
                if (language != value)
                {
                    language = value;
                    LanguageChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public static string Get(Dictionary<string, string> localizations)
        {
            if (localizations == null)
                return null;

            // Fallback to default language if language is not provided.
            if (localizations.TryGetValue(Language ?? DefaultLanguage, out string value))
                return value;

            // Fallback to default language if nothing found with provided language
            if (localizations.TryGetValue(DefaultLanguage, out value))
                return value;

            return null;
        }

        public static string GetDefault(Dictionary<string, string> localizations)
        {
            if (localizations == null)
                return null;

            // Fallback to default language if language is not provided.
            localizations.TryGetValue(DefaultLanguage, out string value);

            return value;
        }
    }
}
