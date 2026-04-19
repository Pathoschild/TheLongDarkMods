using ModSettings;

namespace Pathoschild.TheLongDarkMods.AutoFillMapOnExplore.Framework;

/// <summary>The mod config model.</summary>
internal class ModConfig : JsonModSettings
{
    /*********
    ** Accessors
    *********/
    /// <summary>Whether the mod is enabled.</summary>
    [Name("Enabled")]
    [Description("Whether the mod is enabled.")]
    public bool Enabled = true;

    /// <summary>The distance around the player where icons appear.</summary>
    [Name("Auto-Fill Distance")]
    [Description("The distance around you where icons appear. The fog reveal distance will be somewhat larger.")]
    [Slider(0, 500, 51)] // in increments of 10
    public int AutoFillRadius = 20;

    /// <summary>The number of seconds between each map fill.</summary>
    [Name("Auto-Fill Delay")]
    [Description("The number of seconds between each map auto-fill.")]
    [Slider(1, 30, 30)] // in increments of 1
    public int AutoFillSeconds = 5;
}
