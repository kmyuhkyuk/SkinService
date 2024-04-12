#if !UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EFT;
using EFTUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SkinService.Models
{
    internal class SkinServiceModel
    {
        public static SkinServiceModel Instance { get; private set; }

        public PlayerSkinModel PlayerSkin;

        public PlayerSkinModel ScavPlayerSkin;

        public PlayerSkinModel CurrentPlayerSkin => IsScav ? ScavPlayerSkin : PlayerSkin;

        public bool IsScav;

        public readonly List<PlayerSkinModel> OtherPlayerSkinList = new List<PlayerSkinModel>();

        public readonly GameObject SkinServicePublic = new GameObject("SkinServicePublic", typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));

        public readonly string ModPath = Path.Combine(BepInEx.Paths.PluginPath, "kmyuhkyuk-SkinService");

        public SkinModel[] BodySkins;

        public SkinModel[] FeetSkins;

        public SkinModel[] HeadSkins;

        public SkinModel[] HandsSkins;

        public VoiceModel[] Voices;

        public Action UpdateSkinService;

        public Action UpdateOtherPlayer;

        public Action ClearOtherPlayer;

        private SkinServiceModel()
        {
            var canvas = SkinServicePublic.GetComponent<Canvas>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 |
                                              AdditionalCanvasShaderChannels.Normal |
                                              AdditionalCanvasShaderChannels.Tangent;

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).transform.SetParent(
                SkinServicePublic.transform);

            Object.DontDestroyOnLoad(SkinServicePublic);

            var assetBundle = AssetBundleHelper.LoadBundle(Path.Combine(ModPath, "bundles", "skinservice.bundle"));

            Object.Instantiate(
                assetBundle.LoadAsset<GameObject>("SkinService").ReplaceAllFont(EFTFontHelper.BenderNormal),
                SkinServicePublic.transform);

            assetBundle.Unload(false);

            var settingsModel = SettingsModel.Instance;

            canvas.sortingOrder = settingsModel.KeySortingOrder.Value;
            settingsModel.KeySortingOrder.SettingChanged +=
                (value, value2) => canvas.sortingOrder = settingsModel.KeySortingOrder.Value;
        }

        public bool TryGetSkinModelArray(EBodyModelPart bodyPart, out SkinModel[] skinModelArray)
        {
            skinModelArray = null;

            switch (bodyPart)
            {
                case EBodyModelPart.Body:
                    skinModelArray = BodySkins;
                    break;
                case EBodyModelPart.Feet:
                    skinModelArray = FeetSkins;
                    break;
                case EBodyModelPart.Head:
                    skinModelArray = HeadSkins;
                    break;
                case EBodyModelPart.Hands:
                    skinModelArray = HandsSkins;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bodyPart), bodyPart, null);
            }

            return skinModelArray != null;
        }

        public bool TryGetVoiceModelArray(out VoiceModel[] voiceModelArray)
        {
            voiceModelArray = Voices;

            return voiceModelArray != null;
        }

        public bool TryGetCurrentSkin(EBodyModelPart bodyPart, out string currentSkin)
        {
            currentSkin = string.Empty;

            if (CurrentPlayerSkin == null)
                return false;

            currentSkin = CurrentPlayerSkin.Customization[bodyPart];

            return true;
        }

        public bool TryGetCurrentVoice(out string currentVoice)
        {
            currentVoice = string.Empty;

            if (CurrentPlayerSkin == null)
                return false;

            currentVoice = CurrentPlayerSkin.InfoClass.Voice;

            return true;
        }

        public bool TryGetOtherPlayerSkinModel(int otherPlayerIndex, out PlayerSkinModel playerSkinModel)
        {
            playerSkinModel = OtherPlayerSkinList.ElementAtOrDefault(otherPlayerIndex);

            return playerSkinModel != null;
        }

        public bool TryGetFullSkinModel(int bodySkinIndex, int feetSkinIndex, int headSkinIndex, int handsSkinIndex,
            out SkinModel bodySkinModel, out SkinModel feetSkinModel, out SkinModel headSkinModel,
            out SkinModel handsSkinModel)
        {
            return TryGetSkinModel(EBodyModelPart.Body, bodySkinIndex, out bodySkinModel) &
                   TryGetSkinModel(EBodyModelPart.Feet, feetSkinIndex, out feetSkinModel) &
                   TryGetSkinModel(EBodyModelPart.Head, headSkinIndex, out headSkinModel) &
                   TryGetSkinModel(EBodyModelPart.Hands, handsSkinIndex, out handsSkinModel);
        }

        public bool TryGetSkinModel(EBodyModelPart bodyPart, int skinIndex, out SkinModel skinModel)
        {
            skinModel = null;

            if (TryGetSkinModelArray(bodyPart, out var skinModelArray))
            {
                skinModel = skinModelArray.ElementAtOrDefault(skinIndex);
            }

            return skinModel != null;
        }

        public bool TryGetVoiceModel(int voiceIndex, out VoiceModel voiceModel)
        {
            voiceModel = null;

            if (!TryGetVoiceModelArray(out var voiceModelArray))
                return false;

            voiceModel = voiceModelArray.ElementAtOrDefault(voiceIndex);

            return voiceModel != null;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static SkinServiceModel Create()
        {
            if (Instance != null)
                return Instance;

            return Instance = new SkinServiceModel();
        }
    }
}

#endif