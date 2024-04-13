#if !UNITY_EDITOR

using System.Diagnostics.CodeAnalysis;
using BepInEx.Configuration;
using SkinService.Attributes;
using UnityEngine;

namespace SkinService.Models
{
    internal class SettingsModel
    {
        public static SettingsModel Instance { get; private set; }

        public readonly ConfigEntry<Vector2> KeyDefaultPosition;

        public readonly ConfigEntry<int> KeySortingOrder;

        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
        private SettingsModel(ConfigFile configFile)
        {
            const string mainSettings = "Skin Service Settings";

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