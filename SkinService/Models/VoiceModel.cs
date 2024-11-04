#if !UNITY_EDITOR

using EFT;
using EFTApi.Helpers;

namespace SkinService.Models
{
    internal class VoiceModel : SkinModel
    {
        public readonly ResourceKey VoicePrefab;

        public VoiceModel(object id, string name, string nameLocalizationKey) : base(id, name, nameLocalizationKey,
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