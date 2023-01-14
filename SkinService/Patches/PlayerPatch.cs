﻿using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using HarmonyLib;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using EFT;

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
                SkinServicePlugin.AllSkinInfos.Who[0].Player = __instance;
                SkinServicePlugin.AllSkinInfos.Who[0].PlayerBody = __instance.PlayerBody;
            }
            else
            {
                SkinServicePlugin.AllSkinInfo.SkinInfo info = new SkinServicePlugin.AllSkinInfo.SkinInfo();

                info.Player = __instance;

                info.PlayerBody = __instance.PlayerBody;

                Profile profile = __instance.Profile;

                info.Profile = new Profile[]
                {
                    profile
                };

                info.InfoClass = new InfoClass[]
                {
                    profile.Info
                };

                info.Customization = new Dictionary<EBodyModelPart, string>[]
                {
                    Traverse.Create(profile).Field("Customization").GetValue<Dictionary<EBodyModelPart, string>>()
                };

                SkinServicePlugin.AllSkinInfos.Who.Add(info);
                SkinServicePlugin.AllSkinInfos.Name.Add(string.Concat("Bot", __instance.Id - 1));
            }
        }
    }
}
