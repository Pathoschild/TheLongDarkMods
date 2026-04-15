using System;
using ModSettings;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.ShortcutHome.Framework;

/// <summary>The mod config model.</summary>
internal class ModConfig : JsonModSettings
{
    /*********
    ** Accessors
    *********/
    [Name("Home Location")]
    [Description("The location to treat as your central home (i.e. where you fast travel to).")]
    [Choice("Disabled", "Mystery Lake: camp office")]
    public int HomeLocation = 1;

    [Name("Fast Travel key")]
    [Description("The button you can press to fast travel back home, or (if you're already home) back to the location you warped from.")]
    public KeyCode FastTravelKey = KeyCode.F1;


    /*********
    ** Public methods
    *********/
    /// <summary>Get the scene name for the player's home location, or <c>null</c> to disable it.</summary>
    public string? GetHomeScene()
    {
        return this.HomeLocation switch
        {
            0 => null,
            1 => "CampOffice",
            _ => throw new InvalidOperationException($"Unknown home location ID '{this.HomeLocation}'.")
        };
    }
}
