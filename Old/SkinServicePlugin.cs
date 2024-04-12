using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using Comfort.Common;
using EFT;
using SkinService.Patches;
using SkinService.Utils;
using SkinService.Utils.Session;

namespace SkinService
{
    [BepInPlugin("com.kmyuhkyuk.SkinService", "kmyuhkyuk-SkinService", "1.1.4")]
    public class SkinServicePlugin : BaseUnityPlugin
    {
        internal static RaidSettings Raid;

        internal static readonly AllSkinInfo AllSkinInfos = new AllSkinInfo();

        private Dictionary<EBodyModelPart, string> OldCustomization;

        private string OldVoice;

        private readonly SkinItemInfo SkinItems = new SkinItemInfo();

        private readonly SettingsData SettingsDatas = new SettingsData();

        internal static Action<object[], IEnumerable<object>> LoadSkinItem;

        internal static Version GameVersion { get; private set; }

        private void Start()
        {
            Logger.LogInfo("Loaded: kmyuhkyuk-SkinService");

            FileVersionInfo exeInfo = Process.GetCurrentProcess().MainModule.FileVersionInfo;

            GameVersion = new Version(exeInfo.FileMajorPart, exeInfo.ProductMinorPart, exeInfo.ProductBuildPart, exeInfo.FilePrivatePart);

            new MainApplicationPatch().Enable();
            new GameWorldPatch().Enable();
            new SkinItemPatch().Enable();
            new PlayerPatch().Enable();

            LocalizedHelp.Init();
            RaidSkinReplace.Init();

            LoadSkinItem = GetItem;
        }

        void LoadSkinConfig()
        {
            const string mainSettings = "Skin Service Settings";

            string playerName = AllSkinInfos.Name[0];

            SettingsDatas.KeyWho = Config.Bind<string>(mainSettings, "Who", playerName, new ConfigDescription("", new AcceptableValueList<string>(AllSkinInfos.Name.ToArray()), new ConfigurationManagerAttributes { Order = 9, HideDefaultButton = true }));

            SettingsDatas.KeyWho.Value = playerName;

            SettingsDatas.KeyBody = Config.Bind<string>(mainSettings, "Body", SkinItems.Body.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(SkinItems.Body.Localization), new ConfigurationManagerAttributes { Order = 8, HideDefaultButton = true }));
            SettingsDatas.KeyFeet = Config.Bind<string>(mainSettings, "Feet", SkinItems.Feet.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(SkinItems.Feet.Localization), new ConfigurationManagerAttributes { Order = 7, HideDefaultButton = true }));
            SettingsDatas.KeyHead = Config.Bind<string>(mainSettings, "Head", SkinItems.Head.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(SkinItems.Head.Localization), new ConfigurationManagerAttributes { Order = 6, HideDefaultButton = true }));
            SettingsDatas.KeyHands = Config.Bind<string>(mainSettings, "Hands", SkinItems.Hands.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(SkinItems.Hands.Localization), new ConfigurationManagerAttributes { Order = 5, HideDefaultButton = true }));
            SettingsDatas.KeyVoice = Config.Bind<string>(mainSettings, "Voice", SkinItems.Voice.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(SkinItems.Voice.Localization), new ConfigurationManagerAttributes { Order = 4, HideDefaultButton = true }));

            Config.Bind(mainSettings, "GetBotDrawer", "", new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 3, HideDefaultButton = true, CustomDrawer = GetBotDrawer, HideSettingName = true }));

            Config.Bind(mainSettings, "GetNowDrawer", "", new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2, HideDefaultButton = true, CustomDrawer = GetNowDrawer, HideSettingName = true }));

            Config.Bind(mainSettings, "SaveDrawer", "", new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1, HideDefaultButton = true, CustomDrawer = SaveDrawer, HideSettingName = true }));

            GetNow();
        }

        void GetBotDrawer(ConfigEntryBase entry)
        {
            if (GUILayout.Button("Get All Bot", GUILayout.ExpandWidth(true)))
            {
                string old = SettingsDatas.KeyWho.Value;

                Config.Remove(SettingsDatas.KeyWho.Definition);

                SettingsDatas.KeyWho = Config.Bind<string>("Skin Service Settings", "Who", old, new ConfigDescription("", new AcceptableValueList<string>(AllSkinInfos.Name.ToArray()), new ConfigurationManagerAttributes { Order = 9, HideDefaultButton = true }));
            }
        }

        void SaveDrawer(ConfigEntryBase entry)
        {
            if (GUILayout.Button("Save", GUILayout.ExpandWidth(true)))
            {
                int num = IsPmc();

                int who = IsWho();

                int allWho = AllSkinInfos.Who.Count;

                if (who == 0)
                {
                    var nowWho = AllSkinInfos.Who[0];

                    Save(nowWho.Player, nowWho.PlayerBody, nowWho.Customization[num], nowWho.InfoClass[num], nowWho.Profile[num], Convert.ToBoolean(num));

                }
                else if (who == 1)
                {
                    if (allWho > 2)
                    {
                        for (int i = 2; i < allWho; i++)
                        {
                            var nowWho = AllSkinInfos.Who[i];

                            Save(nowWho.Player, nowWho.PlayerBody, nowWho.Customization[0], nowWho.InfoClass[0], nowWho.Profile[0], false);
                        }
                    }
                }
                else
                {
                    var nowWho = AllSkinInfos.Who[who];

                    Save(nowWho.Player, nowWho.PlayerBody, nowWho.Customization[0], nowWho.InfoClass[0], nowWho.Profile[0], false);
                }
            }
        }

        void Save(Player player, PlayerBody body, Dictionary<EBodyModelPart, string> customization, InfoClass infoclass, Profile profile, bool ispmc)
        {
            List<string> array = new List<string>
            {
                SkinItems.Body.Id[GetIndex(SettingsDatas.KeyBody.Value, SkinItems.Body.Localization)],
                SkinItems.Feet.Id[GetIndex(SettingsDatas.KeyFeet.Value, SkinItems.Feet.Localization)],
                SkinItems.Head.Id[GetIndex(SettingsDatas.KeyHead.Value, SkinItems.Head.Localization)],
                SkinItems.Hands.Id[GetIndex(SettingsDatas.KeyHands.Value, SkinItems.Hands.Localization)]
            };

            //Save old customization
            OldCustomization = new Dictionary<EBodyModelPart, string>(customization);

            customization[EBodyModelPart.Body] = array[0];
            customization[EBodyModelPart.Feet] = array[1];
            customization[EBodyModelPart.Head] = array[2];
            customization[EBodyModelPart.Hands] = array[3];

            //Save old voice id
            OldVoice = infoclass.Voice;

            infoclass.Voice = SkinItems.Voice.Id[GetIndex(SettingsDatas.KeyVoice.Value, SkinItems.Voice.Localization)];

            //Save To Local Profile
            if (ispmc)
            {
                //Trigger ChangeCustomization event
                ApplySkinChange(array.ToArray(), new Callback(SkinBack));

                //Trigger ChangeVoice event
                ISessionHelp.ChangeVoice(infoclass.Voice, new Callback(VoiceBack));
            }

            if (body != null)
            {
                RaidSkinReplace.Replace(player, body, customization, infoclass, profile);
            }
        }

        void GetNowDrawer(ConfigEntryBase entry)
        {
            if (GUILayout.Button("Get Now Skin and Voice", GUILayout.ExpandWidth(true)))
            {
                GetNow();
            }
        }

        void GetNow()
        {
            //Prevent the value is allbot cause main menu can't load
            if (IsWho() != 1)
            {
                SettingsDatas.KeyBody.Value = SkinItems.Body.Localization[GetNowPartIndex(EBodyModelPart.Body, SkinItems.Body.Id)];
                SettingsDatas.KeyFeet.Value = SkinItems.Feet.Localization[GetNowPartIndex(EBodyModelPart.Feet, SkinItems.Feet.Id)];
                SettingsDatas.KeyHead.Value = SkinItems.Head.Localization[GetNowPartIndex(EBodyModelPart.Head, SkinItems.Head.Id)];
                SettingsDatas.KeyHands.Value = SkinItems.Hands.Localization[GetNowPartIndex(EBodyModelPart.Hands, SkinItems.Hands.Id)];

                SettingsDatas.KeyVoice.Value = SkinItems.Voice.Localization[GetNowVoiceIndex(SkinItems.Voice.Id)];
            }
        }

        void ApplySkinChange(string[] ids, Callback onFinished)
        {
            ISessionHelp.SendOperationRightNow(new SkinClass<string, string[]>("ChangeCustomization", ids), onFinished);
        }

        void SkinBack(IResult response)
        {
            if (response.ErrorCode != 0)
            {
                foreach (KeyValuePair<EBodyModelPart, string> keyValuePair in OldCustomization)
                {
                    AllSkinInfos.Who[0].Customization[1][keyValuePair.Key] = keyValuePair.Value;
                }
            }
        }

        void VoiceBack(IResult result)
        {
            if (!result.Succeed)
            {
                AllSkinInfos.Who[0].InfoClass[1].Voice = OldVoice;
            }
        }

        void GetItem(object[] templates, IEnumerable<object> voices)
        {
            GetSkin(EBodyModelPart.Body, templates, SkinItems.Body);
            GetSkin(EBodyModelPart.Feet, templates, SkinItems.Feet);
            GetSkin(EBodyModelPart.Head, templates, SkinItems.Head);
            GetSkin(EBodyModelPart.Hands, templates, SkinItems.Hands);

            GetVoice(voices, SkinItems.Voice);

            LoadSkinConfig();
        }

        void GetSkin(EBodyModelPart part, object[] templates, SkinItemInfo.ItemInfo iteminfo)
        {
            iteminfo.Item = templates.Where(x => Traverse.Create(x).Field("BodyPart").GetValue<EBodyModelPart>() == part).ToArray();
            iteminfo.Id = iteminfo.Item.Select(x => Traverse.Create(x).Field("Id").GetValue<string>()).ToArray();
            iteminfo.Localization = ItemLocalized(iteminfo.Item);
        }

        void GetVoice(IEnumerable<object> voices, SkinItemInfo.ItemInfo iteminfo)
        {
            iteminfo.Item = voices.ToArray();
            iteminfo.Id = SkinItems.Voice.Item.Select(x => Traverse.Create(x).Field("Name").GetValue<string>()).ToArray();
            iteminfo.Localization = ItemLocalized(SkinItems.Voice.Item);
        }

        int GetIndex(string id, string[] ids)
        {
            return Array.IndexOf(ids, id);
        }

        int GetNowPartIndex(EBodyModelPart part, string[] ids)
        {
            int who = IsWho();

            int isPmc;

            if (who > 0)
            {
                isPmc = 0;
            }
            else
            {
                isPmc = IsPmc();
            }

            return GetIndex(AllSkinInfos.Who[who].Customization[isPmc][part], ids);
        }

        int GetNowVoiceIndex(string[] ids)
        {
            int who = IsWho();

            int isPmc;

            if (who > 0)
            {
                isPmc = 0;
            }
            else
            {
                isPmc = IsPmc();
            }

            return GetIndex(AllSkinInfos.Who[who].InfoClass[isPmc].Voice, ids);
        }

        int IsWho()
        {
            return GetIndex(SettingsDatas.KeyWho.Value, AllSkinInfos.Name.ToArray());
        }

        int IsPmc()
        {
            return Convert.ToInt32(Raid.IsPmc);
        }

        public class AllSkinInfo
        {
            public List<SkinInfo> Who = new List<SkinInfo>
            {
                new SkinInfo(),
                new SkinInfo()
            };

            public List<string> Name = new List<string>
            {
                "Player",
                "All Bot"
            };

            public class SkinInfo
            {
                public Player Player;

                public PlayerBody PlayerBody;

                public Profile[] Profile;

                public InfoClass[] InfoClass;

                public Dictionary<EBodyModelPart, string>[] Customization;
            }

            public void Clear()
            {
                Who.RemoveRange(2, Who.Count - 2);
                Name.RemoveRange(2, Name.Count - 2);
            }
        }

        public class SkinItemInfo
        {
            public ItemInfo Head = new ItemInfo();

            public ItemInfo Body = new ItemInfo();

            public ItemInfo Feet = new ItemInfo();

            public ItemInfo Hands = new ItemInfo();

            public ItemInfo Voice = new ItemInfo();

            public class ItemInfo
            {
                public object[] Item;
                public string[] Id;
                public string[] Localization;
            }
        }

        public class SkinClass<T, V>
        {
            public T Action;
            public V Skinids;

            public SkinClass(T name, V value)
            {
                Action = name;
                Skinids = value;
            }
        }

        string[] ItemLocalized(object[] item)
        {
            List<string> idNames = new List<string>();

            string[] nameKeys = item.Select(x => Traverse.Create(x).Property("NameLocalizationKey").GetValue<string>()).ToArray();

            for (int i = 0; i < nameKeys.Length; i++)
            {
                string nameKey = nameKeys[i];

                string Localization = LocalizedHelp.localized(nameKey, null);

                //If this key no has localization else return it name
                if (Localization != nameKey && !string.IsNullOrEmpty(Localization))
                {
                    idNames.Add(Localization);
                }
                else
                {
                    idNames.Add(Traverse.Create(item[i]).Field("Name").GetValue<string>());
                }
            }

            return idNames.ToArray();
        }

        public class SettingsData
        {
            public ConfigEntry<string> KeyWho;
            public ConfigEntry<string> KeyHead;
            public ConfigEntry<string> KeyBody;
            public ConfigEntry<string> KeyFeet;
            public ConfigEntry<string> KeyHands;
            public ConfigEntry<string> KeyVoice;
        }
    }
}
