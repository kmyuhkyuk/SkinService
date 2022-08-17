using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Comfort.Common;
using EFT;
using SkinService.Patches;
using SkinService.Utils;

namespace SkinService
{
    [BepInPlugin("com.kmyuhkyuk.SkinService", "kmyuhkyuk-SkinService", "1.0.0")]
    public class SkinServicePlugin : BaseUnityPlugin
    {
        public static Profile[] Profile;

        public static ISession Session;

        public static MainApplication MainApplication;

        public static InfoClass[] InfoClass;

        public static object SkinItem;

        public static object[] Templates;

        public static Dictionary<EBodyModelPart, string>[] Customization;

        private Dictionary<EBodyModelPart, string> oldCustomization;

        public static IEnumerable<object> Voices;

        private string oldVoice;

        public static ItemInfo Head { get; private set; } = new ItemInfo();

        public static ItemInfo Body { get; private set; } = new ItemInfo();

        public static ItemInfo Feet { get; private set; } = new ItemInfo();

        public static ItemInfo Hands { get; private set; } = new ItemInfo();

        public static ItemInfo Voice { get; private set; } = new ItemInfo();

        public static ConfigEntry<string> KeyHead { get; set; }
        public static ConfigEntry<string> KeyBody { get; set; }
        public static ConfigEntry<string> KeyFeet { get; set; }
        public static ConfigEntry<string> KeyHands { get; set; }
        public static ConfigEntry<string> KeyVoice { get; set; }

        private void Start()
        {
            Logger.LogInfo("Loaded: kmyuhkyuk-SkinService");

            new MainApplicationPatch().Enable();

            new SkinItemPatch().Enable();

            new PlayerPatch().Enable();

            Localized.LocalizedInit();

            RaidSkinReplace.RaidSkinReplaceInit();

            LoadSkinConfig();
        }

        async void LoadSkinConfig()
        {
            await SkinConfig();
        }

        async Task SkinConfig()
        {
            string MainSettings = "Skin Service Settings";

            //Wait Game Load
            while (SkinItem == null)
                await Task.Yield();

            KeyBody = Config.Bind<string>(MainSettings, "Body", Body.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(Body.Localization), new ConfigurationManagerAttributes { Order = 7, HideDefaultButton = true }));
            KeyFeet = Config.Bind<string>(MainSettings, "Feet", Feet.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(Feet.Localization), new ConfigurationManagerAttributes { Order = 6, HideDefaultButton = true }));
            KeyHead = Config.Bind<string>(MainSettings, "Head", Head.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(Head.Localization), new ConfigurationManagerAttributes { Order = 5, HideDefaultButton = true }));
            KeyHands = Config.Bind<string>(MainSettings, "Hands", Hands.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(Hands.Localization), new ConfigurationManagerAttributes { Order = 4, HideDefaultButton = true }));
            KeyVoice = Config.Bind<string>(MainSettings, "Voice", Voice.Localization[0], new ConfigDescription("", new AcceptableValueList<string>(Voice.Localization), new ConfigurationManagerAttributes { Order = 3, HideDefaultButton = true }));

            Config.Bind(MainSettings, "GetNowDrawer", "", new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2, HideDefaultButton = true, CustomDrawer = GetNowDrawer, HideSettingName = true }));

            Config.Bind(MainSettings, "SaveDrawer", "", new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1, HideDefaultButton = true, CustomDrawer = SaveDrawer, HideSettingName = true }));

            GetNow();
        }

        void SaveDrawer(ConfigEntryBase entry)
        {
            if (GUILayout.Button("Save", GUILayout.ExpandWidth(true)))
            {
                int num = IsPmc();

                Save(Customization[num], InfoClass[num], Profile[num], Convert.ToBoolean(num));
            }
        }

        void Save(Dictionary<EBodyModelPart, string> customization, InfoClass infoclass, Profile profile, bool IsPmc)
        {
            List<string> array = new List<string>()
            {
                Body.Id[GetIndex(KeyBody.Value, Body.Localization)],
                Feet.Id[GetIndex(KeyFeet.Value, Feet.Localization)],
                Head.Id[GetIndex(KeyHead.Value, Head.Localization)],
                Hands.Id[GetIndex(KeyHands.Value, Hands.Localization)]
            };

            //Save old customization
            oldCustomization = new Dictionary<EBodyModelPart, string>(customization);

            customization[EBodyModelPart.Body] = array[0];
            customization[EBodyModelPart.Feet] = array[1];
            customization[EBodyModelPart.Head] = array[2];
            customization[EBodyModelPart.Hands] = array[3];

            //Save old voice id
            oldVoice = infoclass.Voice;

            infoclass.Voice = Voice.Id[GetIndex(KeyVoice.Value, Voice.Localization)];

            //Save To Local Profile
            if (IsPmc)
            {
                //Trigger ChangeCustomization event
                ApplySkinChange(array.ToArray(), new Callback(SkinBack));

                //Trigger ChangeVoice event
                Session.ChangeVoice(infoclass.Voice, new Callback(VoiceBack));
            }

            if (RaidSkinReplace.PlayerBody != null)
            {
                RaidSkinReplace.Replace(customization, infoclass, profile);
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
            KeyBody.Value = Body.Localization[GetNowIndex(EBodyModelPart.Body, Body.Id)];
            KeyFeet.Value = Feet.Localization[GetNowIndex(EBodyModelPart.Feet, Feet.Id)];
            KeyHead.Value = Head.Localization[GetNowIndex(EBodyModelPart.Head, Head.Id)];
            KeyHands.Value = Hands.Localization[GetNowIndex(EBodyModelPart.Hands, Hands.Id)];
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
                    Customization[1][keyValuePair.Key] = keyValuePair.Value;
                }
            }
        }

        void VoiceBack(IResult result)
        {
            if (!result.Succeed)
            {
                InfoClass[1].Voice = oldVoice;
            }
        }

        public static void SkinList(object skinobject)
        {
            Templates = Traverse.Create(skinobject).Property("Templates").GetValue<object[]>();

            GetSkinItem(EBodyModelPart.Body, Templates, Body);

            GetSkinItem(EBodyModelPart.Feet, Templates, Feet);

            GetSkinItem(EBodyModelPart.Head, Templates, Head);

            GetSkinItem(EBodyModelPart.Hands, Templates, Hands);
        }

        private static void GetSkinItem(EBodyModelPart part, object[] templates, ItemInfo iteminfo)
        {
            iteminfo.Item = templates.Where(x => Traverse.Create(x).Field("BodyPart").GetValue<EBodyModelPart>() == part).ToArray();
            iteminfo.Id = iteminfo.Item.Select(x => Traverse.Create(x).Field("Id").GetValue<string>()).ToArray();
            iteminfo.Localization = ItemLocalized(iteminfo.Item, iteminfo.Item.Select(x => Traverse.Create(x).Property("NameLocalizationKey").GetValue<string>()).ToArray());
        }

        public static void VoiceList(object voiceobject)
        {
            Voices = Traverse.Create(voiceobject).Property("Voices").GetValue<IEnumerable<object>>();

            Voice.Item = Voices.ToArray();
            Voice.Id = Voice.Item.Select(x => Traverse.Create(x).Field("Name").GetValue<string>()).ToArray();
            Voice.Localization = ItemLocalized(Voice.Item, Voice.Item.Select(x => Traverse.Create(x).Property("NameLocalizationKey").GetValue<string>()).ToArray());
        }

        private static int GetIndex(string id, string[] ids)
        {
            return Array.FindIndex(ids, x => x == id);
        }

        private static int GetNowIndex(EBodyModelPart part, string[] ids)
        {
            string Now;

            Customization[IsPmc()].TryGetValue(part, out Now);

            return GetIndex(Now, ids);
        }

        private static int IsPmc()
        {
            return Convert.ToInt32(Traverse.Create(MainApplication).Field("_raidSettings").GetValue<RaidSettings>().IsPmc);
        }

        public class ItemInfo
        {
            public object[] Item;
            public string[] Id;
            public string[] Localization;
        }

        private static string[] ItemLocalized(object[] Item, string[] NameKey)
        {
            List<string> IdNames = new List<string>();

            foreach (string id in NameKey)
            {
                string Localization = Localized.localized(id, null);

                //If id no have localization else return name
                if (Localization != id)
                {
                    IdNames.Add(Localization);
                }
                else
                {
                    IdNames.Add(Traverse.Create(Item[GetIndex(id, NameKey)]).Field("Name").GetValue<string>());
                }    
            }

            return IdNames.ToArray();
        }
    }
}
