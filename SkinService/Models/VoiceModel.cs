using EFT;
using EFTApi.Helpers;

#if !UNITY_EDITOR

namespace SkinService.Models
{
    internal class VoiceModel : SkinModel
    {
        public readonly ResourceKey VoicePrefab;

        public VoiceModel(string id, string name, string nameLocalizationKey) : base(id, name, nameLocalizationKey,
            null, null)
        {
            VoicePrefab = new ResourceKey
            {
                path = VoiceHelper.Instance.TakePhrasePath(name),
                rcid = name
            };
        }
    }
}

#endif