#if !UNITY_EDITOR

using System;
using System.Collections.Generic;
using EFTUtils;

namespace SkinService.Helpers
{
    internal class LocalizedHelper : CustomLocalized<string, Dictionary<string, string>>
    {
        private static readonly Lazy<LocalizedHelper> Lazy = new Lazy<LocalizedHelper>(() => new LocalizedHelper());

        public static LocalizedHelper Instance => Lazy.Value;

        public override string Localized(string key)
        {
            if ((LanguageDictionary.TryGetValue(CurrentLanguageLower, out var localizedDictionary) ||
                 LanguageDictionary.TryGetValue("en", out localizedDictionary)) &&
                localizedDictionary.TryGetValue(key, out var localized))
            {
                return localized;
            }

            return key;
        }
    }
}

#endif