#if !UNITY_EDITOR

using EFT;
using EFTApi;
using SkinService.Models;

namespace SkinService
{
    public partial class SkinServicePlugin
    {
        private static void PlayerInit(Player __instance)
        {
            var skinServiceModel = SkinServiceModel.Instance;

            if (EFTHelpers._PlayerHelper.IsYourPlayer(__instance))
            {
                skinServiceModel.IsScav = __instance.Side == EPlayerSide.Savage;
                skinServiceModel.CurrentPlayerSkin.Player = __instance;
            }
            else
            {
                skinServiceModel.OtherPlayerSkinList.Add(new PlayerSkinModel(__instance));

                skinServiceModel.UpdateOtherPlayer();
            }
        }
    }
}

#endif