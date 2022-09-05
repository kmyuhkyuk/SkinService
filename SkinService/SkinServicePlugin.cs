using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Comfort.Common;
using EFT;
using SkinService.Patches;
using SkinService.Utils;

namespace SkinService
{
    [BepInPlugin("com.kmyuhkyuk.SkinService", "kmyuhkyuk-SkinService", "1.1.0")]
    public class SkinServicePlugin : BaseUnityPlugin
    {
        public static ISession Session;

        public static MainApplication MainApplication;

        public static AllSkinInfo allskinInfo = new AllSkinInfo();

        private object[] Templates;

        private Dictionary<EBodyModelPart, string> oldCustomization;

        private string oldVoice;

        private SkinItemInfo skinitem = new SkinItemInfo();

        private SettingsData settingsdata = new SettingsData();

        public static Action<object> LoadSkinItem;

        public static Action LoadConfig;

        private void Start()
        {
            Logger.LogInfo("Loaded: kmyuhkyuk-SkinService");

            new MainApplicationPatch().Enable();
            new GameWorldPatch().Enable();
            new SkinItemPatch().Enable();
            new PlayerPatch().Enable();

            Localized.Init();
            RaidSkinReplace.Init();

            LoadSkinItem = GetItem;
            LoadConfig = LoadSkinConfig;
        }

        void LoadSkinConfig()
        {
            string MainSettings = "Skin Service Settings";

            settingsdata.KeyWho = Config.Bind<string>(MainSettings, "Who", allskinInfo.Name[0], new ConfigDescription("", new AcceptableValueList<string>(allskinInfo.Name.ToArray()), new ConfigurationManagerAttributes { Order = 9, HideDefaultButton = true }));

            settingsdata.KeyBody = Config.Bind<string>(MainSettings, "Body", skinitem.Body.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(skinitem.Body.Localization), new ConfigurationManagerAttributes { Order = 8, HideDefaultButton = true }));
            settingsdata.KeyFeet = Config.Bind<string>(MainSettings, "Feet", skinitem.Feet.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(skinitem.Feet.Localization), new ConfigurationManagerAttributes { Order = 7, HideDefaultButton = true }));
            settingsdata.KeyHead = Config.Bind<string>(MainSettings, "Head", skinitem.Head.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(skinitem.Head.Localization), new ConfigurationManagerAttributes { Order = 6, HideDefaultButton = true }));
            settingsdata.KeyHands = Config.Bind<string>(MainSettings, "Hands", skinitem.Hands.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(skinitem.Hands.Localization), new ConfigurationManagerAttributes { Order = 5, HideDefaultButton = true }));
            settingsdata.KeyVoice = Config.Bind<string>(MainSettings, "Voice", skinitem.Voice.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(skinitem.Voice.Localization), new ConfigurationManagerAttributes { Order = 4, HideDefaultButton = true }));

            Config.Bind(MainSettings, "GetBotDrawer", "", new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 3, HideDefaultButton = true, CustomDrawer = GetBotDrawer, HideSettingName = true }));

            Config.Bind(MainSettings, "GetNowDrawer", "", new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2, HideDefaultButton = true, CustomDrawer = GetNowDrawer, HideSettingName = true }));

            Config.Bind(MainSettings, "SaveDrawer", "", new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1, HideDefaultButton = true, CustomDrawer = SaveDrawer, HideSettingName = true }));

            GetNow();
        }

        void GetBotDrawer(ConfigEntryBase entry)
        {
            if (GUILayout.Button("Get All Bot", GUILayout.ExpandWidth(true)))
            {
                string old = settingsdata.KeyWho.Value;

                Config.Remove(settingsdata.KeyWho.Definition);

                settingsdata.KeyWho = Config.Bind<string>("Skin Service Settings", "Who", old, new ConfigDescription("", new AcceptableValueList<string>(allskinInfo.Name.ToArray()), new ConfigurationManagerAttributes { Order = 9, HideDefaultButton = true }));
            }
        }

        void SaveDrawer(ConfigEntryBase entry)
        {
            if (GUILayout.Button("Save", GUILayout.ExpandWidth(true)))
            {
                int num = IsPmc();

                int who = IsWho();

                int allwho = allskinInfo.Who.Count;

                if (who == 0)
                {
                    Save(allskinInfo.Who[0].Player, allskinInfo.Who[0].PlayerBody, allskinInfo.Who[0].Customization[num], allskinInfo.Who[0].InfoClass[num], allskinInfo.Who[0].Profile[num], Convert.ToBoolean(num));

                }
                else if (who == 1)
                {
                    if (allwho <= 2)
                        return;

                    for (int i = 2; i< allwho; i++)
                    {
                        Save(allskinInfo.Who[i].Player, allskinInfo.Who[i].PlayerBody, allskinInfo.Who[i].Customization[0], allskinInfo.Who[i].InfoClass[0], allskinInfo.Who[i].Profile[0], false);
                    }
                }
                else
                {
                    Save(allskinInfo.Who[who].Player, allskinInfo.Who[who].PlayerBody, allskinInfo.Who[who].Customization[0], allskinInfo.Who[who].InfoClass[0], allskinInfo.Who[who].Profile[0], false);
                }
            }
        }

        void Save(Player player, PlayerBody body, Dictionary<EBodyModelPart, string> customization, InfoClass infoclass, Profile profile, bool IsPmc)
        {
            List<string> array = new List<string>()
            {
                skinitem.Body.Id[GetIndex(settingsdata.KeyBody.Value, skinitem.Body.Localization)],
                skinitem.Feet.Id[GetIndex(settingsdata.KeyFeet.Value, skinitem.Feet.Localization)],
                skinitem.Head.Id[GetIndex(settingsdata.KeyHead.Value, skinitem.Head.Localization)],
                skinitem.Hands.Id[GetIndex(settingsdata.KeyHands.Value, skinitem.Hands.Localization)]
            };

            //Save old customization
            oldCustomization = new Dictionary<EBodyModelPart, string>(customization);

            customization[EBodyModelPart.Body] = array[0];
            customization[EBodyModelPart.Feet] = array[1];
            customization[EBodyModelPart.Head] = array[2];
            customization[EBodyModelPart.Hands] = array[3];

            //Save old voice id
            oldVoice = infoclass.Voice;

            infoclass.Voice = skinitem.Voice.Id[GetIndex(settingsdata.KeyVoice.Value, skinitem.Voice.Localization)];

            //Save To Local Profile
            if (IsPmc)
            {
                //Trigger ChangeCustomization event
                ApplySkinChange(array.ToArray(), new Callback(SkinBack));

                //Trigger ChangeVoice event
                Session.ChangeVoice(infoclass.Voice, new Callback(VoiceBack));
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
            settingsdata.KeyBody.Value = skinitem.Body.Localization[GetNowPartIndex(EBodyModelPart.Body, skinitem.Body.Id)];
            settingsdata.KeyFeet.Value = skinitem.Feet.Localization[GetNowPartIndex(EBodyModelPart.Feet, skinitem.Feet.Id)];
            settingsdata.KeyHead.Value = skinitem.Head.Localization[GetNowPartIndex(EBodyModelPart.Head, skinitem.Head.Id)];
            settingsdata.KeyHands.Value = skinitem.Hands.Localization[GetNowPartIndex(EBodyModelPart.Hands, skinitem.Hands.Id)];

            settingsdata.KeyVoice.Value = skinitem.Voice.Localization[GetNowVoiceIndex(skinitem.Voice.Id)];

        }

        void ApplySkinChange(string[] ids,Callback onFinished)
        {
            Session.SendOperationRightNow(new SkinGeneric<string, string[]>("ChangeCustomization", ids), onFinished);
        }

        void SkinBack(IResult response)
        {
            if (response.ErrorCode != 0)
            {
                foreach (KeyValuePair<EBodyModelPart, string> keyValuePair in oldCustomization)
                {
                    allskinInfo.Who[0].Customization[1][keyValuePair.Key] = keyValuePair.Value;
                }
            }
        }

        void VoiceBack(IResult result)
        {
            if (!result.Succeed)
            {
                allskinInfo.Who[0].InfoClass[1].Voice = oldVoice;
            }
        }

        void GetItem(object skinobject)
        {
            Templates = Traverse.Create(skinobject).Property("Templates").GetValue<object[]>();

            GetSkin(EBodyModelPart.Body, Templates, skinitem.Body);

            GetSkin(EBodyModelPart.Feet, Templates, skinitem.Feet);

            GetSkin(EBodyModelPart.Head, Templates, skinitem.Head);

            GetSkin(EBodyModelPart.Hands, Templates, skinitem.Hands);

            GetVoice(skinobject, skinitem.Voice);
        }

        void GetSkin(EBodyModelPart part, object[] templates, SkinItemInfo.ItemInfo iteminfo)
        {
            iteminfo.Item = templates.Where(x => Traverse.Create(x).Field("BodyPart").GetValue<EBodyModelPart>() == part).ToArray();
            iteminfo.Id = iteminfo.Item.Select(x => Traverse.Create(x).Field("Id").GetValue<string>()).ToArray();
            iteminfo.Localization = ItemLocalized(iteminfo.Item);
        }

        void GetVoice(object voiceobject, SkinItemInfo.ItemInfo iteminfo)
        {
            IEnumerable<object> voices = Traverse.Create(voiceobject).Property("Voices").GetValue<IEnumerable<object>>();

            iteminfo.Item = voices.ToArray();
            iteminfo.Id = skinitem.Voice.Item.Select(x => Traverse.Create(x).Field("Name").GetValue<string>()).ToArray();
            iteminfo.Localization = ItemLocalized(skinitem.Voice.Item);
        }

        int GetIndex(string id, string[] ids)
        {
            return Array.FindIndex(ids, x => x == id);
        }

        int GetNowPartIndex(EBodyModelPart part, string[] ids)
        {
            string now;

            int who = IsWho();

            if (who != 0 && who != 1)
            {
                allskinInfo.Who[IsWho()].Customization[0].TryGetValue(part, out now);
            }
            else
            {
                allskinInfo.Who[0].Customization[IsPmc()].TryGetValue(part, out now);
            }

            return GetIndex(now, ids);
        }

        int GetNowVoiceIndex(string[] ids)
        {
            string now;

            int who = IsWho();

            if (who != 0 && who != 1)
            {
                now = allskinInfo.Who[IsWho()].InfoClass[0].Voice;
            }
            else
            {
                now = allskinInfo.Who[0].InfoClass[IsPmc()].Voice;
            }

            return GetIndex(now, ids);
        }

        int IsWho()
        {
            return GetIndex(settingsdata.KeyWho.Value, allskinInfo.Name.ToArray());
        }

        int IsPmc()
        {
            return Convert.ToInt32(Traverse.Create(MainApplication).Field("_raidSettings").GetValue<RaidSettings>().IsPmc);
        }

        public class AllSkinInfo
        {
            public List<SkinInfo> Who = new List<SkinInfo>()
            {
                new SkinInfo(),
                new SkinInfo()
            };

            public List<string> Name = new List<string>()
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

        string[] ItemLocalized(object[] Item)
        {
            List<string> IdNames = new List<string>();

            string[] NameKeys = Item.Select(x => Traverse.Create(x).Property("NameLocalizationKey").GetValue<string>()).ToArray();

            foreach (string key in NameKeys)
            {
                string Localization = Localized.localized(key, null);

                //If this key no has localization else return it name
                if (Localization != key)
                {
                    IdNames.Add(Localization);
                }
                else
                {
                    IdNames.Add(Traverse.Create(Item[GetIndex(key, NameKeys)]).Field("Name").GetValue<string>());
                }    
            }

            return IdNames.ToArray();
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
