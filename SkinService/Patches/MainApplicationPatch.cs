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
        private static void PatchPostfix(MainApplication __instance)
        {
            IBackendInterface __backEnd = Traverse.Create(__instance).Field("_backEnd").GetValue<IBackendInterface>();

            var Pmcprofile = __backEnd.Session.Profile;

            var Scavprofile = __backEnd.Session.ProfileOfPet;

            SkinServicePlugin.MainApplication = __instance;

            SkinServicePlugin.Session = __backEnd.Session;

            SkinServicePlugin.AllSkinInfo.SkinInfo info = SkinServicePlugin.allskininfo.Who[0];

            info.Profile = new Profile[]
            {
                Scavprofile,
                Pmcprofile
            };

            info.Customization = new Dictionary<EBodyModelPart, string>[]
            {
                Traverse.Create(Scavprofile).Field("Customization").GetValue<Dictionary<EBodyModelPart, string>>(),
                Traverse.Create(Pmcprofile).Field("Customization").GetValue<Dictionary<EBodyModelPart, string>>()
            };

            info.InfoClass = new InfoClass[]
            {
                Scavprofile.Info,
                Pmcprofile.Info
            };
        }
    }
}