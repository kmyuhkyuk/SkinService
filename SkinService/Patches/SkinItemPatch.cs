using Aki.Reflection.Patching;
using System.Reflection;
using SkinService.Utils;

namespace SkinService.Patches
{
    public class SkinItemPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            return RefHelp.GetEftType(x => x.GetMethod("GetAnyCustomizationItem", flags) != null).GetMethod("SetAvailableSuites", flags);
        }

        [PatchPostfix]
        private static void PatchPostfix(object __instance)
        {
            SkinServicePlugin.LoadSkinItem(__instance);
        }
    }
}
