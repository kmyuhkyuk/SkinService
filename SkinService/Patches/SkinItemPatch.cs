using Aki.Reflection.Patching;
using System.Reflection;
using SkinService.Utils;
using HarmonyLib;
using System.Collections.Generic;

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
            SkinServicePlugin.LoadSkinItem(Traverse.Create(__instance).Property("Templates").GetValue<object[]>(), Traverse.Create(__instance).Property("Voices").GetValue<IEnumerable<object>>());
        }
    }
}
