using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using System.Linq;
using System.Reflection;

namespace SkinService.Patches
{
    public class SkinItemPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            return PatchConstants.EftTypes.Single(x =>
            x.GetMethod("GetAnyCustomizationItem", flags) != null)
            .GetMethod("SetAvailableSuites", flags);
        }

        [PatchPostfix]
        private static void PatchPostfix(object __instance)
        {
            SkinServicePlugin.LoadSkinItem(__instance);
            SkinServicePlugin.LoadConfig();
        }
    }
}
