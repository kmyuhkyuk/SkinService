using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using System.Reflection;
using System.Threading.Tasks;
using EFT;
using SkinService.Utils;

namespace SkinService.Patches
{
    public class PlayerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("Init", PatchConstants.PrivateFlags);
        }

        [PatchPostfix]
        private async static void PatchPostfix(Task __result, Player __instance)
        {
            await __result;

            if (__instance.IsYourPlayer)
            {
                RaidSkinReplace.IsYourPlayer = __instance;

                RaidSkinReplace.PlayerBody = __instance.GetComponentInChildren<PlayerBody>();
            }
        }
    }
}
