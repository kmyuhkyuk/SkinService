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
        private static Type SpeakerType;

        private static MethodInfo PlayerBodyMethod;

        private static MethodInfo PoolManagerMethod;

        private static MethodInfo SpeakerMethod;

        private static object Low;

        public static void Init()
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            PlayerBodyMethod = typeof(PlayerBody).GetMethod("Init", flags);
            PoolManagerMethod = typeof(PoolManager).GetMethod("LoadBundlesAndCreatePools", flags);

            SpeakerType = PatchConstants.EftTypes.Single(x =>
            x.GetMethod("ReplaceVoice", flags) != null);

            SpeakerMethod = SpeakerType.GetMethod("Init", flags);

            Low = Traverse.Create(typeof(JobPriority)).Property("Low").GetValue<object>();
        }

        public static async void Replace(Player player, PlayerBody body, Dictionary<EBodyModelPart, string> customization, InfoClass infoclass, Profile profile)
        {
            await LoadBundlesAndCreatePools(profile, Low);

            EquipmentClass equipment = Traverse.Create(player).Property("Equipment").GetValue<EquipmentClass>();

            BindableState<Item> itemlnHands = Traverse.Create(body).Field("_itemInHands").GetValue<BindableState<Item>>();

            await (Task)PlayerBodyMethod.Invoke(body, new object[] { customization, equipment, itemlnHands, LayerMask.NameToLayer("Player"), infoclass.Side });

            body.UpdatePlayerRenders(player.PointOfView, infoclass.Side);

            //Voice
            SpeakerMethod.Invoke(Traverse.Create(player).Field("Speaker").GetValue<object>(), new object[] { infoclass.Side, player.PlayerId, infoclass.Voice, true });
        }

        private static async Task LoadBundlesAndCreatePools(Profile profile, object yield)
        {
            ResourceKey[] resources = profile.GetAllPrefabPaths(true).ToArray<ResourceKey>();

            await (Task)PoolManagerMethod.Invoke(Singleton<PoolManager>.Instance, new object[] { PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, resources, yield, null, default(CancellationToken) });
        }
    }
}
