using Aki.Reflection.Patching;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using EFT;

namespace SkinService.Patches
{
    public class MainApplicationPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainApplication).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.IsAssembly);
        }

        [PatchPostfix]
        private static void PatchPostfix(MainApplication __instance, IBackendInterface ____backEnd)
        {
            Profile pmcProfile = ____backEnd.Session.Profile;

            Profile scavProfile = ____backEnd.Session.ProfileOfPet;

            SkinServicePlugin.MainApplication = __instance;

            SkinServicePlugin.Session = ____backEnd.Session;

            SkinServicePlugin.AllSkinInfo.SkinInfo info = SkinServicePlugin.AllSkinInfos.Who[0];

            info.Profile = new Profile[]
            {
                scavProfile,
                pmcProfile
            };

            info.Customization = new Dictionary<EBodyModelPart, string>[]
            {
                Traverse.Create(scavProfile).Field("Customization").GetValue<Dictionary<EBodyModelPart, string>>(),
                Traverse.Create(pmcProfile).Field("Customization").GetValue<Dictionary<EBodyModelPart, string>>()
            };

            info.InfoClass = new InfoClass[]
            {
                scavProfile.Info,
                pmcProfile.Info
            };
        }
    }
}