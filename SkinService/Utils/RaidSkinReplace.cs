using Aki.Reflection.Utils;
using HarmonyLib;
using System;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;

namespace SkinService.Utils
{
    public class RaidSkinReplace
    {
        public static PlayerBody PlayerBody;

        public static Player IsYourPlayer;

        private static Type SpeakerType;

        private static MethodInfo PlayerBodyMethod;

        private static MethodInfo PoolManagerMethod;

        private static MethodInfo SpeakerMethod;

        private static object Low;

        public static void RaidSkinReplaceInit()
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            PlayerBodyMethod = typeof(PlayerBody).GetMethod("Init", flags);
            PoolManagerMethod = typeof(PoolManager).GetMethod("LoadBundlesAndCreatePools", flags);

            SpeakerType = PatchConstants.EftTypes.Single(x =>
            x.GetMethod("ReplaceVoice", flags) != null);

            SpeakerMethod = SpeakerType.GetMethod("Init", flags);

            Low = Traverse.Create(typeof(JobPriority)).Property("Low").GetValue<object>();
        }

        public static async void Replace(Dictionary<EBodyModelPart, string> customization, InfoClass infoclass, Profile profile)
        {
            await LoadBundlesAndCreatePools(profile, Low);

            EquipmentClass equipment = Traverse.Create(IsYourPlayer).Property("Equipment").GetValue<EquipmentClass>();

            BindableState<Item> itemlnHands = Traverse.Create(PlayerBody).Field("_itemInHands").GetValue<BindableState<Item>>();

            await (Task)PlayerBodyMethod.Invoke(PlayerBody, new object[] { customization, equipment, itemlnHands, LayerMask.NameToLayer("Player"), infoclass.Side });

            PlayerBody.UpdatePlayerRenders(IsYourPlayer.PointOfView, infoclass.Side);

            //Voice
            SpeakerMethod.Invoke(IsYourPlayer.Speaker, new object[] { infoclass.Side, IsYourPlayer.PlayerId, infoclass.Voice, true });
        }

        private static async Task LoadBundlesAndCreatePools(Profile profile, object yield)
        {
            ResourceKey[] resources = profile.GetAllPrefabPaths(true).ToArray<ResourceKey>();

            await (Task)PoolManagerMethod.Invoke(Singleton<PoolManager>.Instance, new object[] { PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, resources, yield, null, default(CancellationToken) });
        }
    }
}
