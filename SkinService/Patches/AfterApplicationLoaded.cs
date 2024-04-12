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

            var profile = _SessionHelper.Profile;
            var profileOfPet = _SessionHelper.ProfileOfPet;

            skinServiceModel.PlayerSkin = new PlayerSkinModel(profile);
            skinServiceModel.ScavPlayerSkin = new PlayerSkinModel(profileOfPet);
        }
    }
}

#endif