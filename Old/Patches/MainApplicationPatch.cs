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

        protected override MethodBase GetTargetMethod()
        {
            BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance;

            if (Is330Up)
            {
                return RefHelp.GetEftMethod(x => x.Name == "TarkovApplication", flags, x => x.IsAssembly);
            }
            else
            {
                return RefHelp.GetEftMethod(x => x.Name == "MainApplication", flags, x => x.IsAssembly);
            }
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

            info.Profile = new[]
            {
                scavProfile,
                pmcProfile
            };

            info.Customization = new[]
            {
                Traverse.Create(scavProfile).Field("Customization").GetValue<Dictionary<EBodyModelPart, string>>(),
                Traverse.Create(pmcProfile).Field("Customization").GetValue<Dictionary<EBodyModelPart, string>>()
            };

            info.InfoClass = new[]
            {
                scavProfile.Info,
                pmcProfile.Info
            };
        }
    }
}