using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Il2Cpp;
using Pathoschild.TheLongDarkMods.Common;
using Action = Il2CppSystem.Action;

namespace Pathoschild.TheLongDarkMods.FastTravel.Framework.Patches;

/// <summary>Harmony patches for the <see cref="Panel_PauseMenu"/> class.</summary>
[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.MethodsReferencedByHarmony)]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = SuppressReasons.ParametersMatchHarmonyConventions)]
internal static class PanelPauseMenuPatches
{
    /*********
    ** Fields
    *********/
    /// <summary>The panel to show when the menu button is clicked.</summary>
    private static DestinationManagerPanel Panel = null!; // set in Initialize

    /// <summary>Get the player's current location.</summary>
    private static System.Func<Destination> GetCurrentLocation = null!; // set in Initialize


    /*********
    ** Public methods
    *********/
    /// <summary>Initialize the patches on startup.</summary>
    /// <param name="panel"><inheritdoc cref="Panel" path="/summary"/></param>
    /// <param name="getCurrentLocation"><inheritdoc cref="GetCurrentLocation" path="/summary"/></param>
    public static void Initialize(DestinationManagerPanel panel, System.Func<Destination> getCurrentLocation)
    {
        PanelPauseMenuPatches.Panel = panel;
        PanelPauseMenuPatches.GetCurrentLocation = getCurrentLocation;
    }

    /// <summary>Patches for the <see cref="Panel_PauseMenu.AddMenuItem"/> method.</summary>
    [HarmonyPatch(typeof(Panel_PauseMenu), nameof(Panel_PauseMenu.AddMenuItem))]
    public static class AddMenuItemPatches
    {
        /// <summary>Add the 'fast travel' button to the pause menu.</summary>
        /// <param name="__instance">The pause menu instance.</param>
        /// <param name="itemIndex">The index of the menu item being added.</param>
        [HarmonyPostfix]
        public static void AddFastTravelOption(Panel_PauseMenu __instance, int itemIndex)
        {
            // make sure we're on the top menu
            BasicMenu menu = __instance.m_BasicMenu;
            if (menu?.m_ItemModelList is null)
                return;

            // wait until 'back to game' is added
            {
                bool foundBackToGame = false;
                bool alreadyAdded = false;

                foreach (var item in menu.m_ItemModelList)
                {
                    switch (item.m_Id)
                    {
                        case "BackToGame":
                            foundBackToGame = true;
                            break;

                        case "FastTravelDestinations":
                            alreadyAdded = true;
                            break;
                    }
                }

                if (!foundBackToGame || alreadyAdded)
                    return;
            }

            // add new option
            menu.AddItem(
                id: "FastTravelDestinations",
                value: 0x4654, // unique identifier for the item (0x4654 = 'FT' in ASCII)
                itemIndex: itemIndex,
                labelText: "Fast travel",
                descriptionText: "Manage your fast travel destinations.",
                secondaryText: null,
                onClickAction: (Action)(() => PanelPauseMenuPatches.Panel.Show(GetCurrentLocation())),
                tintNormal: menu.m_ItemModelList[0].m_NormalTint,
                tintHighlight: menu.m_ItemModelList[0].m_HighlightTint
            );
        }
    }

    /// <summary>Patches for the <see cref="Panel_PauseMenu.OnDone"/> method.</summary>
    [HarmonyPatch(typeof(Panel_PauseMenu), nameof(Panel_PauseMenu.OnDone))]
    public static class OnDonePatches
    {
        /// <summary>Handle the player pressing escape while the destination manager panel is open.</summary>
        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (PanelPauseMenuPatches.Panel.IsVisible)
            {
                PanelPauseMenuPatches.Panel.HandleEscape();
                return false;
            }

            return true;
        }
    }
}
