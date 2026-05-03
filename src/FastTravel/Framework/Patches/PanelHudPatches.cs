using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using Pathoschild.TheLongDarkMods.Common;

namespace Pathoschild.TheLongDarkMods.FastTravel.Framework.Patches;

/// <summary>Harmony patches for the <see cref="Panel_HUD"/> class.</summary>
[HarmonyPatch(typeof(Panel_HUD), nameof(Panel_HUD.ShowLocationReveal))]
[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Harmony patches are applied by MelonLoader automatically.")]
internal static class PanelHudPatches
{
    /*********
    ** Fields
    *********/
    /// <summary>The log instance.</summary>
    private static MelonLogger.Instance Log = null!; // set in Initialize

    /// <summary>Get the destination that should be shown on-screen when the player arrives. This deletes the cached value, if any.</summary>
    private static Func<Destination?> ConsumeDestinationOnArrival = null!; // set in Initialize


    /*********
    ** Public methods
    *********/
    /// <summary>Initialize the patch on startup.</summary>
    /// <param name="log"><inheritdoc cref="Log" path="/summary"/></param>
    /// <param name="consumeDestinationOnArrival"><inheritdoc cref="ConsumeDestinationOnArrival" path="/summary"/></param>
    public static void Initialize(MelonLogger.Instance log, Func<Destination?> consumeDestinationOnArrival)
    {
        PanelHudPatches.Log = log;
        PanelHudPatches.ConsumeDestinationOnArrival = consumeDestinationOnArrival;
    }

    /// <summary>Prefix <see cref="Panel_HUD.ShowLocationReveal"/> to set the region name to the correct value after fast travel.</summary>
    /// <param name="subText">The localized region name that will be shown in-game.</param>
    public static void Prefix(ref string subText)
    {
        MelonLogger.Instance log = PanelHudPatches.Log;
        Destination? destination = PanelHudPatches.ConsumeDestinationOnArrival();

        if (destination is null)
            return;

        if (destination.Region is null)
        {
            log.Warning("Can't override location text after fast travel: the destination has no region saved.");
            return;
        }

        string currentSceneName = SceneHelper.GetSceneName();
        if (destination.Scene.Name != currentSceneName)
        {
            log.Warning($"Can't override location text after fast travel: the current scene '{currentSceneName}' doesn't match the fast travel destination '{destination.Scene.Name}'.");
            return;
        }

        subText = Localization.Get(destination.Region.NameLocalizationId);
    }
}
