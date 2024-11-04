using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if !UNITY_EDITOR
using System.Threading.Tasks;
using SkinService.Models;
using EFT;
using Newtonsoft.Json;
using static EFTApi.EFTHelpers;

#endif

namespace SkinService.Views
{
    public class SkinServiceView : MonoBehaviour
    {
        [SerializeField] private Transform windowRoot;

        [SerializeField] private Button closeButton;

        [SerializeField] private TMP_Text skinServiceName;

        [SerializeField] private SkinServiceConfig bodySkin;

        [SerializeField] private SkinServiceConfig feetSkin;

        [SerializeField] private SkinServiceConfig headSkin;

        [SerializeField] private SkinServiceConfig handsSkin;

        [SerializeField] private SkinServiceConfig voice;

        [SerializeField] private SkinServiceAction applyPlayerSkin;

        [SerializeField] private SkinServiceAction applyScavPlayerSkin;

        [SerializeField] private SkinServiceConfig otherPlayer;

        [SerializeField] private SkinServiceAction applyOtherPlayerSkin;

        [SerializeField] private SkinServiceAction applyAllOtherPlayerSkin;

        private bool State
        {
            get => windowRoot.gameObject.activeSelf;
            set
            {
                if (State == value)
                    return;

#if !UNITY_EDITOR

                if (value)
                {
                    _windowRect.anchoredPosition = SettingsModel.Instance.KeyDefaultPosition.Value;

                    UpdateSkinService();
                }

#endif

                windowRoot.gameObject.SetActive(value);
            }
        }

        private RectTransform _windowRect;

#if !UNITY_EDITOR

        private void Awake()
        {
            _windowRect = windowRoot.GetComponent<RectTransform>();

            closeButton.onClick.AddListener(() => State = false);

            UpdateLocalized();

            bodySkin.Init("Body", () => UpdateSkinIndex(bodySkin, EBodyModelPart.Body));
            feetSkin.Init("Feet", () => UpdateSkinIndex(feetSkin, EBodyModelPart.Feet));
            headSkin.Init("Head", () => UpdateSkinIndex(headSkin, EBodyModelPart.Head));
            handsSkin.Init("Hands", () => UpdateSkinIndex(handsSkin, EBodyModelPart.Hands));
            voice.Init("Voice", () => UpdateVoiceIndex(voice));
            otherPlayer.Init("Other Player");
            applyPlayerSkin.Init("Apply Current Player Skin", () =>
            {
                ChangeSelfSkin(bodySkin.CurrentIndex, feetSkin.CurrentIndex, headSkin.CurrentIndex,
                    handsSkin.CurrentIndex, false);
                ChangeSelfVoice(voice.CurrentIndex, false);
            });
            applyScavPlayerSkin.Init("Apply Scav Player Skin", () =>
            {
                ChangeSelfSkin(bodySkin.CurrentIndex, feetSkin.CurrentIndex, headSkin.CurrentIndex,
                    handsSkin.CurrentIndex, true);
                ChangeSelfVoice(voice.CurrentIndex, true);
            });
            applyOtherPlayerSkin.Init("Apply Other Player Skin",
                () =>
                {
                    var otherPlayerIndex = otherPlayer.CurrentIndex;

                    ChangeOtherSkin(otherPlayerIndex, bodySkin.CurrentIndex, feetSkin.CurrentIndex,
                        headSkin.CurrentIndex,
                        handsSkin.CurrentIndex);
                    ChangeOtherVoice(otherPlayerIndex, voice.CurrentIndex);
                });
            applyAllOtherPlayerSkin.Init("Apply All Other Player Skin",
                () =>
                {
                    ChangeAllOtherSkin(bodySkin.CurrentIndex, feetSkin.CurrentIndex, headSkin.CurrentIndex,
                        handsSkin.CurrentIndex);
                    ChangeAllOtherVoice(voice.CurrentIndex);
                });
        }

        private void Start()
        {
            var skinServiceModel = SkinServiceModel.Instance;

            skinServiceModel.UpdateSkinService = UpdateSkinService;
            skinServiceModel.UpdateOtherPlayer = () =>
            {
                var oldIndex = otherPlayer.CurrentIndex;

                otherPlayer.UpdateDropdown(skinServiceModel.OtherPlayerSkinList.Select(x => x.Nickname).ToList());

                otherPlayer.CurrentIndex = oldIndex;
            };
            skinServiceModel.ClearOtherPlayer = otherPlayer.ClearDropdown;

            skinServiceModel.OpenSkinServiceView = () =>
            {
                State = false;
                State = true;
            };

            Helpers.LocalizedHelper.Instance.LanguageChange += UpdateLocalized;
        }

        private void Update()
        {
            if (SettingsModel.Instance.KeySkinServiceShortcut.Value.IsDown())
            {
                State = !State;
            }
        }

        private void UpdateLocalized()
        {
            skinServiceName.text = Helpers.LocalizedHelper.Instance.Localized("SkinService");
        }

        private void UpdateSkinService()
        {
            UpdateSkin(bodySkin, EBodyModelPart.Body);
            UpdateSkin(feetSkin, EBodyModelPart.Feet);
            UpdateSkin(headSkin, EBodyModelPart.Head);
            UpdateSkin(handsSkin, EBodyModelPart.Hands);
            UpdateVoice(voice);
        }

        private static void ChangeAllOtherSkin(int bodySkinIndex, int feetSkinIndex,
            int headSkinIndex, int handsSkinIndex)
        {
            var skinServiceModel = SkinServiceModel.Instance;

            if (!skinServiceModel.TryGetFullSkinModel(bodySkinIndex, feetSkinIndex, headSkinIndex, handsSkinIndex,
                    out var bodySkin, out var feetSkin, out var headSkin, out var handsSkin))
                return;

            foreach (var playerSkinModel in skinServiceModel.OtherPlayerSkinList)
            {
                InitSkin(playerSkinModel, bodySkin, feetSkin, headSkin, handsSkin);
            }
        }

        private static void ChangeOtherSkin(int otherPlayerIndex, int bodySkinIndex, int feetSkinIndex,
            int headSkinIndex, int handsSkinIndex)
        {
            var skinServiceModel = SkinServiceModel.Instance;

            if (skinServiceModel.TryGetFullSkinModel(bodySkinIndex, feetSkinIndex, headSkinIndex, handsSkinIndex,
                    out var bodySkin, out var feetSkin, out var headSkin, out var handsSkin) &&
                skinServiceModel.TryGetOtherPlayerSkinModel(otherPlayerIndex, out var playerSkinModel))
            {
                InitSkin(playerSkinModel, bodySkin, feetSkin, headSkin, handsSkin);
            }
        }

        private static void ChangeSelfSkin(int bodySkinIndex, int feetSkinIndex, int headSkinIndex, int handsSkinIndex,
            bool isScav)
        {
            var skinServiceModel = SkinServiceModel.Instance;

            if (!skinServiceModel.TryGetFullSkinModel(bodySkinIndex, feetSkinIndex, headSkinIndex, handsSkinIndex,
                    out var bodySkin, out var feetSkin, out var headSkin, out var handsSkin))
                return;

            RequestSkin(bodySkin.Id, feetSkin.Id, headSkin.Id, handsSkin.Id, isScav);
            InitSkin(isScav ? skinServiceModel.ScavPlayerSkin : skinServiceModel.CurrentPlayerSkin, bodySkin,
                feetSkin, headSkin, handsSkin);
        }

        private static void RequestSkin(object bodySkinId, object feetSkinId,
            object headSkinId, object handsSkinId, bool isScav)
        {
            _RequestHandlerHelper.PutJson(isScav ? "/skin-service/scav/change" : "/skin-service/pmc/change",
                JsonConvert.SerializeObject(new
                {
                    bodyId = bodySkinId,
                    feetId = feetSkinId,
                    headId = headSkinId,
                    handsId = handsSkinId
                }));
        }

        private static async void InitSkin(PlayerSkinModel playerSkinModel, SkinModel bodySkinModel,
            SkinModel feetSkinModel,
            SkinModel headSkinModel, SkinModel handsSkinModel)
        {
            playerSkinModel.Customization[EBodyModelPart.Body] = bodySkinModel.Id;
            playerSkinModel.Customization[EBodyModelPart.Feet] = feetSkinModel.Id;
            playerSkinModel.Customization[EBodyModelPart.Head] = headSkinModel.Id;
            playerSkinModel.Customization[EBodyModelPart.Hands] = handsSkinModel.Id;

            if (playerSkinModel.Player == null)
                return;

            var resourceKeyList = new List<ResourceKey>
            {
                bodySkinModel.Prefab,
                feetSkinModel.Prefab,
                headSkinModel.Prefab,
                handsSkinModel.Prefab
            };

            if (!string.IsNullOrEmpty(handsSkinModel.WatchPrefab.path))
            {
                resourceKeyList.Add(handsSkinModel.WatchPrefab);
            }

            await LoadBundlesAndCreatePools(resourceKeyList.ToArray());

            await ReflectionModel.Instance.PlayerBodyInit(playerSkinModel.PlayerBody, playerSkinModel.Customization,
                playerSkinModel.Equipment, playerSkinModel.ItemInHands, LayerMask.NameToLayer("Player"),
                playerSkinModel.Side);

            playerSkinModel.PlayerBody.UpdatePlayerRenders(playerSkinModel.PointOfView, playerSkinModel.Side);
        }

        private static void ChangeOtherVoice(int otherPlayerIndex, int voiceIndex)
        {
            var skinServiceModel = SkinServiceModel.Instance;

            if (skinServiceModel.TryGetVoiceModel(voiceIndex, out var voiceModel) &&
                skinServiceModel.TryGetOtherPlayerSkinModel(otherPlayerIndex, out var playerSkinModel))
            {
                InitVoice(playerSkinModel, voiceModel);
            }
        }

        private static void ChangeAllOtherVoice(int voiceIndex)
        {
            var skinServiceModel = SkinServiceModel.Instance;

            if (!skinServiceModel.TryGetVoiceModel(voiceIndex, out var voiceModel))
                return;

            foreach (var playerSkinModel in skinServiceModel.OtherPlayerSkinList)
            {
                InitVoice(playerSkinModel, voiceModel);
            }
        }

        private static void ChangeSelfVoice(int voiceIndex, bool isScav)
        {
            var skinServiceModel = SkinServiceModel.Instance;

            if (!skinServiceModel.TryGetVoiceModel(voiceIndex, out var voiceModel))
                return;

            RequestVoice(voiceModel.Name, isScav);
            InitVoice(isScav ? skinServiceModel.ScavPlayerSkin : skinServiceModel.CurrentPlayerSkin, voiceModel);
        }

        private static void RequestVoice(string voice, bool isScav)
        {
            _RequestHandlerHelper.PutJson(isScav ? "/skin-service/scav/voice/change" : "/skin-service/pmc/voice/change",
                JsonConvert.SerializeObject(new
                {
                    voiceId = voice
                }));
        }

        private static async void InitVoice(PlayerSkinModel playerSkinModel, VoiceModel voiceModel)
        {
            playerSkinModel.InfoClass.Voice = voiceModel.Name;

            if (playerSkinModel.Player == null)
                return;

            await LoadBundlesAndCreatePools(new[] { voiceModel.VoicePrefab });

            _SpeakerHelper.Init(_SpeakerHelper.Speaker, playerSkinModel.Side, playerSkinModel.PlayerId,
                voiceModel.Name);
        }

        private static Task LoadBundlesAndCreatePools(ResourceKey[] resources)
        {
            return _PoolManagerHelper.LoadBundlesAndCreatePools(_PoolManagerHelper.PoolManager,
                PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, resources,
                _JobPriorityHelper.Low);
        }

        private static void UpdateSkin(SkinServiceConfig config, EBodyModelPart bodyPart)
        {
            var skinServiceModel = SkinServiceModel.Instance;

            if (skinServiceModel.TryGetSkinModelArray(bodyPart, out var skinModelArray) &&
                skinServiceModel.TryGetCurrentSkin(bodyPart, out var currentSkin))
            {
                config.UpdateConfig(GetLocalizationNameList(skinModelArray),
                    FindSkinIndex(skinModelArray, currentSkin));
            }
        }

        private static void UpdateVoice(SkinServiceConfig config)
        {
            var skinServiceModel = SkinServiceModel.Instance;

            if (skinServiceModel.TryGetVoiceModelArray(out var voiceModelArray) &&
                skinServiceModel.TryGetCurrentVoice(out var currentVoice))
            {
                config.UpdateConfig(GetLocalizationNameList(voiceModelArray),
                    FindVoiceIndex(voiceModelArray, currentVoice));
            }
        }

        private static void UpdateSkinIndex(SkinServiceConfig config, EBodyModelPart bodyPart)
        {
            var skinServiceModel = SkinServiceModel.Instance;

            if (skinServiceModel.TryGetSkinModelArray(bodyPart, out var skinModelArray) &&
                skinServiceModel.TryGetCurrentSkin(bodyPart, out var currentSkin))
            {
                config.CurrentIndex = FindSkinIndex(skinModelArray, currentSkin);
            }
        }

        private static void UpdateVoiceIndex(SkinServiceConfig config)
        {
            var skinServiceModel = SkinServiceModel.Instance;

            if (skinServiceModel.TryGetVoiceModelArray(out var voiceModelArray) &&
                skinServiceModel.TryGetCurrentVoice(out var currentVoice))
            {
                config.CurrentIndex = FindVoiceIndex(voiceModelArray, currentVoice);
            }
        }

        private static int FindSkinIndex(IEnumerable<SkinModel> skinModelEnumerable, string id)
        {
            return FindIndex(skinModelEnumerable, x => x.Id == id);
        }

        private static int FindVoiceIndex(IEnumerable<VoiceModel> skinModelEnumerable, string id)
        {
            return FindIndex(skinModelEnumerable, x => x.Name == id);
        }

        private static int FindIndex(IEnumerable<SkinModel> skinModelEnumerable, Func<SkinModel, bool> skinModelFunc)
        {
            var index = -1;

            foreach (var skinModel in skinModelEnumerable)
            {
                index++;

                if (skinModelFunc(skinModel))
                    return index;
            }

            return -1;
        }

        private static List<string> GetLocalizationNameList(IEnumerable<SkinModel> skinModelEnumerable)
        {
            return skinModelEnumerable.Select(x => x.LocalizationName).ToList();
        }

#endif
    }
}