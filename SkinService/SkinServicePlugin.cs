#if !UNITY_EDITOR

using BepInEx;
using SkinService.Models;
using static EFTApi.EFTHelpers;

namespace SkinService
{
    [BepInPlugin("com.kmyuhkyuk.SkinService", "SkinService", "1.1.4")]
    public partial class SkinServicePlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            SettingsModel.Create(Config);

            SkinServiceModel.Create();
        }

        private void Start()
        {
            ReflectionModel.Instance.CustomizationClassConstructor.Add(this, nameof(CustomizationClassConstructor));

            _SessionHelper.AfterApplicationLoaded.Add(this, nameof(AfterApplicationLoaded));
            _PlayerHelper.Init.Add(this, nameof(PlayerInit));
            _GameWorldHelper.Dispose.Add(this, nameof(GameWorldDispose));
        }
    }
}

#endif