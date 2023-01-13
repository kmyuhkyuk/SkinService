using Aki.Reflection.Patching;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using EFT;
using SkinService.Utils;
using SkinService.Utils.Session;

namespace SkinService.Patches
{
    public class MainApplicationPatch : ModulePatch
    {
        private static readonly bool Is330Up = SkinServicePlugin.GameVersion > new Version("0.12.12.20243");

        private static readonly MethodBase MainApplicationBase;

        static MainApplicationPatch()
        {
            BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance;

            if (Is330Up)
            {
                MainApplicationBase = RefHelp.GetEftMethod(x => x.Name == "TarkovApplication", flags, x => x.IsAssembly);
            }
            else
            {
                MainApplicationBase = RefHelp.GetEftMethod(x => x.Name == "MainApplication", flags, x => x.IsAssembly);
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