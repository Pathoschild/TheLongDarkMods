using ModSettings;

namespace Pathoschild.TheLongDarkMods.AutoFillMapOnExplore.Framework;

/// <summary>The mod config model.</summary>
internal class ModConfig : JsonModSettings
{
    /*********
    ** Accessors
    *********/
    /****
    ** Auto-fill
    ****/
    /// <summary>Whether auto-fill is enabled.</summary>
    [Section("Auto-Fill")]
    [Name("Enabled")]
    [Description("Whether the map should auto-fill around you.")]
    public bool AutoFillEnabled = true;

    /// <summary>The distance around the player where icons appear.</summary>
    [Name("Distance")]
    [Description("The distance around you where icons appear. The fog reveal distance will be somewhat larger. Default 20.")]
    [Slider(0, 500, 51)] // in increments of 10
    public int AutoFillRadius = 20;

    /// <summary>The delay (in seconds) between each map fill.</summary>
    [Name("Delay")]
    [Description("The number of seconds between each map auto-fill. Default 5.")]
    [Slider(1, 30, 30)] // in increments of 1
    public int AutoFillSeconds = 5;

    /****
    ** Map options
    ****/
    /// <summary>Whether the player arrow appears on the map.</summary>
    [Section("Map options")]
    [Name("Show Player Arrow")]
    [Description("Whether the player arrow should appear on the map.")]
    public bool ShowPlayerArrow = true;

    /// <summary>Whether to center the map on the player when it's opened.</summary>
    [Name("Center on Player")]
    [Description("Whether to center the map on your position when you open it.")]
    public bool CenterOnPlayer = true;
}
