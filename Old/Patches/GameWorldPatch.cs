using Aki.Reflection.Patching;
using System.Reflection;
using EFT;

namespace SkinService.Patches
{
    public class GameWorldPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            SkinServicePlugin.AllSkinInfos.Clear();
        }
    }
}
