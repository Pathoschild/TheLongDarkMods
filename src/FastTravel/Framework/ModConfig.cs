using ModSettings;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.FastTravel.Framework;

/// <summary>The mod config model.</summary>
internal class ModConfig : JsonModSettings
{
    /*********
    ** Accessors
    *********/
    [Section("Modifier keys")]
    [Name("Save destination")]
    [Description("Save your current place as a fast travel destination by holding this key, then pressing the destination key below you want to bind it to.")]
    public KeyCode SaveModifierKey = KeyCode.KeypadPlus;

    [Name("Forget destination")]
    [Description("Forget a destination by holding this key, then pressing the destination key below you want to delete.")]
    public KeyCode DeleteModifierKey = KeyCode.KeypadMinus;

    [Section("Destinations")]
    [Name("Fast travel point 1")]
    [Description("Press this button to fast travel to your first saved destination. You can change this point using the 'modifier keys' above.")]
    public KeyCode Destination1 = KeyCode.Keypad1;

    [Name("Fast travel point 2")]
    [Description("Press this button to fast travel to your second saved destination. You can change this point using the 'modifier keys' above.")]
    public KeyCode Destination2 = KeyCode.Keypad2;

    [Name("Fast travel point 3")]
    [Description("Press this button to fast travel to your third saved destination. You can change this point using the 'modifier keys' above.")]
    public KeyCode Destination3 = KeyCode.Keypad3;

    [Name("Fast travel point 4")]
    [Description("Press this button to fast travel to your fourth saved destination. You can change this point using the 'modifier keys' above.")]
    public KeyCode Destination4 = KeyCode.Keypad4;

    [Name("Fast travel point 5")]
    [Description("Press this button to fast travel to your fifth saved destination. You can change this point using the 'modifier keys' above.")]
    public KeyCode Destination5 = KeyCode.Keypad5;

    [Name("Fast travel point 6")]
    [Description("Press this button to fast travel to your sixth saved destination. You can change this point using the 'modifier keys' above.")]
    public KeyCode Destination6 = KeyCode.Keypad6;

    [Name("Fast travel point 7")]
    [Description("Press this button to fast travel to your seventh saved destination. You can change this point using the 'modifier keys' above.")]
    public KeyCode Destination7 = KeyCode.Keypad7;

    [Name("Fast travel point 8")]
    [Description("Press this button to fast travel to your eighth saved destination. You can change this point using the 'modifier keys' above.")]
    public KeyCode Destination8 = KeyCode.Keypad8;

    [Name("Fast travel point 9")]
    [Description("Press this button to fast travel to your ninth saved destination. You can change this point using the 'modifier keys' above.")]
    public KeyCode Destination9 = KeyCode.Keypad9;

    [Name("Return to previous location")]
    [Description("Press this button to return to where you were before your *most recent* fast travel.")]
    public KeyCode ReturnPointKey = KeyCode.Keypad0;

    [Section("Debug options")]
    [Name("Log debug info")]
    [Description("Whether to log debug information about scene transitions and fast travel. This is meant for troubleshooting, and has no effect on the in-game behavior.")]
    public bool LogDebugInfo = false;
}
