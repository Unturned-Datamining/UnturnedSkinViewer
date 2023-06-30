using HarmonyLib;
using SDG.Provider;
using SDG.Unturned;
using Steamworks;
using System;

namespace UnturnedSkinViewer.Commands;
public class CommandSkinViewer : Command
{
    public CommandSkinViewer()
    {
        _command = "skinviewer";
    }

    protected override void execute(CSteamID executorID, string parameter)
    {
        var args = parameter.Split(' ');
        if (args.Length == 0)
        {
            return;
        }

        var player = Player.player;
        var clothing = player.clothing;

        if (args[0].Equals("clear", StringComparison.OrdinalIgnoreCase))
        {
            // try clear specific type
            if (args.Length >= 2 && Enum.TryParse<EItemType>(args[1], true, out var type))
            {
                SetVisualClothing(clothing, type, 0);
                return;
            }

            SetVisualClothing(clothing, EItemType.BACKPACK, 0);
            SetVisualClothing(clothing, EItemType.GLASSES, 0);
            SetVisualClothing(clothing, EItemType.PANTS, 0);
            SetVisualClothing(clothing, EItemType.HAT, 0);
            SetVisualClothing(clothing, EItemType.MASK, 0);
            SetVisualClothing(clothing, EItemType.SHIRT, 0);
            SetVisualClothing(clothing, EItemType.VEST, 0);
            return;
        }

        var econInfo = FindEconWithMythic(args[0],
            args.Length >= 2 ? args[1] : null);

        if (econInfo == null)
        {
            UnturnedLog.info("No econ info found for specific GUID");
            return;
        }

        var asset = Assets.find(econInfo.item_guid);

        if (asset is ItemClothingAsset clothingAsset)
        {
            SetVisualClothing(clothing, clothingAsset.type, econInfo.itemdefid);
            return;
        }

        if (asset is ItemAsset itemAsset)
        {
            player.channel.owner.itemSkins[itemAsset.sharedSkinLookupID] = econInfo.itemdefid;
            player.inventory.tryAddItemAuto(new Item(itemAsset, EItemOrigin.ADMIN), true, true, true, true);
        }
    }

    private UnturnedEconInfo? FindEconInfo(string input)
    {
        if (Guid.TryParse(input, out var guid))
        {
            return TempSteamworksEconomy.econInfo.Find(x => x.item_guid == guid);
        }

        return int.TryParse(input, out var item)
            ? TempSteamworksEconomy.econInfo.Find(x => x.itemdefid == item)
            : null;
    }

    private MythicAsset? FindMythicAsset(string input)
    {
        if (Guid.TryParse(input, out var guid))
        {
            return Assets.find<MythicAsset>(guid);
        }

        return ushort.TryParse(input, out var id)
            ? Assets.find(EAssetType.MYTHIC, id) as MythicAsset
            : null;
    }

    private UnturnedEconInfo? FindEconWithMythic(string econInfoInput, string? mythicInput)
    {
        var econInfo = FindEconInfo(econInfoInput);
        if (econInfo == null || econInfo.item_guid == Guid.Empty || mythicInput == null)
        {
            return econInfo;
        }

        var mythic = FindMythicAsset(mythicInput);
        if (mythic == null)
        {
            return econInfo;
        }

        var newEconInfo = TempSteamworksEconomy.econInfo
            .Find(x => x.item_guid == econInfo.item_guid && x.item_effect == mythic.id);

        return newEconInfo ?? econInfo;
    }

    private void SetVisualClothing(PlayerClothing clothing, EItemType type, int item)
    {
        static void SetPropertyValue(PlayerClothing clothing, string propertyName, int item)
        {
            var property = typeof(HumanClothes).GetProperty(propertyName, AccessTools.all);
            if (clothing.thirdClothes != null)
            {
                property.SetValue(clothing.thirdClothes, item);
                clothing.thirdClothes.apply();
            }

            if (clothing.firstClothes != null)
            {
                property.SetValue(clothing.firstClothes, item);
                clothing.firstClothes.apply();
            }

            if (clothing.characterClothes != null)
            {
                property.SetValue(clothing.characterClothes, item);
                clothing.characterClothes.apply();
            }
        }

        switch (type)
        {
            case EItemType.BACKPACK:
                SetPropertyValue(clothing, nameof(HumanClothes.visualBackpack), item);
                break;
            case EItemType.GLASSES:
                SetPropertyValue(clothing, nameof(HumanClothes.visualGlasses), item);
                break;
            case EItemType.HAT:
                SetPropertyValue(clothing, nameof(HumanClothes.visualHat), item);
                break;
            case EItemType.MASK:
                SetPropertyValue(clothing, nameof(HumanClothes.visualMask), item);
                break;
            case EItemType.PANTS:
                SetPropertyValue(clothing, nameof(HumanClothes.visualPants), item);
                break;
            case EItemType.SHIRT:
                SetPropertyValue(clothing, nameof(HumanClothes.visualShirt), item);
                break;
            case EItemType.VEST:
                SetPropertyValue(clothing, nameof(HumanClothes.visualVest), item);
                break;
        }
    }
}
