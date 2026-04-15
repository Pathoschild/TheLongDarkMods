using ModSettings;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.ShortcutHome.Framework;

/// <summary>The mod config model.</summary>
internal class ModConfig : JsonModSettings
{
    /*********
    ** Accessors
    *********/
    [Name("Set home key")]
    [Description("Press this button to set your current position as your home.")]
    public KeyCode SetHomeKey = KeyCode.KeypadPeriod;

    [Name("Fast travel key")]
    [Description("If you're not home, press this button to (a) save your current location as the return point and (b) fast travel back home.\n\nIf you're home, press this button to fast travel back to your return point.")]
    public KeyCode FastTravelKey = KeyCode.Keypad0;
}
