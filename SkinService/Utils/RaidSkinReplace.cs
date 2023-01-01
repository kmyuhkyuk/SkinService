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
        private static Func<PlayerBody, object, object, BindableState<Item>, int, EPlayerSide, Task> RefPlayerBody;

        private static Func<PoolManager, PoolManager.PoolsCategory, PoolManager.AssemblyType, ResourceKey[], object, object, CancellationToken, Task> RefPoolManager;

        private static RefHelp.FieldRef<Player, object> RefSpeaker;

        private static Action<object, EPlayerSide, int, string, bool> RefReplaceVoice;

        private static object Low;

        public static void Init()
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            RefPlayerBody = RefHelp.ObjectMethodDelegate<Func<PlayerBody, object, object, BindableState<Item>, int, EPlayerSide, Task>>(typeof(PlayerBody).GetMethod("Init", flags));

            RefPoolManager = RefHelp.ObjectMethodDelegate<Func<PoolManager, PoolManager.PoolsCategory, PoolManager.AssemblyType, ResourceKey[], object, object, CancellationToken, Task>>(typeof(PoolManager).GetMethod("LoadBundlesAndCreatePools", flags));

            RefSpeaker = RefHelp.FieldRef<Player, object>.Create("Speaker");

            RefReplaceVoice = RefHelp.ObjectMethodDelegate<Action<object, EPlayerSide, int, string, bool>>(RefHelp.GetEftType(x => x.GetMethod("ReplaceVoice", flags) != null).GetMethod("Init", flags));

            Low = Traverse.Create(typeof(JobPriority)).Property("Low").GetValue<object>();
        }

        public static async void Replace(Player player, PlayerBody body, Dictionary<EBodyModelPart, string> customization, InfoClass infoclass, Profile profile)
        {
            await LoadBundlesAndCreatePools(profile, Low);

            EquipmentClass equipment = Traverse.Create(player).Property("Equipment").GetValue<EquipmentClass>();

            BindableState<Item> itemlnHands = Traverse.Create(body).Field("_itemInHands").GetValue<BindableState<Item>>();

            await RefPlayerBody(body, customization, equipment, itemlnHands, LayerMask.NameToLayer("Player"), infoclass.Side);

            body.UpdatePlayerRenders(player.PointOfView, infoclass.Side);

            //Voice
            RefReplaceVoice(RefSpeaker.GetValue(player), infoclass.Side, player.PlayerId, infoclass.Voice, true);
        }

        private static async Task LoadBundlesAndCreatePools(Profile profile, object yield)
        {
            ResourceKey[] resources = profile.GetAllPrefabPaths(true).ToArray();

            await RefPoolManager(Singleton<PoolManager>.Instance, PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, resources, yield, null, default(CancellationToken));
        }
    }
}
