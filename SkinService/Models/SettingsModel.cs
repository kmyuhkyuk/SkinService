#if !UNITY_EDITOR

using System.Diagnostics.CodeAnalysis;
using BepInEx.Configuration;
using SkinService.AcceptableValue;
using SkinService.Attributes;
using SkinService.Helpers;
using UnityEngine;

namespace SkinService.Models
{
    internal class SettingsModel
    {
        public static SettingsModel Instance { get; private set; }

        public readonly ConfigEntry<Vector2> KeyDefaultPosition;

        public readonly ConfigEntry<string> KeyLanguage;

        public readonly ConfigEntry<int> KeySortingOrder;

        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
        private SettingsModel(ConfigFile configFile)
        {
            const string mainSettings = "Main Settings";

            configFile.Bind(mainSettings, "Draw Open Skin Service Window", string.Empty,
                new ConfigDescription(string.Empty, null,
                    new ConfigurationManagerAttributes
                        { Order = 2, HideDefaultButton = true, CustomDrawer = DrawSkinService, HideSettingName = true },
                    new EFTConfigurationAttributes { HideSetting = true }));

            configFile.Bind(mainSettings, "Open Skin Service Window", string.Empty,
                new ConfigDescription(string.Empty, null, new ConfigurationManagerAttributes { Browsable = false },
                    new EFTConfigurationAttributes { ButtonAction = OpenSkinServiceView }));

            KeyDefaultPosition = configFile.Bind<Vector2>(mainSettings, "Default Position", new Vector2(800, 0));
            KeySortingOrder = configFile.Bind<int>(mainSettings, "Sorting Order", 29997);

            var localizedHelper = LocalizedHelper.Instance;

            KeyLanguage = configFile.Bind<string>(mainSettings, "Language",
                "En",
                new ConfigDescription(
                    "Preferred language, if not available will tried English, if still not available than return original text",
                    new AcceptableValueCustomList<string>(localizedHelper.Languages)));

            var acceptableValueCustomList =
                (AcceptableValueCustomList<string>)KeyLanguage.Description.AcceptableValues;
            localizedHelper.LanguageAdd += () =>
            {
                acceptableValueCustomList.AcceptableValuesCustom = localizedHelper.Languages;
            };

            localizedHelper.CurrentLanguage = KeyLanguage.Value;
            KeyLanguage.SettingChanged += (value, value2) =>
                localizedHelper.CurrentLanguage = KeyLanguage.Value;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static SettingsModel Create(ConfigFile configFile)
        {
            if (Instance != null)
                return Instance;

            return Instance = new SettingsModel(configFile);
        }

        private static void DrawSkinService(ConfigEntryBase entry)
        {
            if (GUILayout.Button("Open Skin Service Window", GUILayout.ExpandWidth(true)))
            {
                OpenSkinServiceView();
            }
        }

        private static void OpenSkinServiceView()
        {
            SkinServiceModel.Instance.OpenSkinServiceView();
        }
    }
}

#endif