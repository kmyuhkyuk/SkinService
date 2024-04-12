#if !UNITY_EDITOR

using SkinService.Models;

namespace SkinService
{
    public partial class SkinServicePlugin
    {
        private static void GameWorldDispose()
        {
            var skinServiceModel = SkinServiceModel.Instance;

            skinServiceModel.OtherPlayerSkinList.Clear();
            skinServiceModel.ClearOtherPlayer();
        }
    }
}

#endif