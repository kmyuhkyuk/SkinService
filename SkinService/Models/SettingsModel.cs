#if !UNITY_EDITOR

using System.Diagnostics.CodeAnalysis;
using BepInEx.Configuration;
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

            KeyDefaultPosition = configFile.Bind<Vector2>(mainSettings, "Default Position", Vector2.zero);
            KeySortingOrder = configFile.Bind<int>(mainSettings, "Sorting Order", 29997);
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static SettingsModel Create(ConfigFile configFile)
        {
            if (Instance != null)
                return Instance;

            return Instance = new SettingsModel(configFile);
        }
    }
}

#endif