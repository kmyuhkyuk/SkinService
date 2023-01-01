using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using EFT;
using SkinService.Utils.Session;

namespace SkinService.Patches
{
    public class MainApplicationPatch : ModulePatch
    {
        private static readonly bool Is330Up;

        private static readonly MethodBase MainApplicationBase;

        static MainApplicationPatch()
        {
            Type mainApp = PatchConstants.EftTypes.SingleOrDefault(x => x.Name == "MainApplication");

            Is330Up = mainApp == null;

            if (Is330Up)
            {
                MainApplicationBase = PatchConstants.EftTypes.Single(x => x.Name == "TarkovApplication").GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.IsAssembly);
            }
            else
            {
                MainApplicationBase = mainApp.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.IsAssembly);
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            return MainApplicationBase;
        }

        [PatchPostfix]
        private static void PatchPostfix(object __instance)
        {
            object backEnd;

            if (Is330Up)
            {
                backEnd = Traverse.Create(__instance).Field("ClientBackEnd").GetValue<object>();
            }
            else
            {
                backEnd = Traverse.Create(__instance).Field("_backEnd").GetValue<object>();
            }

            ISession session = Traverse.Create(backEnd).Property("Session").GetValue<ISession>();

            Profile pmcProfile = Traverse.Create(session).Property("Profile").GetValue<Profile>();

            Profile scavProfile = session.ProfileOfPet;

            SkinServicePlugin.Raid = Traverse.Create(__instance).Field("_raidSettings").GetValue<RaidSettings>();

            ISessionHelp.Init(session);

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