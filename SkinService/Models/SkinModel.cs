#if !UNITY_EDITOR

using EFT;
using static EFTApi.EFTHelpers;

namespace SkinService.Models
{
    internal class SkinModel
    {
        public readonly object Id;

        public readonly string Name;

        public readonly string NameLocalizationKey;

        public string LocalizationName
        {
            get
            {
                var localizationName = _LocalizedHelper.Localized(NameLocalizationKey);

                return localizationName != NameLocalizationKey && !string.IsNullOrEmpty(localizationName)
                    ? localizationName
                    : Name;
            }
        }

        public readonly ResourceKey Prefab;

        public readonly ResourceKey WatchPrefab;

        public SkinModel(object id, string name, string nameLocalizationKey, ResourceKey prefab,
            ResourceKey watchPrefab)
        {
            Id = id;
            Name = name;
            NameLocalizationKey = nameLocalizationKey;
            Prefab = prefab;
            WatchPrefab = watchPrefab;
        }
    }
}

#endif