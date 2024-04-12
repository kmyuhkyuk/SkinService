#if !UNITY_EDITOR

using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;
using static EFTApi.EFTHelpers;

namespace SkinService.Models
{
    internal class PlayerSkinModel
    {
        public Player Player;

        public PlayerBody PlayerBody => Player.PlayerBody;

        public readonly Profile Profile;

        public InfoClass InfoClass => Profile.Info;

        public Dictionary<EBodyModelPart, string> Customization =>
            ReflectionModel.Instance.RefCustomization.GetValue(Profile);

        public string Nickname => Profile.Nickname;

        public object Inventory => _InventoryHelper.RefInventory.GetValue(Profile);

        public object Equipment => _InventoryHelper.RefEquipment.GetValue(Inventory);

        public BindableState<Item> ItemInHands => ReflectionModel.Instance.RefItemInHands.GetValue(Player);

        public EPlayerSide Side => InfoClass.Side;

        public int PlayerId => Player.PlayerId;

        public EPointOfView PointOfView => Player.PointOfView;

        public PlayerSkinModel(Profile profile)
        {
            Profile = profile;
        }

        public PlayerSkinModel(Player player)
        {
            Player = player;
            Profile = player.Profile;
        }
    }
}

#endif