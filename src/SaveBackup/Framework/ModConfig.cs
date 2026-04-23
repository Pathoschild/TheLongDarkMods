using ModSettings;

namespace Pathoschild.TheLongDarkMods.SaveBackup.Framework;

/// <summary>The mod config model.</summary>
internal class ModConfig : JsonModSettings
{
    /*********
    ** Accessors
    *********/
    [Name("Number of daily backups")]
    [Description("The number of daily backups to keep. Older backups are deleted automatically.")]
    [Slider(0, 30)]
    public int DailyBackupCount = 10;

    [Name("Number of hourly backups")]
    [Description("The number of hourly backups to keep. Older backups are deleted automatically.")]
    [Slider(0, 30)]
    public int HourlyBackupCount = 10;

    [Name("Include survival saves")]
    [Description("Whether to back up saves created in survival mode.")]
    public bool IncludeSurvival = true;

    [Name("Include Wintermute saves")]
    [Description("Whether to back up saves created in Wintermute.\n\nNote: Wintermute saves are only backed up when you launch the game, unless you add MelonLoader to the Wintermute version.")]
    public bool IncludeWintermute = true;
}
