#if !UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using EFT;
using EFT.InventoryLogic;
using EFTApi;
using EFTReflection;

namespace SkinService.Models
{
    internal class ReflectionModel
    {
        private static readonly Lazy<ReflectionModel> Lazy = new Lazy<ReflectionModel>(() => new ReflectionModel());

        public static ReflectionModel Instance => Lazy.Value;

        public readonly RefHelper.FieldRef<object, EBodyModelPart> RefBodyPart;

        public readonly RefHelper.FieldRef<object, string> RefId;

        public readonly RefHelper.FieldRef<object, string> RefName;

        public readonly RefHelper.FieldRef<object, ResourceKey> RefPrefab;

        public readonly RefHelper.FieldRef<object, ResourceKey> RefWatchPrefab;

        public readonly RefHelper.FieldRef<Profile, Dictionary<EBodyModelPart, string>> RefCustomization;

        public readonly RefHelper.FieldRef<Player, BindableState<Item>> RefItemInHands;

        public readonly RefHelper.PropertyRef<object, object[]> RefTemplates;

        public readonly RefHelper.PropertyRef<object, string> RefNameLocalizationKey;

        public readonly RefHelper.PropertyRef<object, IEnumerable> RefVoices;

        public readonly RefHelper.HookRef CustomizationClassConstructor;

        private readonly Func<PlayerBody, object, object, BindableState<Item>, int,
                EPlayerSide, string, Task>
            _refPlayerBodyInit;

        private readonly Func<PlayerBody, object, object, BindableState<Item>, int,
                EPlayerSide, Task>
            _refBelow358PlayerBodyInit;

        private ReflectionModel()
        {
            var customizationClassType =
                RefTool.GetEftType(x => x.GetMethod("GetAnyCustomizationItem", RefTool.Public) != null);

            RefTemplates = RefHelper.PropertyRef<object, object[]>.Create(customizationClassType, "Templates");

            var templateType = RefTemplates.PropertyType.GetElementType();

            RefBodyPart = RefHelper.FieldRef<object, EBodyModelPart>.Create(templateType, "BodyPart");
            RefId = RefHelper.FieldRef<object, string>.Create(templateType, "Id");
            RefName = RefHelper.FieldRef<object, string>.Create(templateType, "Name");
            RefPrefab = RefHelper.FieldRef<object, ResourceKey>.Create(templateType, "Prefab");
            RefWatchPrefab = RefHelper.FieldRef<object, ResourceKey>.Create(templateType, "WatchPrefab");

            RefNameLocalizationKey = RefHelper.PropertyRef<object, string>.Create(templateType, "NameLocalizationKey");

            RefCustomization = RefHelper.FieldRef<Profile, Dictionary<EBodyModelPart, string>>.Create("Customization");

            RefVoices = RefHelper.PropertyRef<object, IEnumerable>.Create(customizationClassType, "Voices");

            CustomizationClassConstructor = RefHelper.HookRef.Create(customizationClassType.GetConstructors()[0]);

            RefItemInHands = RefHelper.FieldRef<Player, BindableState<Item>>.Create("_itemInHands");

            if (EFTVersion.AkiVersion > EFTVersion.Parse("3.5.8"))
            {
                _refPlayerBodyInit = RefHelper
                    .ObjectMethodDelegate<Func<PlayerBody, object, object, BindableState<Item>, int,
                        EPlayerSide, string, Task>>(RefTool.GetEftMethod(typeof(PlayerBody), RefTool.Public,
                        x => x.GetParameters().Length == 6));
            }
            else
            {
                _refBelow358PlayerBodyInit = RefHelper
                    .ObjectMethodDelegate<Func<PlayerBody, object, object, BindableState<Item>, int,
                        EPlayerSide, Task>>(typeof(PlayerBody).GetMethod("Init", RefTool.Public));
            }
        }

        public Task PlayerBodyInit(PlayerBody instance, object customization,
            object equipment, BindableState<Item> itemInHands, int layer, EPlayerSide playerSide,
            string playerProfileID)
        {
            return EFTVersion.AkiVersion > EFTVersion.Parse("3.5.8")
                ? _refPlayerBodyInit(instance, customization, equipment, itemInHands, layer, playerSide,
                    playerProfileID)
                : _refBelow358PlayerBodyInit(instance, customization, equipment, itemInHands, layer, playerSide);
        }
    }
}

#endif