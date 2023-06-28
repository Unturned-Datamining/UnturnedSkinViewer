using HarmonyLib;
using SDG.Framework.Modules;
using SDG.Unturned;
using UnturnedSkinViewer.Commands;
using UnturnedSkinViewer.Patches;

namespace UnturnedSkinViewer;

public class UnturnedSkinViewerModule : IModuleNexus
{
    private Harmony? m_Harmony;

    public void initialize()
    {
        m_Harmony = new Harmony("UnturnedSkinViewer");
        m_Harmony.PatchAll(typeof(UnturnedSkinViewerModule).Assembly);

        Patch_Commander.CommandsInitializedEvent += Patch_Commander_CommandsInitializedEvent;
    }

    public void shutdown()
    {
        m_Harmony?.UnpatchAll();
        Patch_Commander.CommandsInitializedEvent -= Patch_Commander_CommandsInitializedEvent;
    }

    private void Patch_Commander_CommandsInitializedEvent()
    {
        Commander.register(new CommandSkinViewer());
    }
}
