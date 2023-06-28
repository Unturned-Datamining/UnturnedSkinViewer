using HarmonyLib;
using SDG.Unturned;
using Action = System.Action;

namespace UnturnedSkinViewer.Patches;

[HarmonyPatch(typeof(Commander), nameof(Commander.init))]
internal class Patch_Commander
{
    public static event Action CommandsInitializedEvent;

    [HarmonyPostfix]
    public static void Initialized()
    {
        CommandsInitializedEvent?.Invoke();
    }
}