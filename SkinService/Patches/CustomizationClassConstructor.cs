#if !UNITY_EDITOR

using System;
using System.Collections.Generic;
using EFT;
using SkinService.Models;

namespace SkinService
{
    public partial class SkinServicePlugin
    {
        private static void CustomizationClassConstructor(object __instance)
        {
            var skinServiceModel = SkinServiceModel.Instance;
            var reflectionModel = ReflectionModel.Instance;

            var templates = reflectionModel.RefTemplates.GetValue(__instance);

            var bodySkinList = new List<SkinModel>();
            var feetSkinList = new List<SkinModel>();
            var headSkinList = new List<SkinModel>();
            var handsSkinList = new List<SkinModel>();
            foreach (var template in templates)
            {
                var skinModel = new SkinModel(reflectionModel.RefId.GetValue(template),
                    reflectionModel.RefName.GetValue(template),
                    reflectionModel.RefNameLocalizationKey.GetValue(template),
                    reflectionModel.RefPrefab.GetValue(template), reflectionModel.RefWatchPrefab.GetValue(template));

                var bodyPart = reflectionModel.RefBodyPart.GetValue(template);

                switch (bodyPart)
                {
                    case EBodyModelPart.Body:
                        bodySkinList.Add(skinModel);
                        break;
                    case EBodyModelPart.Feet:
                        feetSkinList.Add(skinModel);
                        break;
                    case EBodyModelPart.Head:
                        headSkinList.Add(skinModel);
                        break;
                    case EBodyModelPart.Hands:
                        handsSkinList.Add(skinModel);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(bodyPart), bodyPart, null);
                }
            }

            var voices = reflectionModel.RefVoices.GetValue(__instance);

            var voiceList = new List<VoiceModel>();
            foreach (var voice in voices)
            {
                var voiceModel = new VoiceModel(reflectionModel.RefId.GetValue(voice),
                    reflectionModel.RefName.GetValue(voice), reflectionModel.RefNameLocalizationKey.GetValue(voice));

                voiceList.Add(voiceModel);
            }

            skinServiceModel.BodySkins = bodySkinList.ToArray();
            skinServiceModel.FeetSkins = feetSkinList.ToArray();
            skinServiceModel.HeadSkins = headSkinList.ToArray();
            skinServiceModel.HandsSkins = handsSkinList.ToArray();
            skinServiceModel.Voices = voiceList.ToArray();
        }
    }
}

#endif