#if !UNITY_EDITOR

using SkinService.Models;
using static EFTApi.EFTHelpers;

namespace SkinService
{
    public partial class SkinServicePlugin
    {
        private static void AfterApplicationLoaded()
        {
            var skinServiceModel = SkinServiceModel.Instance;

            skinServiceModel.PlayerSkin = new PlayerSkinModel(_SessionHelper.Profile);
            skinServiceModel.ScavPlayerSkin = new PlayerSkinModel(_SessionHelper.ProfileOfPet);

            skinServiceModel.UpdateSkinService();
        }
    }
}

#endif